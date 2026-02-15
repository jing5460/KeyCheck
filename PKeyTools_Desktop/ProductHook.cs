using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PKeyTools
{
    public static class ProductHook
    {
        #region 全局变量（Hook实例/委托/模块句柄/调用计数器）
        private const int GET_PKEYDATA_HOOK_OFFSET = 0xA981;                  // Hook偏移：GetPKeyData+0xA981（对应sub_7BBCA981）
        private static IHook<HookTargetFuncDelegate> _hookTargetFunc; // Hook实例（4.3.3泛型版）
        private static DelegateGetPKeyData _nativeGetPKeyData;        // GetPKeyData原生委托
        private static IntPtr _hModule = IntPtr.Zero;                 // 目标DLL模块句柄
        private static int _hookCallCount = 0;                        // Hook调用计数器（线程安全）
        public static string CurrentConfigID = "";
        #endregion

        #region GetPKeyData原生函数委托（StdCall，无修改）
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private delegate int DelegateGetPKeyData(
            string ProductKey,
            string PkeyConfigPath,
            string MPCID,
            string pwszPKeyAlgorithm,
            IntPtr OemId,
            IntPtr OtherId,
            out string IID,
            out string Description,
            out string channel,
            out string subType,
            StringBuilder PID
        );
        #endregion

        #region sub_7BBCA981 Hook委托（添加FunctionAttribute，适配__fastcall）
        // 保留UnmanagedFunctionPointer，适配.NET Marshal封送
        [Function(CallingConventions.Fastcall)]
        private delegate int HookTargetFuncDelegate(int a1, int a2); // 原生：int __fastcall sub_7BBCA981(int a1, int a2)
        #endregion

        public static string MainCheckFunc(string ProductKey,string ConfigPath,string targetDLL)
        {
            try
            {
                CurrentConfigID="";
                // 步骤1：加载目标DLL，获取模块句柄
                _hModule = LoadLibrary(targetDLL);
                if (_hModule == IntPtr.Zero)
                {
                    //PrintError($"加载{TARGET_DLL}失败", Marshal.GetLastWin32Error());
                    //C:\home\site\wwwroot\ProductKeyUtilities.dll
                    return "";
                }
                //Debug.WriteLine($"✅ 加载{TARGET_DLL}成功，模块基址：0x{_hModule.ToString("X8")}");

                // 步骤2：获取GetPKeyData导出函数地址，封送为C#委托
                IntPtr getPKeyDataAddr = GetProcAddress(_hModule, "GetPKeyData");
                if (getPKeyDataAddr == IntPtr.Zero)
                {
                    //PrintError($"获取GetPKeyData地址失败", Marshal.GetLastWin32Error());
                    FreeLibrary(_hModule);
                    return "";
                }
                _nativeGetPKeyData = Marshal.GetDelegateForFunctionPointer<DelegateGetPKeyData>(getPKeyDataAddr);
                //Debug.WriteLine($"✅ 获取GetPKeyData地址成功：0x{getPKeyDataAddr.ToString("X8")}");

                // 步骤3：【核心修复】计算正确Hook地址（GetPKeyData函数地址 + 偏移，而非模块基址+偏移）
                IntPtr hookTargetAddr = IntPtr.Add(_hModule, GET_PKEYDATA_HOOK_OFFSET);
                //Debug.WriteLine($"✅ 计算Hook目标地址成功sub_7BBCA981 [GetPKeyData+{GET_PKEYDATA_HOOK_OFFSET:X4}]：0x{hookTargetAddr.ToString("X8")}");

                // 步骤4：创建并启用Hook（Reloaded.Hooks 4.3.3标准写法，无修改）
                var hookFactory = new ReloadedHooks();
                _hookTargetFunc = hookFactory.CreateHook<HookTargetFuncDelegate>(
                    HookedGetPKeyData_981,
                    hookTargetAddr.ToInt64()
                );
                _hookTargetFunc.Activate();
                //Debug.WriteLine($"✅ sub_7BBCA981 Hook启用成功，等待调用触发...\n");

                // 步骤5：调用GetPKeyData原生函数，触发Hook拦截（无修改）
                CallGetPKeyData(ProductKey,ConfigPath);

                // 步骤6：卸载Hook，恢复原生函数逻辑（4.3.3版本核心：Disable()）
                _hookTargetFunc?.Disable();
                //Debug.WriteLine($"\n✅ Hook已禁用，恢复原生sub_7BBCA981执行逻辑");

                return CurrentConfigID;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"❌ 程序执行异常：{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // 最终释放所有非托管资源（优化：避免重复Disable()）
                if (_hookTargetFunc != null)
                {
                    try { _hookTargetFunc.Disable(); } catch { }
                }
                if (_hModule != IntPtr.Zero) FreeLibrary(_hModule);
                //Debug.WriteLine($"\n✅ 所有非托管资源已释放，程序执行完成");
            }

            //Debug.WriteLine("Finished...");

            return "";
        }
        static int count = 0;
        #region Hook拦截函数（匹配2个int参数）
        private static int HookedGetPKeyData_981(int a1, int a2)
        {
            IntPtr pA1 = (IntPtr)a1;
            IntPtr pA2 = (IntPtr)a2;

            // 1️⃣ 直接检测 a1
            if (TryPrintIfMatch(pA1, "a1 (ECX)"))
                return _hookTargetFunc.OriginalFunction(a1, a2);

            // 2️⃣ 检测 a2
            if (TryPrintIfMatch(pA2, "a2 (EDX)"))
                return _hookTargetFunc.OriginalFunction(a1, a2);

            // 3️⃣ 检测 a1 + 0x14
            if (pA1 != IntPtr.Zero)
            {
                try
                {
                    IntPtr pExt = Marshal.ReadIntPtr(pA1, 0x14);
                    if (TryPrintIfMatch(pExt, "a1 + 0x14"))
                        return _hookTargetFunc.OriginalFunction(a1, a2);
                }
                catch { }
            }

            // 4️⃣ 什么都没命中 → 静默放行
            return _hookTargetFunc.OriginalFunction(a1, a2);
        }
        private static bool TryPrintIfMatch(IntPtr ptr, string tag)
        {
            if (ptr == IntPtr.Zero)
                return false;

            // 地址基本合法性校验（32位）
            long addr = ptr.ToInt64();
            if (addr < 0x10000 || addr > 0x7FFFFFFF)
                return false;

            // 内存可读性探测
            if (IsBadReadPtr(ptr, 2))
                return false;

            if (!TryReadUnicodeString(ptr, 256, out var str))
                return false;

            if (string.IsNullOrEmpty(str))
                return false;

            if (str.Contains("msft2005", StringComparison.OrdinalIgnoreCase)||str.Contains("msft2009", StringComparison.OrdinalIgnoreCase))
            {
                //Debug.WriteLine("======================================");
                //Debug.WriteLine($"🔥 命中关键字符串：{key}");
                //Debug.WriteLine($"📍 来源：{tag}");
                //Debug.WriteLine($"📌 地址：0x{ptr.ToString("X8")}");
                string clean = TrimToReadableUnicode(str);
                //Debug.WriteLine($"🧾 内容：{clean}");
                CurrentConfigID=clean;
                //Debug.WriteLine($"📏 字符串长度：{clean.Length}");
                //Debug.WriteLine("======================================\n");
                return true;
            }

            return false;
        }
        private static string TrimToReadableUnicode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                // 合法可读字符范围
                if (c == '\0')
                    break;

                if (c >= 0x20 && c <= 0x7E ||   // ASCII
                    c >= 0x4E00 && c <= 0x9FFF) // CJK
                {
                    sb.Append(c);
                }
                else
                {
                    break; // 一旦进入二进制，直接截断
                }
            }

            return sb.ToString();
        }
        private static bool TryReadUnicodeString(
    IntPtr ptr,
    int maxChars,
    out string result)
        {
            result = null;

            if (ptr == IntPtr.Zero)
                return false;

            long addr = ptr.ToInt64();
            if (addr < 0x10000 || addr > 0x7FFFFFFF)
                return false;

            var sb = new StringBuilder();

            for (int i = 0; i < maxChars; i++)
            {
                IntPtr cur = IntPtr.Add(ptr, i * 2);

                // 每次只探测 2 字节（一个 WCHAR）
                if (IsBadReadPtr(cur, 2))
                    break;

                char c;
                try
                {
                    c = (char)Marshal.ReadInt16(cur);
                }
                catch
                {
                    break;
                }

                if (c == '\0')
                    break;

                sb.Append(c);
            }

            if (sb.Length == 0)
                return false;

            result = sb.ToString();
            return true;
        }


        #endregion

        #region GetPKeyData调用逻辑
        private static void CallGetPKeyData(string productKey,string configPath)
        {
            //Debug.WriteLine("==================== 开始调用GetPKeyData ====================");
            StringBuilder pidSb = new StringBuilder(512);
            string iid = null, description = null, channel = null, subType = null;

            if (!File.Exists(configPath))
            {
                //Debug.WriteLine($"❌ 配置文件不存在：{configPath}");
                //Debug.WriteLine($"提示：请将pkconfig_winNext.xrm-ms放在程序运行目录下");
                return;
            }

            try
            {
                int retCode = _nativeGetPKeyData(
                    productKey, configPath, null, null, IntPtr.Zero, IntPtr.Zero,
                    out iid, out description, out channel, out subType, pidSb
                );

                if (retCode == 0)
                {
                    //Debug.WriteLine("✅ GetPKeyData调用成功，结构化数据如下：");
                    //Debug.WriteLine($"产品密钥：{productKey}");
                    //Debug.WriteLine($"IID唯一标识：{iid ?? "空"}");
                    //Debug.WriteLine($"密钥描述：{description ?? "空"}");
                    //Debug.WriteLine($"密钥通道：{channel ?? "空"}");
                    //Debug.WriteLine($"密钥子类型：{subType ?? "空"}");
                    //Debug.WriteLine($"PID标识码：{pidSb.ToString() ?? "空"}");
                }
                else
                {
                    //PrintError($"GetPKeyData调用失败，返回码", retCode);
                    //PrintError($"系统底层错误码", Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"❌ 调用GetPKeyData异常：{ex.Message}");
            }
            finally
            {
                FreeNativeOutString(iid);
                FreeNativeOutString(description);
                FreeNativeOutString(channel);
                FreeNativeOutString(subType);
            }
            //Debug.WriteLine("===============================================================");
        }
        #endregion

        #region 辅助方法
        private static void ExtractUnicodeData(IntPtr ptr, string desc)
        {
            // 1. 空指针直接返回
            if (ptr == IntPtr.Zero)
            {
                //Debug.WriteLine($"{desc}：指针为空（IntPtr.Zero）");
                return;
            }

            // 2. 32位地址范围校验
            if ((long)ptr > 0x7FFFFFFF || (long)ptr < 0x00010000)
            {
                //Debug.WriteLine($"{desc}：指针地址无效（0x{ptr.ToString("X8")}），不在32位有效内存范围");
                return;
            }

            // 3. 尝试用系统API探测内存是否可读
            if (!IsMemoryReadable(ptr, 2)) // 先探测2字节（1个Unicode字符）
            {
                //Debug.WriteLine($"{desc}：内存不可读（0x{ptr.ToString("X8")}），跳过读取");
                return;
            }

            // 4. 安全读取字符串（仅在内存可读时执行）
            string unicodeStr = null;
            try
            {
                unicodeStr = Marshal.PtrToStringUni(ptr, 512);
            }
            catch
            {
                //Debug.WriteLine($"{desc}：读取失败，可能为非Unicode数据");
                return;
            }

            // 5. 处理结果
            if (string.IsNullOrEmpty(unicodeStr))
            {
                //Debug.WriteLine($"{desc}：空字符串或非Unicode数据");
            }
            else
            {
                string showStr = unicodeStr.Substring(0, Math.Min(unicodeStr.Length, 256));
                //Debug.WriteLine($"{desc}：{showStr}");
                //Debug.WriteLine($"{desc}内存地址：0x{ptr.ToString("X8")}");
            }
        }

        // 辅助函数：用系统API探测内存是否可读
        [DllImport("kernel32.dll")]
        private static extern bool IsBadReadPtr(IntPtr lp, uint ucb);
        private static bool IsMemoryReadable(IntPtr ptr, int size)
        {
            return !IsBadReadPtr(ptr, (uint)size);
        }

        private static bool IsContainTargetStr(IntPtr ptr, string target)
        {
            if (ptr == IntPtr.Zero || string.IsNullOrEmpty(target)) return false;
            try
            {
                string unicodeStr = Marshal.PtrToStringUni(ptr);
                return unicodeStr != null && unicodeStr.Contains(target);
            }
            catch
            {
                return false;
            }
        }
        private static void FreeNativeOutString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                try
                {
                    IntPtr strPtr = Marshal.StringToHGlobalUni(str);
                    Marshal.FreeCoTaskMem(strPtr);
                }
                catch { }
            }
        }

        private static void PrintError(string msg, int errorCode)
        {
            //Debug.WriteLine($"❌ {msg}：0x{errorCode:X8}（十进制：{errorCode}）");
        }
        #endregion

        #region Kernel32.dll API导入
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        #endregion
    }
}