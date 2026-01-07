using Native;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Xml;
using System.Xml.Serialization;
using static PKeyTools.PKeyConfigModel;
using static PKeyTools.ProductKeyBaseModel;

namespace PKeyTools
{
    public class UseA
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(UseA),
            new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if (true.Equals(e.OldValue))
                {
                    GetUseA(window)?.Detach();
                    window.ClearValue(UseAProperty);
                }
                if (true.Equals(e.NewValue))
                {
                    var blur = new UseA();
                    blur.Attach(window);
                    window.SetValue(UseAProperty, blur);
                }
            }
        }

        public static readonly DependencyProperty UseAProperty = DependencyProperty.RegisterAttached(
            "UseA", typeof(UseA), typeof(UseA),
            new PropertyMetadata(null, OnUseAChanged));

        public static void SetUseA(DependencyObject element, UseA value)
        {
            element.SetValue(UseAProperty, value);
        }

        public static UseA GetUseA(DependencyObject element)
        {
            return (UseA)element.GetValue(UseAProperty);
        }

        private static void OnUseAChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                (e.OldValue as UseA)?.Detach();
                (e.NewValue as UseA)?.Attach(window);
            }
        }

        private Window _window;

        private void Attach(Window window)
        {
            _window = window;
            var source = (HwndSource)PresentationSource.FromVisual(window);
            if (source == null)
            {
                window.SourceInitialized += OnSourceInitialized;
            }
            else
            {
                AttachCore();
            }
        }

        private void Detach()
        {
            try
            {
                DetachCore();
            }
            finally
            {
                _window = null;
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            ((Window)sender).SourceInitialized -= OnSourceInitialized;
            AttachCore();
        }

        private void AttachCore()
        {
            EnableBlur(_window);
        }

        private void DetachCore()
        {
            _window.SourceInitialized += OnSourceInitialized;
        }

        private static void EnableBlur(Window window)
        {
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
    }

    public class FileTrans
    {
        public static string PAAA(string t)
        {
            byte[] data = Encoding.Default.GetBytes(t);
            string r = Convert.ToBase64String(data);
            data = Encoding.Default.GetBytes(r);
            r = Convert.ToBase64String(data);

            string a = r.Substring(10, 19);
            r = r.Remove(10, 19);
            a += r;
            return a;
        }
        public static string PCCC(string t)
        {
            if (string.IsNullOrWhiteSpace(t))
            {
                return "";
            }
            if (!t.Contains("="))
            {
                return t;
            }

            try
            {
                string a = t.Substring(0, 19);
                t = t.Remove(0, 19);
                t = t.Insert(10, a);

                byte[] b = Convert.FromBase64String(t);
                t = Encoding.Default.GetString(b);
                b = Convert.FromBase64String(t);
                t = Encoding.Default.GetString(b);
            }
            catch (Exception)
            {

            }

            return t;
        }
    }
    /*
         public class ABC
        {
            public static string keyFile = Directory.GetCurrentDirectory().Replace("bin\\Debug\\net8.0-windows", "").Replace("bin\\Release\\net8.0-windows", "") + "\\Data\\";
            public static List<FileTrans> SelectBig(XmlNode keyRoot, XmlDocument xmlDocument, string value)
            {
                int[] keysCount = new int[5];
                keyRoot = xmlDocument.SelectSingleNode(value);
                for (int aa = 0; aa < 5; aa++)
                {
                    keysCount[aa] = keyRoot.ChildNodes[aa].ChildNodes.Count;
                }

                string value2 = "";
                List<FileTrans> list = new List<FileTrans>();
                for (int i = 0; i < 5; i++)
                {
                    switch (i)
                    {
                        case 0:
                            value2 = "//Office-Keys";
                            break;
                        case 1:
                            value2 = "//Windows-Keys";
                            break;
                        case 2:
                            value2 = "//Server-Keys";
                            break;
                        case 3:
                            value2 = "//VS-Keys";
                            break;
                        case 4:
                            value2 = "//Other-Keys";
                            break;
                    }
                    for (int aa = 1; aa <= keysCount[i]; aa++)
                    {
                        keyRoot = xmlDocument.SelectSingleNode(value + value2 + "//Keys[" + aa + "]");
                        list.Add(new FileTrans
                        {
                            A = keyRoot.Attributes[3].InnerText,
                            B = FileTrans.PCCC(keyRoot.Attributes[2].InnerText),
                            C = keyRoot.Attributes[1].InnerText,
                            D = keyRoot.Attributes[0].InnerText
                        }); ;
                    }
                }

                var res = from s in list orderby s.C descending group s by s.A into ss select ss;

                List<FileTrans> list2 = new List<FileTrans>();
                foreach (IGrouping<string, FileTrans> group in res)
                {
                    foreach (FileTrans fileTrans in group)
                    {
                        list2.Add(new FileTrans
                        {
                            A = fileTrans.A,
                            B = fileTrans.B,
                            C = fileTrans.C,
                            D = fileTrans.D
                        }); ;
                    }
                }

                return list2;
            }
        }
     */

    public class KeySelectModel
    {
        public string Name { get; set; }
        public string ShowName { get; set; }
        /// <summary>
        /// 该产品名称的密钥数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 0：所有授权类型（一级）；1：单个授权类型（二级）；2：产品名称（三级）；
        /// </summary>
        public int ItemIndex { get; set; }
        /// <summary>
        /// 当前产品名称属于哪种授权类型，仅当ItemIndex=2时使用该参数
        /// </summary>
        public string LicenseType { get; set; }
    }

    public class PKeyConfigModel
    {
        /// <summary>
        /// 表示 PkeyData.xml 的根节点
        /// </summary>
        [XmlRoot("xmldoc")]
        public class PkeyData
        {
            /// <summary>
            /// 所有证书类的集合
            /// </summary>
            [XmlElement("configType")]
            public List<ConfigType> ConfigTypes { get; set; }
        }

        /// <summary>
        /// 表示单个配置类型项
        /// </summary>
        public class ConfigType
        {
            /// <summary>
            /// 证书的唯一标识符
            /// </summary>
            [XmlAttribute("configID")]
            public int ConfigID { get; set; }

            /// <summary>
            /// 证书文件的路径
            /// </summary>
            [XmlAttribute("configPath")]
            public string ConfigPath { get; set; }

            /// <summary>
            /// 证书的名称
            /// </summary>
            [XmlAttribute("configName")]
            public string ConfigName { get; set; }
        }

    }

    public static class GlobalParameters
    {
        /// <summary>
        /// 证书列表
        /// </summary>
        public static string configFile = Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0-windows", "").Replace("bin\\Release\\net8.0-windows", "") ,"PKeyConfig\\PkeyData.xml");
        public static string configFolder = Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0-windows", "").Replace("bin\\Release\\net8.0-windows", ""), "PKeyConfig");
        public static string GetConfigFilePath()
        {
            var configstr = @"<xmldoc> <!--上半区--> <configType configID=""2"" configPath=""Windows 10\pkeyconfig_26200.xrm-ms"" configName=""Windows 11 Build 26200 +"" /> <configType configID=""29"" configPath=""Windows 10\EnterpriseS_2024.xrm-ms"" configName=""Windows 11 LTSC 2024"" /> <configType configID=""1"" configPath=""Windows 10\pkeyconfig.xrm-ms"" configName=""Windows 10 / WinSvr 16 - 19 / Build 19043"" /> <configType configID=""3"" configPath=""Windows 10\PreBuild_20348.xrm-ms"" configName=""Windows 11 / WinSvr 19 - 22 / Build 20348"" /> <configType configID=""4"" configPath=""Windows 10\pkeyconfigLTSC.xrm-ms"" configName=""Windows 10 / Build 19044"" /> <configType configID=""5"" configPath=""Windows 10\pkeyconfig_1909.xrm-ms"" configName=""Windows 10 / Build 17763"" /> <configType configID=""6"" configPath=""Windows 10\16299-pkeyconfig.xrm-ms"" configName=""Windows 10 / WinSvr 16 / Build 16299"" /> <configType configID=""7"" configPath=""Windows 10\pkconfig_win10.xrm-ms"" configName=""Windows 10 Old"" /> <configType configID=""8"" configPath=""Windows 10\pkeyconfig-csvlk.xrm-ms"" configName=""Windows 10 CSVLK"" /> <configType configID=""9"" configPath=""Windows8 8.1 emb\pkconfig_win8.1.xrm-ms"" configName=""Windows 8.1 / WinSvr 2012 R2 / WinEmbedded 8.1"" /> <configType configID=""10"" configPath=""Windows8 8.1 emb\pkconfig_win8.1-csvlk.xrm-ms"" configName=""Windows 8.1 CSVLK"" /> <configType configID=""11"" configPath=""Windows8 8.1 emb\pkconfig_winemb8.xrm-ms"" configName=""Windows 8 / WinSvr 2012 / WinEmbedded 8"" /> <configType configID=""12"" configPath=""Office All\pkeyconfig-office_19+.xrm-ms"" configName=""Office 2019 + / 365"" /> <configType configID=""13"" configPath=""Office All\pkeyconfig-office16Client.xrm-ms"" configName=""Office 2016"" /> <configType configID=""15"" configPath=""Office All\pkconfig_office15.xrm-ms"" configName=""Office 2013"" /> <!--下半区--> <configType configID=""14"" configPath=""Office All\pkeyconfig-Office16KMSHost.xrm-ms"" configName=""Office 2016 CSVLK"" /> <configType configID=""18"" configPath=""Office All\pkconfig_office14.xrm-ms"" configName=""Office 2010"" /> <configType configID=""17"" configPath=""Old Windows\pkconfig_win7.xrm-ms"" configName=""Windows 7 / WinSvr 2008 R2"" /> <configType configID=""16"" configPath=""Office All\pkconfig_Office15KMSHost-Backup.xrm-ms"" configName=""Office 2013 CSVLK"" /> <configType configID=""19"" configPath=""Old Windows\pkconfig_vista.xrm-ms"" configName=""Windows Vista / WinSvr 2008"" /> <configType configID=""20"" configPath=""Old Windows\pkconfig_winThinPC.xrm-ms"" configName=""Thin PC"" /> <configType configID=""21"" configPath=""Old Windows\pkconfig_winPosReady7.xrm-ms"" configName=""Windows PosReady 7"" /> <configType configID=""30"" configPath=""VS\vs2022.xrm-ms"" configName=""VS 2022"" /> <configType configID=""22"" configPath=""VS\vs2019.xrm-ms"" configName=""VS 2019"" /> <configType configID=""23"" configPath=""VS\vs2017.xrm-ms"" configName=""VS 2017"" /> <configType configID=""24"" configPath=""VS\vs2015.xrm-ms"" configName=""VS 2015"" /> <configType configID=""25"" configPath=""VS\vs2015rc.xrm-ms"" configName=""VS 2015 RC"" /> <configType configID=""26"" configPath=""VS\vs2013.xrm-ms"" configName=""VS 2013"" /> <configType configID=""27"" configPath=""VS\vs2012.xrm-ms"" configName=""VS 2012"" /> <configType configID=""28"" configPath=""VS\vs2010.xrm-ms"" configName=""VS 2010"" /> </xmldoc>";
            if(!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
                File.WriteAllText(configFile, configstr);
            }
            else if(!File.Exists(configFile))
            {
                File.WriteAllText(configFile, configstr);
            }

            return configFile;
        }

        public static PkeyData GetPkeyData()
        {
            try
            {
                return (PkeyData)new XmlSerializer(typeof(PkeyData)).Deserialize(new StringReader(File.ReadAllText(GetConfigFilePath())));
            }
            catch(Exception)
            {

            }

            return null;
        }


        /// <summary>
        /// 密钥数据库
        /// </summary>
        public static string keybaseFile = Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0-windows", "").Replace("bin\\Release\\net8.0-windows", "") , "Data\\Keybase.xml");
        public static string keybaseConfigFolder = Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0-windows", "").Replace("bin\\Release\\net8.0-windows", ""), "Data");
        public static string GetKeyBaseFilePath()
        {
            var configstr = @"<xmldoc>
  <MAK-License>
    <Office-Keys>
      <Keys ConfigID="""" EPID="""" ActiveCount=""0"" ProductKey=""RXVTFTTkZndE9GSTBWMVWs0elVWRXG90V0RjelRWSXRWMFpLUzFrPQ=="" ProductName=""Office19_RTM19_Standard2019VL_MAK_AE"" />
    </Office-Keys>
    <Windows-Keys>
      <Keys ConfigID="""" EPID=""05426-03312-030-583134-03-2052-9200.0000-2322019"" ActiveCount=""0"" ProductKey=""RNazVYUzFrdFNFTlpVaV1ZkQ05FSXlF0U2xGUlNqWXRWMWhEUzAwPQ=="" ProductName=""Win 10 RTM Professional Volume:MAK"" />
    </Windows-Keys>
    <Server-Keys>
      <Keys ConfigID="""" EPID=""05426-02526-013-042329-03-2052-9200.0000-2352019"" ActiveCount=""39"" ProductKey=""RVRGRPU2pjdE0xbFJUVVWpkUVFsQXEl0UWpOSFMwY3ROekpZT0ZjPQ=="" ProductName=""Windows Server 12 R2 RTM ServerStandard Volume:MAK"" />
    </Server-Keys>
    <VS-Keys />
    <Other-Keys>
      <Keys ConfigID="""" EPID=""05426-03702-000-147344-03-2052-9200.0000-2342019"" ActiveCount=""---"" ProductKey=""RSbEE1V0ZZdE5raFFNMV0RKT1ZFc3VF0UkZBNFYwY3RORTAyV0ZjPQ=="" ProductName=""Not Found"" />
    </Other-Keys>
  </MAK-License>
  <Retail-License>
    <Office-Keys>
      <Keys ConfigID="""" EPID=""05426-02192-312-841244-00-2052-9200.0000-2752019"" ActiveCount=""---"" ProductKey=""RWbGxFTmswdFdWbEdORUjBSWFRsa3Gt0TkRSVU4xY3ROazFST0VJPQ=="" ProductName=""Office15_VisioProR_Retail"" />
    </Office-Keys>
    <Windows-Keys>
      <Keys ConfigID="""" EPID=""05426-03915-000-000002-02-2052-9200.0000-2582019"" ActiveCount=""---"" ProductKey=""RTRTR6U2tzdFJEWTVVRVmpJNU0xSXW90TkVReVNqY3RPRWhYTkVZPQ=="" ProductName=""Win 10 RTM ProfessionalWorkstation OEM:DM"" />
      </Windows-Keys>
    <Server-Keys>
      <Keys ConfigID="""" EPID=""05426-04290-010-341496-00-2052-9200.0000-2582019"" ActiveCount=""---"" ProductKey=""RRbEpPU0RrdFNGZEtVRUXpSWFJsZ3Ul0U2tkSVRVWXRSall6UkRNPQ=="" ProductName=""Windows Server 2019 RTM ServerStandard Retail"" />
       </Server-Keys>
    <VS-Keys />
    <Other-Keys>
      <Keys ConfigID="""" EPID=""05426-00098-182-001090-00-2052-9200.0000-2472019"" ActiveCount=""---"" ProductKey=""RSMGhaUkVNdFNGbEdWeUkRkU05rMHmt0T1VaS00xRXRVVGc1VWxnPQ=="" ProductName=""RTM_ProPlus_Retail2"" />
    </Other-Keys>
  </Retail-License>
  <KMS-License>
    <Office-Keys>
        <Keys ConfigID="""" EPID=""05426-04709-000-000000-03-2052-9200.0000-2492021"" ActiveCount=""---"" ProductKey=""RUa3BLT0VNdFIwSTJSRUmxoWlZFc3mN0TTBSWlVWUXROa1kzVkVnPQ=="" ProductName=""Office21_ProPlus2021VL_KMS_Client_AE"" />
    </Office-Keys>
    <Windows-Keys>
      <Keys ConfigID="""" EPID=""05426-00172-037-240421-03-2052-9200.0000-2502019"" ActiveCount=""---"" ProductKey=""RRelF5T0VJdE9VcENSRUjBaRVZGWXkl0UkVSUlIwWXRUVGRaVWpJPQ=="" ProductName=""Windows 7 All Volume Editions Volume:CSVLK"" />
  </Windows-Keys>
    <Server-Keys>
      <Keys ConfigID="""" EPID=""05426-04469-000-000000-03-2052-9200.0000-1372021"" ActiveCount=""---"" ProductKey=""RPVXMzVVRndFZqSTNReVGxSQ1ZqZ3ll0VFRKQ1ZGWXRTMGhOV0ZZPQ=="" ProductName=""Windows Server 2019 RTM ServerTurbine Volume:GVLK"" />
      </Server-Keys>
    <VS-Keys />
    <Other-Keys>
      <Keys ConfigID="""" EPID=""05426-00096-201-945544-03-2052-9200.0000-2542021"" ActiveCount=""---"" ProductKey=""RVa1pZU0ZRdFVsQXpVVTjFkU1RUUXlF0T1VRelVsUXRSbEkyTjBZPQ=="" ProductName=""RTM_ProPlus_KMS_Host"" />
    </Other-Keys>
  </KMS-License>
  <Other-License>
      <Keys />
  </Other-License>
</xmldoc>";         
            if(!Directory.Exists(keybaseConfigFolder))
            {
                Directory.CreateDirectory(keybaseConfigFolder);
                File.WriteAllText(keybaseFile, configstr);
            }
            else if(!File.Exists(keybaseFile))
            {
                File.WriteAllText(keybaseFile, configstr);
            }

            return keybaseFile;
        }

        public static KeyBase GetKeyBaseData()
        {
            try
            {
                return (KeyBase)new XmlSerializer(typeof(KeyBase)).Deserialize(new StringReader(File.ReadAllText(GetKeyBaseFilePath())));
            }
            catch(Exception)
            {

            }

            return null;
        }
        public static bool SaveKeyBaseFile(KeyBase keyBase)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    new XmlSerializer(typeof(KeyBase)).Serialize(ms, keyBase);
                    File.WriteAllText(GetKeyBaseFilePath(), Encoding.ASCII.GetString(ms.ToArray()));
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }

    public class  ProductKeyBaseModel
    {
        [XmlRoot("xmldoc")]
        public class KeyBase
        {
            [XmlElement("MAK-License")]
            public MAKLicense MAKLicense { get; set; }

            [XmlElement("Retail-License")]
            public RetailLicense RetailLicense { get; set; }

            [XmlElement("KMS-License")]
            public KMSLicense KMSLicense { get; set; }

            [XmlElement("Other-License")]
            public OtherLicense OtherLicense { get; set; }
        }
        public class MAKLicense
        {
            [XmlElement("Office-Keys")]
            public KeysCollection OfficeKeys { get; set; }

            [XmlElement("Windows-Keys")]
            public KeysCollection WindowsKeys { get; set; }

            [XmlElement("Server-Keys")]
            public KeysCollection ServerKeys { get; set; }

            [XmlElement("VS-Keys")]
            public KeysCollection VSKeys { get; set; }

            [XmlElement("Other-Keys")]
            public KeysCollection OtherKeys { get; set; }
        }
        public class RetailLicense
        {
            [XmlElement("Office-Keys")]
            public KeysCollection OfficeKeys { get; set; }

            [XmlElement("Windows-Keys")]
            public KeysCollection WindowsKeys { get; set; }

            [XmlElement("Server-Keys")]
            public KeysCollection ServerKeys { get; set; }

            [XmlElement("VS-Keys")]
            public KeysCollection VSKeys { get; set; }

            [XmlElement("Other-Keys")]
            public KeysCollection OtherKeys { get; set; }
        }
        public class KMSLicense
        {
            [XmlElement("Office-Keys")]
            public KeysCollection OfficeKeys { get; set; }

            [XmlElement("Windows-Keys")]
            public KeysCollection WindowsKeys { get; set; }

            [XmlElement("Server-Keys")]
            public KeysCollection ServerKeys { get; set; }

            [XmlElement("VS-Keys")]
            public KeysCollection VSKeys { get; set; }

            [XmlElement("Other-Keys")]
            public KeysCollection OtherKeys { get; set; }
        }
        public class OtherLicense:ProductKeysCollection
        {

        }
        public class KeysCollection:ProductKeysCollection
        {

        }

        public abstract class ProductKeysCollection
        {
            [XmlElement("Keys")]
            public List<Key> Keys { get; set; }
        }
        public class Key
        {
            [XmlAttribute("ConfigID")]
            public string ConfigID { get; set; }

            [XmlAttribute("EPID")]
            public string EPID { get; set; }

            [XmlAttribute("ActiveCount")]
            public string ActiveCount { get; set; }

            [XmlAttribute("ProductKey")]
            public string ProductKey { get; set; }

            [XmlAttribute("ProductName")]
            public string ProductName { get; set; }
            [XmlAttribute("LicenseTypeTag")]
            public string LicenseTypeTag { get; set; }
            [XmlAttribute("ProductTypeTag")]
            public string ProductTypeTag { get; set; }
        }
    }
}

namespace Native
{
    internal enum AccentState
    {
        ACCENT_DISABLED,
        ACCENT_ENABLE_GRADIENT,
        ACCENT_ENABLE_TRANSPARENTGRADIENT,
        ACCENT_ENABLE_BLURBEHIND,
        ACCENT_INVALID_STATE,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // 省略其他未使用的字段
        WCA_ACCENT_POLICY = 19,
        // 省略其他未使用的字段
    }
}

