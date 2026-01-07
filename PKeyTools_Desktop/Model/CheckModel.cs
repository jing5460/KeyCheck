using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using static PKeyTools.Model.AllModel;
using static PKeyTools.Model.GetErrorCodeByPost;
using static PKeyTools.PKeyConfigModel;

namespace PKeyTools.Model
{
    public class CheckModel
    {
        public const string VideoCheckUA = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
        public static string KeyFormat = @"([BCDFGHJKMPQRTVWXY2-9N]{5}\-){4}[BCDFGHJKMPQRTVWXY2-9N]{4}[BCDFGHJKMPQRTVWXY2-9]";

        public string rootPath;
        public List<ConfigType> configList = new List<ConfigType>();

        public int CheckOptionOEM = 0;
        public int SkipBlockDBCheck = 0;
        public int GetSimpIfm = 0;
        public int CheckMAKErrorCode = 0;
        /// <summary>
        /// 初始化构造函数，带参数
        /// </summary>
        /// <param name="pkeyConfigList">检测的证书列表</param>
        /// <param name="projRootPath">PKeyConfig证书文件夹的上级目录</param>
        public CheckModel(List<ConfigType> pkeyConfigList,string projRootPath)
        {
            configList=pkeyConfigList;
            rootPath=projRootPath;
        }
        /// <summary>
        /// 初始化构造函数，无参数
        /// </summary>
        public CheckModel()
        {

        }

        /// <summary>
        /// 检测密钥的接口 （Client专用）
        /// </summary>
        /// <param name="k">密钥</param>
        /// <param name="coem">检测OEM密钥的计数</param>
        /// <param name="skipdb">跳过连接被封密钥数据库</param>
        /// <param name="getsimp">仅获取密钥的数据，不检测次数</param>
        /// <param name="makcode">0为 不检测MAK密钥的错误代码；1为 检测MAK密钥的错误代码</param>
        /// <param name="cindex">证书索引。0为 遍历所有证书；其他数值为 指定证书ID</param>
        /// <returns></returns>
        public async Task<List<CheckResultModel>> CheckKeysClient(string k, int coem = 0, int skipdb = 0, int getsimp = 0, int makcode = 0, int cindex = 0)
        {
            List<CheckResultModel> resultList = new List<CheckResultModel>();
            if (string.IsNullOrWhiteSpace(k))
            {
                resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "参数错误！" });

                return resultList;
            }

            CheckOptionOEM = coem;
            SkipBlockDBCheck = skipdb;
            GetSimpIfm = getsimp;
            CheckMAKErrorCode = makcode;

            k=k.ToUpper().Replace(" ", "").Replace("\0", "");

            int keynummode = 0;

            MatchCollection abc = Regex.Matches(k, KeyFormat);
            if (abc.Count > 0)
            {
                if (abc.Count == 1)
                {
                    keynummode=0;

                    CheckResultModel checkResult = await Checking(abc[0].Value, cindex);
                    if (checkResult == null)
                    {
                        resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "ERROR!" });

                        return resultList;
                    }
                    resultList.Add(checkResult);

