using PKeyToolWeb_NetCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PKeyToolWeb_NetCore.Models.PKeyModel;
using static PKeyToolWeb_NetCore.Models.PKeyModel.AllViewModel;
using static PKeyToolWeb_NetCore.Models.PKeyModel.AllModel;
using Microsoft.CodeAnalysis;
using System.Data;
using static PKeyToolWeb_NetCore.Models.PKeyModel.PKeyConfigModel;
using static PKeyToolWeb_NetCore.Models.PKeyModel.GetErrorCodeByPost;
using System.Xml.Linq;

namespace PKeyToolWeb_NetCore.Controllers
{
    public class CKController : Controller
    {
        private readonly ILogger<CKController> _logger;
        public WAAccountContext context;
        public static IMemoryCache WebCache;
        public const string VideoCheckUA = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36";
        public const string KeyFormat = @"([BCDFGHJKMPQRTVWXY2-9N]{5}\-){4}[BCDFGHJKMPQRTVWXY2-9N]{4}[BCDFGHJKMPQRTVWXY2-9]";
        public string rootPath = Directory.GetCurrentDirectory()+"/wwwroot/";
        public string pkconfigPath;
        public List<ConfigType> configList = new List<ConfigType>();
        public CKController(ILogger<CKController> logger, WAAccountContext wacontext, IMemoryCache cache)
        {
            _logger = logger;
            context = wacontext;
            WebCache= cache;

            //初始化证书配置
            LoadConfig();
        }