                    return resultList;
                }
                else if (abc.Count > 50)
                {
                    keynummode=1;

                    resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "一次不能检测超过50个Key" });
                    return resultList;
                }
                else
                {
                    keynummode=1;

                    for (int i = 0; i < abc.Count; i++)
                    {
                        resultList.Add(await Checking(abc[i].Value, cindex));
                    }

                    return resultList;
                }

            }
            else
            {
                resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "密钥长度不正确或输入的内容里不包含密钥！！" });

                return resultList;
            }
        }

        private async Task<CheckResultModel> Checking(string key, int cindex)
        {
            //连接已封密钥数据库进行检查
            if (SkipBlockDBCheck==0)
            {
                //try
                //{
                //    BlockedKeys blresult =
                //    if (blresult!=null)
                //    {
                //        return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="已从 被封密钥数据库 中检测到该密钥已被封 Blocked ,第一次检测时间："+blresult.C };
                //    }
                //}
                //catch (Exception)
                //{

                //}

            }

            //遍历所有证书
            if (cindex==0)
            {
                CheckResultModel checkData = null;
                //快速跳转到对应索引，指使用PKEY2005算法的
                if (!key.Contains("N"))
                {
                    bool isContinueSearch = false;
                    for (int conindex = configList.FindIndex(p => p.ConfigID == 14); conindex <configList.Count; conindex++)
                    {
                        //string configpath = rootPath+"pkeyconfig/"+configList[i].ConfigPath;
                        string configpath = Path.Combine(rootPath, configList[conindex].ConfigPath);
                        string configname = configList[conindex].ConfigName;

                        checkData=await CheckKeysFunc(key, configpath, configname);
                        if (checkData!=null)
                        {
                            break;
                        }

                        if (conindex == configList.Count - 1)
                        {
                            isContinueSearch= true;
                        }
                    }

                    if (isContinueSearch)
                    {
                        for (int conindex = 0; conindex <configList.FindIndex(p => p.ConfigID == 14); conindex++)
                        {
                            //string configpath = rootPath+"pkeyconfig/"+configList[i].ConfigPath;
                            string configpath = Path.Combine(rootPath, configList[conindex].ConfigPath);
                            string configname = configList[conindex].ConfigName;

                            checkData=await CheckKeysFunc(key, configpath, configname);
                            if (checkData!=null)
                            {
                                break;
                            }

                            if (conindex == configList.Count - 1)
                            {
                                return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="密钥错误或者证书不匹配!" };
                            }
                        }
                    }

                }
                else
                {
                    for (int conindex = 0; conindex <configList.Count; conindex++)
                    {
                        //string configpath = rootPath+"pkeyconfig/"+configList[i].ConfigPath;
                        string configpath = Path.Combine(rootPath,  configList[conindex].ConfigPath);
                        string configname = configList[conindex].ConfigName;

                        checkData=await CheckKeysFunc(key, configpath, configname);
                        if (checkData!=null)
                        {
                            break;
                        }

                        if (conindex == configList.Count - 1)
                        {
                            return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="密钥错误或者证书不匹配!" };
                        }
                    }
                }

                //反馈检测结果
                return checkData;

                /*
                                 if (!key.Contains("N"))
                {
                    int groupSize = 15;
                    var groups = configList
                        .Select((v, i) => new { v, i })
                        .GroupBy(x => x.i / groupSize)
                        .Select(g => g.Select(x => x.v).ToList())
                        .ToList();

                    var cts = new CancellationTokenSource();
                    var token = cts.Token;

                    var tasks = groups.Select(group => Task.Run( async() =>
                    {
                        foreach (var item in group)
                        {
                            if (token.IsCancellationRequested) break;

                            string configpath = Path.Combine(rootPath, "pkeyconfig", item.ConfigPath);
                            string configname = item.ConfigName;

                            var tckData=await CheckKeysFunc(key, configpath, configname);
                            if (tckData!=null)
                            {
                                checkData=tckData;
                                cts.Cancel(); // 通知其他线程结束
                                break;
                            }
                        }
                    }, token)).ToList();

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (AggregateException)
                    {
                        // 有任务被取消时会抛出，忽略即可
                    }

                    if (checkData==null)
                    {
                        return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="密钥错误或者证书不匹配!" };
                    }

                }
                 */
            }
            else if (cindex==900)
            {
                CheckResultModel checkData = null;
                for (int conindex = 0; conindex <configList.Count; conindex++)
                {
                    List<int> checkID = new List<int>() { 12, 13, 14, 15, 16, 18 };
                    if (checkID.Where(p => p==configList[conindex].ConfigID).Count()>0)
                    {
                        string configpath = Path.Combine(rootPath,  configList[conindex].ConfigPath);
                        string configname = configList[conindex].ConfigName;

                        checkData=await CheckKeysFunc(key, configpath, configname);
                        if (checkData!=null)
                        {
                            break;
                        }
                    }

                    if (conindex == configList.Count - 1)
                    {
                        return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="当前密钥可能不是Office的产品密钥，请尝试更换其他检测项。" };
                    }
                }
                //反馈检测结果
                return checkData;
            }
            else if (cindex==901)
            {
                CheckResultModel checkData = null;
                for (int conindex = 0; conindex <configList.Count; conindex++)
                {
                    List<int> checkID = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 17, 19, 20, 21, 29 };
                    if (checkID.Where(p => p==configList[conindex].ConfigID).Count()>0)
                    {
                        string configpath = Path.Combine(rootPath,  configList[conindex].ConfigPath);
                        string configname = configList[conindex].ConfigName;

                        checkData=await CheckKeysFunc(key, configpath, configname);
                        if (checkData!=null)
                        {
                            break;
                        }
                    }

                    if (conindex == configList.Count - 1)
                    {
                        return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="当前密钥可能不是Windows的产品密钥，请尝试更换其他检测项。" };
                    }
                }
                //反馈检测结果
                return checkData;
            }
            else
            {
                //查找证书ID
                ConfigType config = configList.FirstOrDefault(p => p.ConfigID == cindex);
                if (config!=null)
                {
                    string configpath = Path.Combine(rootPath,config.ConfigPath);
                    string configname = config.ConfigName;

                    CheckResultModel checkData = await CheckKeysFunc(key, configpath, configname);
                    if (checkData==null)
                    {
                        return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="密钥错误或者证书不匹配!" };
                    }

                    return checkData;
                }
                else
                {
                    return new CheckResultModel() { ProductKey=key, ActiveCount= "", IsShowPopup=true, ShowMessage="无法通过提供的ID查找到证书!" };
                }

            }

        }

        [DllImport("pidgenx.dll", EntryPoint = "PidGenX", CharSet = CharSet.Auto)]
        private static extern int PidGenX(string ProductKey, string PkeyPath, string MSPID, string oemId, IntPtr ProductID, IntPtr DigitalProductID, IntPtr DigitalProductID4);

        private async Task<CheckResultModel> CheckKeysFunc(string ProductKey, string PKeyPath, string configName)
        {
            CheckResultModel checkResult = new CheckResultModel();
            checkResult.ProductKey=ProductKey;

            int RetID = 0;
            byte[] gpid = new byte[0x32];
            byte[] opid = new byte[0xA4];
            byte[] npid = new byte[0x04F8];

            IntPtr PID = Marshal.AllocHGlobal(0x32);
            IntPtr DPID = Marshal.AllocHGlobal(0xA4);
            IntPtr DPID4 = Marshal.AllocHGlobal(0x04F8);

            string MSPID = "03612";
            string oemId = null;

            gpid[0] = 0x32;
            opid[0] = 0xA4;
            npid[0] = 0xF8;
            npid[1] = 0x04;

            Marshal.Copy(gpid, 0, PID, 0x32);
            Marshal.Copy(opid, 0, DPID, 0xA4);
            Marshal.Copy(npid, 0, DPID4, 0x04F8);

            try
            {
                RetID = PidGenX(ProductKey, PKeyPath, MSPID, oemId, PID, DPID, DPID4);
            }
            catch (Exception)
            {
                //OpenPOOP("调用pidgenx.dll时出错！", false, false, 1);
                return null;
            }
            if (RetID == 0)
            {
                checkResult.KeyConfig=configName;
                ErrorCodeCheckResultModel eccheckresult = new ErrorCodeCheckResultModel();
                Marshal.Copy(PID, gpid, 0, gpid.Length);
                Marshal.Copy(DPID4, npid, 0, npid.Length);

                try
                {
                    if (ProductKey.Contains("N"))
                    {
                        ProductKeyDecoder pkey_data = new ProductKeyDecoder(ProductKey);
                        PKeyConfig pkc = new PKeyConfig(XElement.Parse(System.IO.File.ReadAllText(PKeyPath)));
                        try
                        {
                            checkResult.KeyProduN = pkc.ConfigForGroup(pkey_data.Group).Desc;
                        }
                        catch
                        {
                            checkResult.KeyProduN = "Not Found";
                        }
                    }
                    else
                    {
                        checkResult.KeyProduN = KeyCheckFunModel.GetProductDescription(PKeyPath, "{" + KeyCheckFunModel.GetStringFromID(npid, 0x0088) + "}", KeyCheckFunModel.GetStringFromID(npid, 0x0118));
                    }


                    string LicenseTypeV = KeyCheckFunModel.GetStringFromID(npid, 0x03F8);
                    checkResult.LicenseType = LicenseTypeV;

                    string partNum = checkResult.PartNum = KeyCheckFunModel.GetStringFromID(npid, 0x0378);

                    checkResult.KeyEdition = KeyCheckFunModel.GetStringFromID(npid, 0x0118);
                    if (checkResult.KeyEdition.Contains("EnterpriseS"))
                    {
                        if (partNum.Contains("TH"))
                        {
                            checkResult.KeyProduN += " (LTSB 2015)";
                        }
                        else if (partNum.Contains("RS1"))
                        {
                            checkResult.KeyProduN += " (LTSB 2016)";
                        }
                        else if (partNum.Contains("RS5"))
                        {
                            checkResult.KeyProduN += " (LTSC 2019)";
                        }
                        else if (partNum.Contains("Vb"))
                        {
                            checkResult.KeyProduN += " (LTSC 2021)";
                        }
                        else if (partNum.Contains("Ge"))
                        {
                            checkResult.KeyProduN += " (LTSC 2024)";
                        }
                    }
                    if (checkResult.KeyProduN.IndexOf("RTM") == 0)
                    {
                        checkResult.KeyProduN += " (Office 2010)";
                    }

                    if (checkResult.LicenseType.Contains("CSVLK")||checkResult.LicenseType.Contains("GVLK")||checkResult.KeyProduN.Contains("Visual Studio"))
                    {
                        checkResult.ActiveCount ="Valid";
                    }
                    else
                    {
                        //判断是否勾选 只获取基本信息
                        if (GetSimpIfm==0)
                        {
                            if (LicenseTypeV.Contains("Retail") || LicenseTypeV.Contains("OEM"))
                            {
                                if ((CheckOptionOEM==1) && LicenseTypeV.Contains("OEM"))
                                {
                                    checkResult.ActiveCount = KeyCheckFunModel.GetCount(KeyCheckFunModel.GetStringFromID(npid, 0x0008));
                                    //如果只获取OEM密钥的计数，就不会有ConfigID，这里手动获取一下
                                    checkResult.ConfigID=  eccheckresult.ConfigId=GetConfigID(ProductKey, PKeyPath);
                                }
                                else
                                {
                                    eccheckresult = await ConsumeKey(ProductKey, PKeyPath);
                                    checkResult.ActiveCount =eccheckresult.CheckResult=="" ? "---" : eccheckresult.CheckResult;
                                }
                            }
                            else
                            {
                                if (CheckMAKErrorCode==1&&checkResult.LicenseType.Contains("MAK"))
                                {
                                    eccheckresult = await ConsumeKey(ProductKey, PKeyPath);
                                    checkResult.ActiveCount =eccheckresult.CheckResult=="" ? "---" : eccheckresult.CheckResult;
                                }
                                else
                                {
                                    checkResult.ActiveCount = KeyCheckFunModel.GetCount(KeyCheckFunModel.GetStringFromID(npid, 0x0008));
                                    //批量密钥默认不检测错误代码，所以不会有ConfigID，这里手动获取一下
                                    checkResult.ConfigID=  eccheckresult.ConfigId=GetConfigID(ProductKey, PKeyPath);
                                }

                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    checkResult.ActiveCount="";
                    checkResult.IsShowPopup=true;
                    checkResult.ShowMessage="检测密钥次数或代码出错!";

                    //暂时不return
                }

                checkResult.ProductID = KeyCheckFunModel.GetStringFromID(gpid, 0x0000);
                checkResult.ePID = KeyCheckFunModel.GetStringFromID(npid, 0x0008);
                checkResult.KeySKU = KeyCheckFunModel.GetStringFromID(npid, 0x0088);
                checkResult.LicenseChannel = KeyCheckFunModel.GetStringFromID(npid, 0x0478);


                //判断是否勾选 只获取基本信息
                if (GetSimpIfm==0)
                {
                    if (checkResult.ActiveCount.Contains("Blocked")||checkResult.ActiveCount.Contains("0xC004C003")||checkResult.ActiveCount.Contains("0xC004C060"))
                    {
                        try
                        {
                            if (SkipBlockDBCheck==0)
                            {
                                //将已封密钥加入数据库

                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        if (SkipBlockDBCheck==1)
                        {
                            if (!checkResult.ActiveCount.Contains("-") && !checkResult.ActiveCount.Contains("Error") && checkResult.ActiveCount != "")
                            {
                                //将密钥从已封密钥数据库中移除
                            }
                        }
          
                    }

                }

            }
            else if (RetID == -2147024809)
            {
                checkResult.ActiveCount="";
                checkResult.IsShowPopup=true;
                checkResult.ShowMessage="参数错误!";

                //暂时不return
            }
            else if ((RetID == -1979645695) || (RetID == -1979645951))
            {
                return null;
            }
            else
            {
                return null;
            }
            Marshal.FreeHGlobal(PID);
            Marshal.FreeHGlobal(DPID);
            Marshal.FreeHGlobal(DPID4);


            //返回检测结果
            return checkResult;
        }
    }
}