        private void LoadConfig()
        {
            pkconfigPath = rootPath + "config/PkeyData.xml";
            try
            {
                PkeyData pkconfigModel = (PkeyData)new XmlSerializer(typeof(PkeyData)).Deserialize(new StringReader(System.IO.File.ReadAllText(pkconfigPath)));

                if (pkconfigModel!=null)
                {
                    configList = pkconfigModel.ConfigTypes;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 检测密钥的接口 （Server专用）
        /// </summary>
        /// <param name="k">密钥</param>
        /// <param name="coem">检测OEM密钥的计数</param>
        /// <param name="skipdb">跳过连接被封密钥数据库</param>
        /// <param name="getsimp">仅获取密钥的数据，不检测次数</param>
        /// <param name="makcode">0为 不检测MAK密钥的错误代码；1为 检测MAK密钥的错误代码</param>
        /// <param name="cindex">证书索引。0为 遍历所有证书；其他数值为 指定证书ID</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Check")]
        public async Task<string> CheckKeys(string k, int coem = 0, int skipdb = 0, int getsimp = 0, int makcode = 0, int cindex = 0,string token="")
        {      
            if (string.IsNullOrWhiteSpace(k))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    new XmlSerializer(typeof(PopupPageModel)).Serialize(ms, new AllViewModel().GetPopupDetail(0, "参数错误！", true));
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            #region 设置缓存
            HttpContext.Session.SetInt32("CheckOptionOEM", coem);
            HttpContext.Session.SetInt32("SkipBlockDBCheck", skipdb);
            HttpContext.Session.SetInt32("GetSimpIfm", getsimp);
            HttpContext.Session.SetInt32("CheckMAKErrorCode", makcode);
            #endregion

            k=k.ToUpper().Replace(" ", "").Replace("\0", "");

            int keynummode = 0;

            MatchCollection abc = Regex.Matches(k, KeyFormat);
            if (abc.Count > 0)
            {
                if (abc.Count == 1)
                {
                    keynummode=0;

                    List<CheckResultModel> CheckResultList = new List<CheckResultModel>();
                    CheckResultModel checkResult = await Checking(abc[0].Value, cindex);
                    if (checkResult == null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            new XmlSerializer(typeof(PopupPageModel)).Serialize(ms, new AllViewModel().GetPopupDetail(0, "ERROR!", true, keynummode));
                            return Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }
                    CheckResultList.Add(checkResult);

                    PopupPageModel pageModel = new AllViewModel().GetPopupDetail(0, checkResult.ShowMessage, checkResult.IsShowPopup, keynummode);
                    pageModel.CheckResultList = CheckResultList;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        new XmlSerializer(typeof(PopupPageModel)).Serialize(ms, pageModel);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                else if (abc.Count > 50)
                {
                    keynummode=1;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        new XmlSerializer(typeof(PopupPageModel)).Serialize(ms, new AllViewModel().GetPopupDetail(0, "一次不能检测超过50个Key", true, keynummode));
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                else
                {
                    keynummode=1;

                    List<CheckResultModel> CheckResultList = new List<CheckResultModel>();
                    for (int i = 0; i < abc.Count; i++)
                    {
                        CheckResultList.Add(await Checking(abc[i].Value, cindex));
                    }

                    PopupPageModel pageModel = new AllViewModel().GetPopupDetail(0, "", false, keynummode);
                    pageModel.CheckResultList = CheckResultList;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        new XmlSerializer(typeof(PopupPageModel)).Serialize(ms, pageModel);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }

            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    new XmlSerializer(typeof(PopupPageModel)).Serialize(ms, new AllViewModel().GetPopupDetail(0, "密钥长度不正确或输入的内容里不包含密钥！！", true, keynummode));
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
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
        [HttpPost]
        [Route("api/CK")]
        public async Task<string> CheckKeysClient(string k, int coem = 0, int skipdb = 0, int getsimp = 0, int makcode = 0, int cindex = 0)
        {
            List<CheckResultModel> resultList = new List<CheckResultModel>();
            if (string.IsNullOrWhiteSpace(k))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "参数错误！" });
                    new XmlSerializer(typeof(List<CheckResultModel>)).Serialize(ms, resultList);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            HttpContext.Session.SetInt32("CheckOptionOEM", coem);
            HttpContext.Session.SetInt32("SkipBlockDBCheck", skipdb);
            HttpContext.Session.SetInt32("GetSimpIfm", getsimp);
            HttpContext.Session.SetInt32("CheckMAKErrorCode", makcode);

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
                        using (MemoryStream ms = new MemoryStream())
                        {
                            resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "ERROR!" });
                            new XmlSerializer(typeof(List<CheckResultModel>)).Serialize(ms, resultList);
                            return Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }
                    resultList.Add(checkResult);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        new XmlSerializer(typeof(List<CheckResultModel>)).Serialize(ms, resultList);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                else if (abc.Count > 50)
                {
                    keynummode=1;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "一次不能检测超过50个Key" });
                        new XmlSerializer(typeof(List<CheckResultModel>)).Serialize(ms, resultList);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                else
                {
                    keynummode=1;

                    for (int i = 0; i < abc.Count; i++)
                    {
                        resultList.Add(await Checking(abc[i].Value, cindex));
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        new XmlSerializer(typeof(List<CheckResultModel>)).Serialize(ms, resultList);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }

            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    resultList.Add(new CheckResultModel() { IsShowPopup = true, ShowMessage = "密钥长度不正确或输入的内容里不包含密钥！！" });
                    new XmlSerializer(typeof(List<CheckResultModel>)).Serialize(ms, resultList);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }
        [HttpPost]
        [Route("api/SK")]
        public async Task<string> ShareKey(string sname="",string token="")
        {
            return "请自行编写密钥分享的接口实现，业务逻辑大致为判断用户提供的密钥是否有效，然后录入数据库。";
        }

        /// <summary>
        /// 密钥分享页面的重新检测接口
        /// </summary>
        /// <param name="k">密钥</param>
        /// <param name="coem">检测OEM密钥的计数</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/RecheckSK")]
        public async Task<string> RecheckSharedKey(string k, int coem = 0, string token = "")
        {
            return "请自行编写密钥分享页面的重新检测接口的接口实现，业务逻辑大致为判断用户提供的密钥是否有效，以及重新检测，最后更新数据库。";
        }

        /// <summary>
        /// 被封密钥查询接口
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/BlockKeyQuery")]
        public async Task<string> BlockKeyQueryFunc(string q,string token)
        {
            return "请自行编写被封密钥查询的接口实现，业务逻辑大致为从数据库查询被封密钥。";
        }

        private async Task<CheckResultModel> Checking(string key, int cindex)
        {
            //连接已封密钥数据库进行检查
            if (HttpContext.Session.GetInt32("SkipBlockDBCheck")==0)
            {
//请自行编写连接被封密钥数据库进行查询的业务逻辑！
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
                        string configpath = Path.Combine(rootPath, "pkeyconfig", configList[conindex].ConfigPath);
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

                    if(isContinueSearch)
                    {
                        for (int conindex = 0; conindex <configList.FindIndex(p => p.ConfigID == 14); conindex++)
                        {
                            //string configpath = rootPath+"pkeyconfig/"+configList[i].ConfigPath;
                            string configpath = Path.Combine(rootPath, "pkeyconfig", configList[conindex].ConfigPath);
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
                        string configpath = Path.Combine(rootPath, "pkeyconfig", configList[conindex].ConfigPath);
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
            }
            else if (cindex==900)
            {
                CheckResultModel checkData = null;
                for (int conindex = 0; conindex <configList.Count; conindex++)
                {
                    List<int> checkID = new List<int>() { 12,13,14,15,16,18};
                    if(checkID.Where(p => p==configList[conindex].ConfigID).Count()>0)
                    {
                        string configpath = Path.Combine(rootPath, "pkeyconfig", configList[conindex].ConfigPath);
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
                        string configpath = Path.Combine(rootPath, "pkeyconfig", configList[conindex].ConfigPath);
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
                    string configpath = Path.Combine(rootPath, "pkeyconfig", config.ConfigPath);
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
                ErrorCodeCheckResultModel eccheckresult=new ErrorCodeCheckResultModel();
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
                    }
                    if (checkResult.KeyProduN.IndexOf("RTM") == 0)
                    {
                        checkResult.KeyProduN += " (Office 2010)";
                    }

                    //判断是否勾选 只获取基本信息
                    if (HttpContext.Session.GetInt32("GetSimpIfm")==0)
                    {
                        if (LicenseTypeV.Contains("Retail") || LicenseTypeV.Contains("OEM"))
                        {
                            if ((HttpContext.Session.GetInt32("CheckOptionOEM")==1) && LicenseTypeV.Contains("OEM"))
                            {
                                checkResult.ActiveCount = KeyCheckFunModel.GetCount(KeyCheckFunModel.GetStringFromID(npid, 0x0008));
                                //如果只获取OEM密钥的计数，就不会有ConfigID，这里手动获取一下
                                eccheckresult.ConfigId=GetConfigID(ProductKey, PKeyPath);
                            }
                            else
                            {
                                eccheckresult = await ConsumeKey(ProductKey, PKeyPath);
                                checkResult.ActiveCount =eccheckresult.CheckResult=="" ? "---" : eccheckresult.CheckResult;
                            }
                        }
                        else
                        {
                            if (HttpContext.Session.GetInt32("CheckMAKErrorCode")==1&&checkResult.LicenseType.Contains("MAK"))
                            {
                                eccheckresult = await ConsumeKey(ProductKey, PKeyPath);
                                checkResult.ActiveCount =eccheckresult.CheckResult=="" ? "---" : eccheckresult.CheckResult;
                            }
                            else
                            {
                                checkResult.ActiveCount = KeyCheckFunModel.GetCount(KeyCheckFunModel.GetStringFromID(npid, 0x0008));
                                //批量密钥默认不检测错误代码，所以不会有ConfigID，这里手动获取一下
                                eccheckresult.ConfigId=GetConfigID(ProductKey,PKeyPath);
                            }

                        }
                    }

                }
                catch (Exception)
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
                if (HttpContext.Session.GetInt32("GetSimpIfm")==0)
                {
                    if (checkResult.ActiveCount.Contains("Blocked")||checkResult.ActiveCount.Contains("0xC004C003")||checkResult.ActiveCount.Contains("0xC004C060"))
                    {
                        try
                        {
                            if (HttpContext.Session.GetInt32("SkipBlockDBCheck")==0)
                            {
                                //向已封密钥数据库里插入被封密钥
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        if (HttpContext.Session.GetInt32("SkipBlockDBCheck")==1)
                        {
                            if (!checkResult.ActiveCount.Contains("-") && !checkResult.ActiveCount.Contains("Error") && checkResult.ActiveCount != "")
                            {
                                //从已封密钥数据库里删除复活的密钥
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
   
        public IActionResult KeyCheck()
        {
            return View();
        }

        /// <summary>
        /// 密钥分享
        /// </summary>
        /// <param name="skptype">显示点击的哪个按钮。1：首页。2：末页。3：上一页。4：下一页。</param>
        /// <param name="pagenum">要获取的页数</param>
        /// <param name="q">搜索的关键词</param>
        /// <param name="qtype">查找类型。0为 下拉框选择版本；1为 搜索框输入搜索。</param>
        /// <returns>把模型返回给视图</returns>
        public async Task<ActionResult<AllModel>> ShareKeys(int skptype, int pagenum, int qtype, string q="")
        {
            //"请自行编写共享密钥数据获取的接口实现，业务逻辑大致为从密钥共享数据库读取数据。";
            AllModel allModel = new AllModel();
            //返回对应页面的密钥数据
            allModel.SKList = new List<SharedKey>();
            allModel.SharedKeyIfmList = new List<SharedKeyIfm>();

            return View(allModel);
        }
        public IActionResult Privacy()
        {
            return View();
        }      
        public IActionResult DataQuery()
        {
            return View();
        }     
        public IActionResult MSProduct()
        {
            return View();
        }   
        public IActionResult WebAPIPage()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

