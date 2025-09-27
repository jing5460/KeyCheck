using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static PKeyToolWeb_NetCore.Models.PKeyModel.AllModel;

namespace PKeyToolWeb_NetCore.Models.PKeyModel
{
    public class AllModel
    {
        public List<SharedKey> SKList { get; set; }
        public List<SharedKeyIfm> SharedKeyIfmList { get; set; }

        public class BlockedKeys
        {
//自行设计Model
        }
        public class SharedKey
        {
            //自行设计Model
        }
        public class CheckResultModel
        {
            public string ProductKey { get; set; }
            public string KeyConfig { get; set; }
            public string KeyProduN { get; set; }
            public string KeyEdition { get; set; }
            public string ePID { get; set; }
            public string ProductID { get; set; }
            public string KeySKU { get; set; }
            public string LicenseType { get; set; }
            public string LicenseChannel { get; set; }
            public string PartNum { get; set; }
            public string ActiveCount { get; set; }
            /// <summary>
            /// 是否显示整个弹窗
            /// </summary>
            public bool IsShowPopup;
            /// <summary>
            /// 弹窗的提示文本
            /// </summary>
            public string ShowMessage;
        }
        public class SharedKeyIfm
        {
            public string ProductName { get; set; }
            public int Count { get; set; }
        }
        public class RecheckSKReturnModel
        {
            public string Key { get; set; }
            public bool Status { get; set; }
            public string CheckResult { get; set; }
            public string ReturnMessage { get; set; }
        }
        public class BlockedKeyQueryResultModel
        {
            public bool Result { get; set; }
            public string ReturnMessage { get; set; }
            public List<BlockedKeys> Keys { get; set; }
        }
    }

    public class AllViewModel
    {
        public class PopupPageModel
        {
            /// <summary>
            /// 是否显示整个弹窗
            /// </summary>
            public bool IsShowPopup;
            /// <summary>
            /// 弹窗的提示文本
            /// </summary>
            public string ShowMessage;
            /// <summary>
            /// 是否显示弹窗内文本框
            /// </summary>
            public bool IsTextboxVisible;
            /// <summary>
            /// 是否显示取消按钮
            /// </summary>
            public bool IsCancelButtonVisible;
            /// <summary>
            /// 弹窗的ID。0为 当前是密钥分享弹窗；1为 当前是消息提示弹窗
            /// </summary>
            public int PopupStateID;
            /// <summary>
            /// 密钥数量类型。0为 单个密钥；1为 多个密钥
            /// </summary>
            public int CheckKeyNumMode;
            /// <summary>
            /// 返回的检测结果，如不需要该值为null
            /// </summary>
            public List<CheckResultModel> CheckResultList;
        }

        /// <summary>
        /// 弹窗时调用该函数，函数会返回一个类，是该弹窗的各个属性
        /// </summary>
        /// <param name="pid">弹窗类型。0为 默认；1为 密钥分享</param>
        ///<param name="msg">弹窗的提示文本</param>
        ///<param name="showpage">是否显示整个弹窗</param>
        ///<param name="keymode">密钥数量类型</param>
        /// <returns>弹窗属性类</returns>
        public PopupPageModel GetPopupDetail(int pid, string msg, bool showpage, int keymode = 0)
        {
            switch (pid)
            {
                case 1:
                    return new PopupPageModel()
                    {
                        IsShowPopup=showpage,
                        ShowMessage=msg,
                        CheckKeyNumMode=keymode,

                        IsTextboxVisible=true,
                        IsCancelButtonVisible=true,
                        PopupStateID=0
                    };
                default:
                    return new PopupPageModel()
                    {
                        IsShowPopup=showpage,
                        ShowMessage =msg,
                        CheckKeyNumMode=keymode,

                        IsTextboxVisible=false,
                        IsCancelButtonVisible=false,
                        PopupStateID=1
                    };
            }
        }


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

    public static class KeyCheckFunModel
    {
        public static string GetStringFromID(byte[] bytes, int index)
        {
            int n = index;
            while (!(bytes[n] == 0 && bytes[n + 1] == 0)) n++;
            return Encoding.ASCII.GetString(bytes, index, n - index).Replace("\0", "");
        }
        public static string GetProductDescription(string pkey, string aid, string edi)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pkey);
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(doc.GetElementsByTagName("tm:infoBin")[0].InnerText));
            doc.Load(stream);
            XmlNamespaceManager ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("pkc", "http://www.microsoft.com/DRM/PKEY/Configuration/2.0");
            try
            {
                XmlNode node = doc.SelectSingleNode("/pkc:ProductKeyConfiguration/pkc:Configurations/pkc:Configuration[pkc:ActConfigId='" + aid + "']", ns);
                if (node == null)
                {
                    node = doc.SelectSingleNode("/pkc:ProductKeyConfiguration/pkc:Configurations/pkc:Configuration[pkc:ActConfigId='" + aid.ToUpper() + "']", ns);
                }
                if (node.HasChildNodes)
                {
                    if (node.ChildNodes[2].InnerText.Contains(edi))
                    {
                        return node.ChildNodes[3].InnerText;
                    }
                    return "Not Found";
                }
                return "Not Found";
            }
            catch (Exception)
            {
                return "Not Found";
            }
            finally
            {
                stream.Dispose();
            }
        }
        public static string GetCount(string ePID)
        {
            var activationRequest = Encoding.Unicode.GetBytes
            (
                "<ActivationRequest xmlns=\"http://www.microsoft.com/DRM/SL/BatchActivationRequest/1.0\">" +
                    "<VersionNumber>2.0</VersionNumber>" +
                    "<RequestType>2</RequestType>" +
                    "<Requests>" +
                        "<Request>" +
                            "<PID>" + ePID + "</PID>" +
                        "</Request>" +
                    "</Requests>" +
                "</ActivationRequest>"
            );
            //                            "<IID>"+ "033850283375015180030253934215848126463520291818512065956800244" + "</IID>"+
            var digest = new HMACSHA256(MSActivationServerHmacKey).ComputeHash(activationRequest, 0, activationRequest.Length);

            var soapRequest = Encoding.UTF8.GetBytes
            (
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<s:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                            "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                            "xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\"" +
                ">" +
                    "<s:Body>" +
                        "<BatchActivate xmlns = \"http://www.microsoft.com/BatchActivationService\">" +
                            "<request>" +
                                "<Digest>" + Convert.ToBase64String(digest, Base64FormattingOptions.None) + "</Digest>" +
                                "<RequestXml>" + Convert.ToBase64String(activationRequest, Base64FormattingOptions.None) + "</RequestXml>" +
                            "</request>" +
                        "</BatchActivate>" +
                    "</s:Body>" +
                "</s:Envelope>"
            );

            var httpRequest = (HttpWebRequest)WebRequest.Create("https://activation.sls.microsoft.com/BatchActivation/BatchActivation.asmx");

            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/soap+xml; charset=utf-8";
            httpRequest.ContentLength = soapRequest.Length;
            httpRequest.ProtocolVersion = new Version(1, 1);
            httpRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 4.0.30319.1)";
            httpRequest.Host = "activation.sls.microsoft.com";

            using (var requestStream = httpRequest.GetRequestStream())
            {
                requestStream.Write(soapRequest, 0, soapRequest.Length);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                //HttpContext.Current.Response.Write("<script>alert('" + "服务器连接失败！" + "');</script>");
                return "Error,Failed Connect to Server";
            }

            var soapResponseDocument = new XmlDocument();

            using (var soapResponse = httpResponse.GetResponseStream())
            {
                if (soapResponse != null) soapResponseDocument.Load(soapResponse);

                /*
                                 int a = soapResponseDocument.ChildNodes.Count;
                                string b = "";
                                for (int i = 0; i < a; i++)
                                {
                                    b += soapResponseDocument.ChildNodes[i].InnerText;
                                }
                                MessageBox.Show(b);
                 */
            }

            var activationResponseDocument = new XmlDocument();
            activationResponseDocument.LoadXml(soapResponseDocument.InnerText);

            //获取错误信息
            var payLoadNode = activationResponseDocument.SelectSingleNode("/*[local-name()='ActivationResponse']/*[local-name()='Responses']/*[local-name()='Response']");
            var errorCodeText = payLoadNode?.SelectSingleNode("//*[local-name()='ErrorInfo']/*[local-name()='ErrorCode']")?.InnerText;

            if (errorCodeText != null)
            {
                switch (errorCodeText)
                {
                    case "0x67":
                        return "Key is Blocked";
                    case "0x86":
                        return "---";
                    default:
                        return "Get Count Error";
                }
            }
            //结束

            //剩余次数
            var activationsRemainingText = payLoadNode.SelectSingleNode("//*[local-name()='ActivationRemaining']")?.InnerText;

            if (activationsRemainingText == null)
            {
                return "Get Count Error";
            }
            //结束


            return activationsRemainingText;
        }

        public static readonly byte[] MSActivationServerHmacKey =
{
                    0xfe, 0x31, 0x98, 0x75, 0xfb, 0x48, 0x84, 0x86, 0x9c, 0xf3, 0xf1, 0xce, 0x99, 0xa8, 0x90, 0x64,
                    0xab, 0x57, 0x1f, 0xca, 0x47, 0x04, 0x50, 0x58, 0x30, 0x24, 0xe2, 0x14, 0x62, 0x87, 0x79, 0xa0,
        };
        public static bool CheckKeyValidWithShare(CheckResultModel resultModel)
        {
            //判断检测结果是否为数字
            try
            {
                Convert.ToInt32(resultModel.ActiveCount);
                return true;
            }
            catch (Exception)
            {

            }

            //判断检测结果的错误代码
            if (resultModel.ActiveCount.Contains("0xC004C020")||resultModel.ActiveCount.Contains("0xC004C008"))
            {
                return true;
            }

            //判断其他状态
            if (resultModel.ActiveCount.Contains("Online Key"))
            {
                return true;
            }

            //特殊类型放行
            if (resultModel.KeyProduN.Contains("Visual Studio")||resultModel.LicenseType.Contains("CSVLK"))
            {
                return true;
            }

            return false;
        }
    }
    public static class GetErrorCodeByPost
    {
        private static readonly string ATO_REQ_TEMPLATE = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
    xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/""
    xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <soap:Body>
        <RequestSecurityToken
            xmlns=""http://schemas.xmlsoap.org/ws/2004/04/security/trust"">
            <TokenType>ProductActivation</TokenType>
            <RequestType>http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue</RequestType>
            <UseKey>
                <Values
                    xmlns:q1=""http://schemas.xmlsoap.org/ws/2004/04/security/trust"" soapenc:arrayType=""q1:TokenEntry[1]"">
                    <TokenEntry>
                        <Name>PublishLicense</Name>
                        <Value>{0}</Value>
                    </TokenEntry>
                </Values>
            </UseKey>
            <Claims>
                <Values
                    xmlns:q1=""http://schemas.xmlsoap.org/ws/2004/04/security/trust"" soapenc:arrayType=""q1:TokenEntry[14]"">
                    <TokenEntry>
                        <Name>BindingType</Name>
                        <Value>msft:rm/algorithm/hwid/4.0</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>Binding</Name>
                        <Value>{1}</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ProductKey</Name>
                        <Value>{2}</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ProductKeyType</Name>
                        <Value>msft:rm/algorithm/pkey/2009</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ProductKeyActConfigId</Name>
                        <Value>{3}</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>otherInfoPublic.licenseCategory</Name>
                        <Value>msft:sl/EUL/ACTIVATED/PUBLIC</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>otherInfoPrivate.licenseCategory</Name>
                        <Value>msft:sl/EUL/ACTIVATED/PRIVATE</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>otherInfoPublic.sysprepAction</Name>
                        <Value>rearm</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>otherInfoPrivate.sysprepAction</Name>
                        <Value>rearm</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ClientInformation</Name>
                        <Value>SystemUILanguageId=1033;UserUILanguageId=1033;GeoId=244</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ClientSystemTime</Name>
                        <Value>{4}</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ClientSystemTimeUtc</Name>
                        <Value>{5}</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>otherInfoPublic.secureStoreId</Name>
                        <Value>{6}</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>otherInfoPrivate.secureStoreId</Name>
                        <Value>{6}</Value>
                    </TokenEntry>
                </Values>
            </Claims>
        </RequestSecurityToken>
    </soap:Body>
</soap:Envelope>";
        private static readonly string PKC_REQ_TEMPLATE = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope
    xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/""
    xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <soap:Body>
        <RequestSecurityToken
            xmlns=""http://schemas.xmlsoap.org/ws/2004/04/security/trust"">
            <TokenType>PKC</TokenType>
            <RequestType>http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue</RequestType>
            <UseKey>
                <Values xsi:nil=""1""/>
            </UseKey>
            <Claims>
                <Values
                    xmlns:q1=""http://schemas.xmlsoap.org/ws/2004/04/security/trust"" soapenc:arrayType=""q1:TokenEntry[3]"">
                    <TokenEntry>
                        <Name>ProductKey</Name>
                        <Value>{0}</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ProductKeyType</Name>
                        <Value>msft:rm/algorithm/pkey/2009</Value>
                    </TokenEntry>
                    <TokenEntry>
                        <Name>ProductKeyActConfigId</Name>
                        <Value>{1}</Value>
                    </TokenEntry>
                </Values>
            </Claims>
        </RequestSecurityToken>
    </soap:Body>
</soap:Envelope>";
        public static async Task<ErrorCodeCheckResultModel> ConsumeKey(string pkey, string configpath, string config_ext = "Retail")
        {
            string pl_data = @"<?xml version=""1.0"" encoding=""utf-8""?><rg:licenseGroup xmlns:rg=""urn:mpeg:mpeg21:2003:01-REL-R-NS""><r:license xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS"" licenseId=""{add96a1a-5ae7-425d-935d-3b6effd43a92}"" xmlns:sx=""urn:mpeg:mpeg21:2003:01-REL-SX-NS"" xmlns:mx=""urn:mpeg:mpeg21:2003:01-REL-MX-NS"" xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2"" xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><r:title>Windows(R) Publishing License (Public)</r:title><r:grant><r:forAll varName=""productId""><r:anXmlExpression>/sl:productId/sl:pid</r:anXmlExpression></r:forAll><r:forAll varName=""binding""></r:forAll><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>v0JgOuEWuaA3INoAK10wY7PLaEhyfjfL5A2joNwBR/3ziJxewXKy5QDzZvD3C9eVdvlSqFCDpZEDUxVWvFFeYKI5YkTeK5x7X4nQPodwZAoTJklTUWpfZNslLYJVMaxRvs8htxKoIbvmssqN4Dhy3Oa7HT80GcOvS95M7UCvXcQ7TjrQUV9QNb0w6WLdMVpuktek1CVi4XQ3ELIHZJhyKAtWNGRN4kxZL9nYyDvZ8be5rlGTuhEsgi1oFqnjzMLYXU4wkF/W8mRedIkvoBu3kCjuwEqsr9P5sIbHowqFX5sRxmTrgwoCXPCtFyXCwu9hO75mvb1I1sCuv8W0gTfMtw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><r:issue/><r:grant><r:forAll varName=""application""><r:anXmlExpression>editionId[@value="""" or @value=""EnterpriseS""]</r:anXmlExpression></r:forAll><r:forAll varName=""appid""><r:propertyPossessor><tm:application varRef=""application""/><r:trustedRootIssuers><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>tajcnLtdaeK0abuL2BpVC7obdfSChnHAx7TSn/37DwbTDegkDkEnbr0YyO/Q5Jluj5QD897+nWW54RDbYYTdNgWjyUpwYEJFXSZtd8LFK2mbIjKfG2HIShp6JJARlrgObR89a1EH716nP3PbJk6PWQa6VfjBzPQUgSVywIRU+OKbnzNbUVmQ/rAN6+AN/8fRmFhyKqOAiV/Np2jBtGNxLXm9ebMdm5cB8/YNrjp5Ey0nyAtYvovb0B7wnQZfolMF+OFiqzWJo2Ze0O7WHsWBHtIlGR3+c/IjxUJAsI7O3U4hncCZdvlC5GORI2YL9YHZgU9guSPLhAybQ3IGg7LBuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder></r:trustedRootIssuers></r:propertyPossessor></r:forAll><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>17FgQIuX2S7YIVn8PIeN+qANo4/TUbV8CH5TzbXwmWo4WVI4npVqI4NNhRVsP0ICgMpql1jgAm75dZDBPTzRTCj+Ni0DXIvk6Whlo/ClK/fpZUO3ORQ9VmBE3cXeQQAehgVlUUIzOmG4EeP1i91PCGf5O7I4ayYS2FeQUj+6hyk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><sl:runSoftware/><sl:appId varRef=""appid""/><r:allConditions><r:allConditions><sl:productPolicies xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""><sl:priority>500</sl:priority><sl:policyInt name=""Security-SPP-Reserved-Store-Token-Required"" attributes=""override-only"">0</sl:policyInt><sl:policyInt name=""Kernel-NonGenuineNotificationType"" attributes=""override-only"">2</sl:policyInt><sl:policyStr name=""Security-SPP-Reserved-Windows-Version-V2"" attributes=""override-only"">10.0</sl:policyStr><sl:policyInt name=""Security-SPP-WriteWauMarker"">1</sl:policyInt><sl:policyStr name=""Security-SPP-Reserved-Family"" attributes=""override-only"">EnterpriseS</sl:policyStr></sl:productPolicies><sl:proxyExecutionKey xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""></sl:proxyExecutionKey><sl:externalValidator xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""><sl:type>msft:sl/externalValidator/generic</sl:type><sl:data Algorithm=""msft:rm/algorithm/flags/1.0"">DAAAAAEAAAAFAAAA</sl:data></sl:externalValidator></r:allConditions><mx:renderer><sl:binding varRef=""binding""/><sl:productId varRef=""productId""/></mx:renderer></r:allConditions></r:grant><r:allConditions><sl:businessRules xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""></sl:businessRules></r:allConditions></r:grant><r:issuer><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.microsoft.com/xrml/lwc14n""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference><Transforms><Transform Algorithm=""urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform""/><Transform Algorithm=""http://www.microsoft.com/xrml/lwc14n""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>ivMENCvkqJvb41ZNgue9GpfjWDI=</DigestValue></Reference></SignedInfo><SignatureValue>exwwz6jLpaJ0u1KEEDOCFDXwUAEwI8jUpcamyUkFyqbuYBVCinoihNCtgZAvXcQ+N35MNSXLKXlXpttYE0M2O8dZWR/Frxt38RWxCQj/4heGIwPqQJ7KUZtOdBvytjA6XSvv6uqq1aNAaSWyb7l7jkXc14ycfvxILMVqYdmkIw6BQNZ8/R/anl4VQjAeBdg/+DrcxoHvVT1pVe5PJkrPFRi2B7+0P0oWBljataVjwqDnxYfcJq7lkErHsl78sH2rWPOP/carliYgFNTyEc8437MN5xkNJmeQpsAyTpfE+H7r74WXsk59aU7NoUxteOBRzUNZCgCp2Trr09awd5k2Pg==</SignatureValue><KeyInfo><KeyValue><RSAKeyValue><Modulus>tajcnLtdaeK0abuL2BpVC7obdfSChnHAx7TSn/37DwbTDegkDkEnbr0YyO/Q5Jluj5QD897+nWW54RDbYYTdNgWjyUpwYEJFXSZtd8LFK2mbIjKfG2HIShp6JJARlrgObR89a1EH716nP3PbJk6PWQa6VfjBzPQUgSVywIRU+OKbnzNbUVmQ/rAN6+AN/8fRmFhyKqOAiV/Np2jBtGNxLXm9ebMdm5cB8/YNrjp5Ey0nyAtYvovb0B7wnQZfolMF+OFiqzWJo2Ze0O7WHsWBHtIlGR3+c/IjxUJAsI7O3U4hncCZdvlC5GORI2YL9YHZgU9guSPLhAybQ3IGg7LBuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></KeyInfo></Signature><r:details><r:timeOfIssue>2016-01-01T00:00:00Z</r:timeOfIssue></r:details></r:issuer><r:otherInfo xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS""><tm:infoTables xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><tm:infoList tag=""#global""><tm:infoStr name=""licenseType"">msft:sl/PL/GENERIC/PUBLIC</tm:infoStr><tm:infoStr name=""licenseVersion"">2.0</tm:infoStr><tm:infoStr name=""licensorUrl"">http://licensing.microsoft.com</tm:infoStr><tm:infoStr name=""licenseCategory"">msft:sl/PL/GENERIC/PUBLIC</tm:infoStr><tm:infoStr name=""productSkuId"">{cce9d2de-98ee-4ce2-8113-222620c64a27}</tm:infoStr><tm:infoStr name=""privateCertificateId"">{38c2c1c2-f73e-4fb2-bb44-d8a52fdcbc51}</tm:infoStr><tm:infoStr name=""applicationId"">{55c92734-d682-4d71-983e-d6ec3f16059f}</tm:infoStr><tm:infoStr name=""productName"">Windows(R), EnterpriseS edition</tm:infoStr><tm:infoStr name=""Family"">EnterpriseS</tm:infoStr><tm:infoStr name=""productAuthor"">Microsoft Corporation</tm:infoStr><tm:infoStr name=""productDescription"">Windows(R) Operating System</tm:infoStr><tm:infoStr name=""clientIssuanceCertificateId"">{4961cc30-d690-43be-910c-8e2db01fc5ad}</tm:infoStr><tm:infoStr name=""hwid:ootGrace"">0</tm:infoStr></tm:infoList></tm:infoTables></r:otherInfo></r:license><r:license xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS"" licenseId=""{38c2c1c2-f73e-4fb2-bb44-d8a52fdcbc51}"" xmlns:sx=""urn:mpeg:mpeg21:2003:01-REL-SX-NS"" xmlns:mx=""urn:mpeg:mpeg21:2003:01-REL-MX-NS"" xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2"" xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><r:title>Windows(R) Publishing License (Private)</r:title><r:grant><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>v0JgOuEWuaA3INoAK10wY7PLaEhyfjfL5A2joNwBR/3ziJxewXKy5QDzZvD3C9eVdvlSqFCDpZEDUxVWvFFeYKI5YkTeK5x7X4nQPodwZAoTJklTUWpfZNslLYJVMaxRvs8htxKoIbvmssqN4Dhy3Oa7HT80GcOvS95M7UCvXcQ7TjrQUV9QNb0w6WLdMVpuktek1CVi4XQ3ELIHZJhyKAtWNGRN4kxZL9nYyDvZ8be5rlGTuhEsgi1oFqnjzMLYXU4wkF/W8mRedIkvoBu3kCjuwEqsr9P5sIbHowqFX5sRxmTrgwoCXPCtFyXCwu9hO75mvb1I1sCuv8W0gTfMtw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><r:issue/><r:grant><r:forAll varName=""anyRight""></r:forAll><r:forAll varName=""appid""></r:forAll><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>17FgQIuX2S7YIVn8PIeN+qANo4/TUbV8CH5TzbXwmWo4WVI4npVqI4NNhRVsP0ICgMpql1jgAm75dZDBPTzRTCj+Ni0DXIvk6Whlo/ClK/fpZUO3ORQ9VmBE3cXeQQAehgVlUUIzOmG4EeP1i91PCGf5O7I4ayYS2FeQUj+6hyk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><tm:decryptContent/><tm:symmetricKey><tm:AESKeyValue size=""16"">AAAAAAAAAAAAAAAAAAAAAA==</tm:AESKeyValue></tm:symmetricKey><r:prerequisiteRight><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>17FgQIuX2S7YIVn8PIeN+qANo4/TUbV8CH5TzbXwmWo4WVI4npVqI4NNhRVsP0ICgMpql1jgAm75dZDBPTzRTCj+Ni0DXIvk6Whlo/ClK/fpZUO3ORQ9VmBE3cXeQQAehgVlUUIzOmG4EeP1i91PCGf5O7I4ayYS2FeQUj+6hyk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><r:right varRef=""anyRight""/><sl:appId varRef=""appid""/><r:trustedRootIssuers><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>v0JgOuEWuaA3INoAK10wY7PLaEhyfjfL5A2joNwBR/3ziJxewXKy5QDzZvD3C9eVdvlSqFCDpZEDUxVWvFFeYKI5YkTeK5x7X4nQPodwZAoTJklTUWpfZNslLYJVMaxRvs8htxKoIbvmssqN4Dhy3Oa7HT80GcOvS95M7UCvXcQ7TjrQUV9QNb0w6WLdMVpuktek1CVi4XQ3ELIHZJhyKAtWNGRN4kxZL9nYyDvZ8be5rlGTuhEsgi1oFqnjzMLYXU4wkF/W8mRedIkvoBu3kCjuwEqsr9P5sIbHowqFX5sRxmTrgwoCXPCtFyXCwu9hO75mvb1I1sCuv8W0gTfMtw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder></r:trustedRootIssuers></r:prerequisiteRight></r:grant></r:grant><r:issuer><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.microsoft.com/xrml/lwc14n""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference><Transforms><Transform Algorithm=""urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform""/><Transform Algorithm=""http://www.microsoft.com/xrml/lwc14n""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>hXPflMtQRYrmAY85A44Ewqbfedo=</DigestValue></Reference></SignedInfo><SignatureValue>qOP09nDXmVv1Ne9vEruoSNoV4mzBW371vp1E+uW8jTTC9BqESCaDyK38KhFsxyjz2UqKoelnaFDBTdbVN8VTzJIQCI5sSjMjWzBP31OUHOYDLUGQO7qpRDYwcGRQPsGsQwmNbyTPgq0m4wYcEU4FRj9LIi8B8saMo9xKAO4JNOB/lS8eScHcoUJAdAOoO4MZXfaqZmT90RrMPGPUIY3uTdjtiwL0B46bRdFNYwFuItdHdTUmXOPbXVWogPScSj3JYI9yhdcxjgyb9SxknG0UID9ogTI7HirsfuMvhWkyCSGtV1N4Rr9+c2oiDpWcYeaY4cWcTuSrb+S7vhA10jaJug==</SignatureValue><KeyInfo><KeyValue><RSAKeyValue><Modulus>tajcnLtdaeK0abuL2BpVC7obdfSChnHAx7TSn/37DwbTDegkDkEnbr0YyO/Q5Jluj5QD897+nWW54RDbYYTdNgWjyUpwYEJFXSZtd8LFK2mbIjKfG2HIShp6JJARlrgObR89a1EH716nP3PbJk6PWQa6VfjBzPQUgSVywIRU+OKbnzNbUVmQ/rAN6+AN/8fRmFhyKqOAiV/Np2jBtGNxLXm9ebMdm5cB8/YNrjp5Ey0nyAtYvovb0B7wnQZfolMF+OFiqzWJo2Ze0O7WHsWBHtIlGR3+c/IjxUJAsI7O3U4hncCZdvlC5GORI2YL9YHZgU9guSPLhAybQ3IGg7LBuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></KeyInfo></Signature><r:details><r:timeOfIssue>2016-01-01T00:00:00Z</r:timeOfIssue></r:details></r:issuer><r:otherInfo xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS""><tm:infoTables xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><tm:infoList tag=""#global""><tm:infoStr name=""licenseType"">msft:sl/PL/GENERIC/PRIVATE</tm:infoStr><tm:infoStr name=""licenseVersion"">2.0</tm:infoStr><tm:infoStr name=""licensorUrl"">http://licensing.microsoft.com</tm:infoStr><tm:infoStr name=""licenseCategory"">msft:sl/PL/GENERIC/PRIVATE</tm:infoStr><tm:infoStr name=""publicCertificateId"">{add96a1a-5ae7-425d-935d-3b6effd43a92}</tm:infoStr><tm:infoStr name=""clientIssuanceCertificateId"">{4961cc30-d690-43be-910c-8e2db01fc5ad}</tm:infoStr><tm:infoStr name=""hwid:ootGrace"">0</tm:infoStr><tm:infoStr name=""win:branding"">125</tm:infoStr></tm:infoList></tm:infoTables></r:otherInfo></r:license></rg:licenseGroup>";
            if (!pkey.Contains("N"))
                return new ErrorCodeCheckResultModel() { CheckResult="", ReturnMessage="不支持检测该版本密钥的错误代码", Status=false };

            ProductKeyDecoder pkey_data = new ProductKeyDecoder(pkey);
            string skuid;
            PKeyConfig pkc = new PKeyConfig(XElement.Parse(File.ReadAllText(configpath)));
            try
            {
                var readDetail = pkc.ConfigForGroup(pkey_data.Group);
                skuid = readDetail.ConfigId.Substring(1, readDetail.ConfigId.Length - 2);
            }
            catch
            {
                return new ErrorCodeCheckResultModel() { CheckResult = "", ReturnMessage = "内部错误：证书", Status = false };
            }
            string act_data = EncodeKeyData(pkey_data.Group, pkey_data.Serial, pkey_data.Security, pkey_data.Upgrade);
            string act_config_id = $"msft2009:{skuid}&{act_data};";
            string payload = string.Format(ATO_REQ_TEMPLATE,
                HttpUtility.HtmlEncode(pl_data),
                GenerateBinding(),
                pkey,
                HttpUtility.HtmlEncode(act_config_id),
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Guid.NewGuid().ToString()
            );

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(payload, Encoding.UTF8, "text/xml");
                    content.Headers.Add("SOAPAction", "http://microsoft.com/SL/ProductActivationService/IssueToken");
                    client.DefaultRequestHeaders.Add("Accept", "text/*");
                    client.DefaultRequestHeaders.Add("User-Agent", "SLSSoapClient");
                    var resp = await client.PostAsync($"https://activation.sls.microsoft.com/SLActivateProduct/SLActivateProduct.asmx?configextension={config_ext}", content);
                    string respText = await resp.Content.ReadAsStringAsync();
                    XElement data = XElement.Parse(respText);

                    var fault = data.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
                    if (fault == null)
                        return new ErrorCodeCheckResultModel() { CheckResult = "Online Key", ReturnMessage = "Online Key", Status = true ,ConfigId=act_config_id };
                    else
                    {
                        var hresult = fault.Descendants().FirstOrDefault(e => e.Name.LocalName == "HRESULT")?.Value;
                        var message = fault.Descendants().FirstOrDefault(e => e.Name.LocalName == "Message")?.Value;
                        return new ErrorCodeCheckResultModel() { CheckResult = hresult, ReturnMessage =message, Status = false ,ConfigId=act_config_id };
                    }
                }
            }
            catch (Exception)
            {
                return new ErrorCodeCheckResultModel() { CheckResult = "", ReturnMessage = "向MS激活服务器发送请求时发生异常", Status = false };
            }
        }
        public static string GetConfigID(string pkey, string configpath)
        {
            if (!pkey.Contains("N"))
                return "";

            ProductKeyDecoder pkey_data = new ProductKeyDecoder(pkey);
            string skuid;
            PKeyConfig pkc = new PKeyConfig(XElement.Parse(File.ReadAllText(configpath)));
            try
            {
                var readDetail = pkc.ConfigForGroup(pkey_data.Group);
                skuid = readDetail.ConfigId.Substring(1, readDetail.ConfigId.Length - 2);
            }
            catch
            {
                return "";
            }
            string act_data = EncodeKeyData(pkey_data.Group, pkey_data.Serial, pkey_data.Security, pkey_data.Upgrade);
            return $"msft2009:{skuid}&{act_data};";
        }
        public static async Task<ErrorCodeCheckResultModel> ConsumeKeyWithID(string pkey, string act_config_id, string config_ext = "Retail")
        {
            string pl_data = @"<?xml version=""1.0"" encoding=""utf-8""?><rg:licenseGroup xmlns:rg=""urn:mpeg:mpeg21:2003:01-REL-R-NS""><r:license xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS"" licenseId=""{add96a1a-5ae7-425d-935d-3b6effd43a92}"" xmlns:sx=""urn:mpeg:mpeg21:2003:01-REL-SX-NS"" xmlns:mx=""urn:mpeg:mpeg21:2003:01-REL-MX-NS"" xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2"" xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><r:title>Windows(R) Publishing License (Public)</r:title><r:grant><r:forAll varName=""productId""><r:anXmlExpression>/sl:productId/sl:pid</r:anXmlExpression></r:forAll><r:forAll varName=""binding""></r:forAll><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>v0JgOuEWuaA3INoAK10wY7PLaEhyfjfL5A2joNwBR/3ziJxewXKy5QDzZvD3C9eVdvlSqFCDpZEDUxVWvFFeYKI5YkTeK5x7X4nQPodwZAoTJklTUWpfZNslLYJVMaxRvs8htxKoIbvmssqN4Dhy3Oa7HT80GcOvS95M7UCvXcQ7TjrQUV9QNb0w6WLdMVpuktek1CVi4XQ3ELIHZJhyKAtWNGRN4kxZL9nYyDvZ8be5rlGTuhEsgi1oFqnjzMLYXU4wkF/W8mRedIkvoBu3kCjuwEqsr9P5sIbHowqFX5sRxmTrgwoCXPCtFyXCwu9hO75mvb1I1sCuv8W0gTfMtw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><r:issue/><r:grant><r:forAll varName=""application""><r:anXmlExpression>editionId[@value="""" or @value=""EnterpriseS""]</r:anXmlExpression></r:forAll><r:forAll varName=""appid""><r:propertyPossessor><tm:application varRef=""application""/><r:trustedRootIssuers><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>tajcnLtdaeK0abuL2BpVC7obdfSChnHAx7TSn/37DwbTDegkDkEnbr0YyO/Q5Jluj5QD897+nWW54RDbYYTdNgWjyUpwYEJFXSZtd8LFK2mbIjKfG2HIShp6JJARlrgObR89a1EH716nP3PbJk6PWQa6VfjBzPQUgSVywIRU+OKbnzNbUVmQ/rAN6+AN/8fRmFhyKqOAiV/Np2jBtGNxLXm9ebMdm5cB8/YNrjp5Ey0nyAtYvovb0B7wnQZfolMF+OFiqzWJo2Ze0O7WHsWBHtIlGR3+c/IjxUJAsI7O3U4hncCZdvlC5GORI2YL9YHZgU9guSPLhAybQ3IGg7LBuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder></r:trustedRootIssuers></r:propertyPossessor></r:forAll><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>17FgQIuX2S7YIVn8PIeN+qANo4/TUbV8CH5TzbXwmWo4WVI4npVqI4NNhRVsP0ICgMpql1jgAm75dZDBPTzRTCj+Ni0DXIvk6Whlo/ClK/fpZUO3ORQ9VmBE3cXeQQAehgVlUUIzOmG4EeP1i91PCGf5O7I4ayYS2FeQUj+6hyk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><sl:runSoftware/><sl:appId varRef=""appid""/><r:allConditions><r:allConditions><sl:productPolicies xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""><sl:priority>500</sl:priority><sl:policyInt name=""Security-SPP-Reserved-Store-Token-Required"" attributes=""override-only"">0</sl:policyInt><sl:policyInt name=""Kernel-NonGenuineNotificationType"" attributes=""override-only"">2</sl:policyInt><sl:policyStr name=""Security-SPP-Reserved-Windows-Version-V2"" attributes=""override-only"">10.0</sl:policyStr><sl:policyInt name=""Security-SPP-WriteWauMarker"">1</sl:policyInt><sl:policyStr name=""Security-SPP-Reserved-Family"" attributes=""override-only"">EnterpriseS</sl:policyStr></sl:productPolicies><sl:proxyExecutionKey xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""></sl:proxyExecutionKey><sl:externalValidator xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""><sl:type>msft:sl/externalValidator/generic</sl:type><sl:data Algorithm=""msft:rm/algorithm/flags/1.0"">DAAAAAEAAAAFAAAA</sl:data></sl:externalValidator></r:allConditions><mx:renderer><sl:binding varRef=""binding""/><sl:productId varRef=""productId""/></mx:renderer></r:allConditions></r:grant><r:allConditions><sl:businessRules xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2""></sl:businessRules></r:allConditions></r:grant><r:issuer><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.microsoft.com/xrml/lwc14n""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference><Transforms><Transform Algorithm=""urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform""/><Transform Algorithm=""http://www.microsoft.com/xrml/lwc14n""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>ivMENCvkqJvb41ZNgue9GpfjWDI=</DigestValue></Reference></SignedInfo><SignatureValue>exwwz6jLpaJ0u1KEEDOCFDXwUAEwI8jUpcamyUkFyqbuYBVCinoihNCtgZAvXcQ+N35MNSXLKXlXpttYE0M2O8dZWR/Frxt38RWxCQj/4heGIwPqQJ7KUZtOdBvytjA6XSvv6uqq1aNAaSWyb7l7jkXc14ycfvxILMVqYdmkIw6BQNZ8/R/anl4VQjAeBdg/+DrcxoHvVT1pVe5PJkrPFRi2B7+0P0oWBljataVjwqDnxYfcJq7lkErHsl78sH2rWPOP/carliYgFNTyEc8437MN5xkNJmeQpsAyTpfE+H7r74WXsk59aU7NoUxteOBRzUNZCgCp2Trr09awd5k2Pg==</SignatureValue><KeyInfo><KeyValue><RSAKeyValue><Modulus>tajcnLtdaeK0abuL2BpVC7obdfSChnHAx7TSn/37DwbTDegkDkEnbr0YyO/Q5Jluj5QD897+nWW54RDbYYTdNgWjyUpwYEJFXSZtd8LFK2mbIjKfG2HIShp6JJARlrgObR89a1EH716nP3PbJk6PWQa6VfjBzPQUgSVywIRU+OKbnzNbUVmQ/rAN6+AN/8fRmFhyKqOAiV/Np2jBtGNxLXm9ebMdm5cB8/YNrjp5Ey0nyAtYvovb0B7wnQZfolMF+OFiqzWJo2Ze0O7WHsWBHtIlGR3+c/IjxUJAsI7O3U4hncCZdvlC5GORI2YL9YHZgU9guSPLhAybQ3IGg7LBuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></KeyInfo></Signature><r:details><r:timeOfIssue>2016-01-01T00:00:00Z</r:timeOfIssue></r:details></r:issuer><r:otherInfo xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS""><tm:infoTables xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><tm:infoList tag=""#global""><tm:infoStr name=""licenseType"">msft:sl/PL/GENERIC/PUBLIC</tm:infoStr><tm:infoStr name=""licenseVersion"">2.0</tm:infoStr><tm:infoStr name=""licensorUrl"">http://licensing.microsoft.com</tm:infoStr><tm:infoStr name=""licenseCategory"">msft:sl/PL/GENERIC/PUBLIC</tm:infoStr><tm:infoStr name=""productSkuId"">{cce9d2de-98ee-4ce2-8113-222620c64a27}</tm:infoStr><tm:infoStr name=""privateCertificateId"">{38c2c1c2-f73e-4fb2-bb44-d8a52fdcbc51}</tm:infoStr><tm:infoStr name=""applicationId"">{55c92734-d682-4d71-983e-d6ec3f16059f}</tm:infoStr><tm:infoStr name=""productName"">Windows(R), EnterpriseS edition</tm:infoStr><tm:infoStr name=""Family"">EnterpriseS</tm:infoStr><tm:infoStr name=""productAuthor"">Microsoft Corporation</tm:infoStr><tm:infoStr name=""productDescription"">Windows(R) Operating System</tm:infoStr><tm:infoStr name=""clientIssuanceCertificateId"">{4961cc30-d690-43be-910c-8e2db01fc5ad}</tm:infoStr><tm:infoStr name=""hwid:ootGrace"">0</tm:infoStr></tm:infoList></tm:infoTables></r:otherInfo></r:license><r:license xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS"" licenseId=""{38c2c1c2-f73e-4fb2-bb44-d8a52fdcbc51}"" xmlns:sx=""urn:mpeg:mpeg21:2003:01-REL-SX-NS"" xmlns:mx=""urn:mpeg:mpeg21:2003:01-REL-MX-NS"" xmlns:sl=""http://www.microsoft.com/DRM/XrML2/SL/v2"" xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><r:title>Windows(R) Publishing License (Private)</r:title><r:grant><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>v0JgOuEWuaA3INoAK10wY7PLaEhyfjfL5A2joNwBR/3ziJxewXKy5QDzZvD3C9eVdvlSqFCDpZEDUxVWvFFeYKI5YkTeK5x7X4nQPodwZAoTJklTUWpfZNslLYJVMaxRvs8htxKoIbvmssqN4Dhy3Oa7HT80GcOvS95M7UCvXcQ7TjrQUV9QNb0w6WLdMVpuktek1CVi4XQ3ELIHZJhyKAtWNGRN4kxZL9nYyDvZ8be5rlGTuhEsgi1oFqnjzMLYXU4wkF/W8mRedIkvoBu3kCjuwEqsr9P5sIbHowqFX5sRxmTrgwoCXPCtFyXCwu9hO75mvb1I1sCuv8W0gTfMtw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><r:issue/><r:grant><r:forAll varName=""anyRight""></r:forAll><r:forAll varName=""appid""></r:forAll><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>17FgQIuX2S7YIVn8PIeN+qANo4/TUbV8CH5TzbXwmWo4WVI4npVqI4NNhRVsP0ICgMpql1jgAm75dZDBPTzRTCj+Ni0DXIvk6Whlo/ClK/fpZUO3ORQ9VmBE3cXeQQAehgVlUUIzOmG4EeP1i91PCGf5O7I4ayYS2FeQUj+6hyk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><tm:decryptContent/><tm:symmetricKey><tm:AESKeyValue size=""16"">AAAAAAAAAAAAAAAAAAAAAA==</tm:AESKeyValue></tm:symmetricKey><r:prerequisiteRight><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>17FgQIuX2S7YIVn8PIeN+qANo4/TUbV8CH5TzbXwmWo4WVI4npVqI4NNhRVsP0ICgMpql1jgAm75dZDBPTzRTCj+Ni0DXIvk6Whlo/ClK/fpZUO3ORQ9VmBE3cXeQQAehgVlUUIzOmG4EeP1i91PCGf5O7I4ayYS2FeQUj+6hyk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder><r:right varRef=""anyRight""/><sl:appId varRef=""appid""/><r:trustedRootIssuers><r:keyHolder><r:info><KeyValue xmlns=""http://www.w3.org/2000/09/xmldsig#""><RSAKeyValue><Modulus>v0JgOuEWuaA3INoAK10wY7PLaEhyfjfL5A2joNwBR/3ziJxewXKy5QDzZvD3C9eVdvlSqFCDpZEDUxVWvFFeYKI5YkTeK5x7X4nQPodwZAoTJklTUWpfZNslLYJVMaxRvs8htxKoIbvmssqN4Dhy3Oa7HT80GcOvS95M7UCvXcQ7TjrQUV9QNb0w6WLdMVpuktek1CVi4XQ3ELIHZJhyKAtWNGRN4kxZL9nYyDvZ8be5rlGTuhEsgi1oFqnjzMLYXU4wkF/W8mRedIkvoBu3kCjuwEqsr9P5sIbHowqFX5sRxmTrgwoCXPCtFyXCwu9hO75mvb1I1sCuv8W0gTfMtw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></r:info></r:keyHolder></r:trustedRootIssuers></r:prerequisiteRight></r:grant></r:grant><r:issuer><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.microsoft.com/xrml/lwc14n""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference><Transforms><Transform Algorithm=""urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform""/><Transform Algorithm=""http://www.microsoft.com/xrml/lwc14n""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>hXPflMtQRYrmAY85A44Ewqbfedo=</DigestValue></Reference></SignedInfo><SignatureValue>qOP09nDXmVv1Ne9vEruoSNoV4mzBW371vp1E+uW8jTTC9BqESCaDyK38KhFsxyjz2UqKoelnaFDBTdbVN8VTzJIQCI5sSjMjWzBP31OUHOYDLUGQO7qpRDYwcGRQPsGsQwmNbyTPgq0m4wYcEU4FRj9LIi8B8saMo9xKAO4JNOB/lS8eScHcoUJAdAOoO4MZXfaqZmT90RrMPGPUIY3uTdjtiwL0B46bRdFNYwFuItdHdTUmXOPbXVWogPScSj3JYI9yhdcxjgyb9SxknG0UID9ogTI7HirsfuMvhWkyCSGtV1N4Rr9+c2oiDpWcYeaY4cWcTuSrb+S7vhA10jaJug==</SignatureValue><KeyInfo><KeyValue><RSAKeyValue><Modulus>tajcnLtdaeK0abuL2BpVC7obdfSChnHAx7TSn/37DwbTDegkDkEnbr0YyO/Q5Jluj5QD897+nWW54RDbYYTdNgWjyUpwYEJFXSZtd8LFK2mbIjKfG2HIShp6JJARlrgObR89a1EH716nP3PbJk6PWQa6VfjBzPQUgSVywIRU+OKbnzNbUVmQ/rAN6+AN/8fRmFhyKqOAiV/Np2jBtGNxLXm9ebMdm5cB8/YNrjp5Ey0nyAtYvovb0B7wnQZfolMF+OFiqzWJo2Ze0O7WHsWBHtIlGR3+c/IjxUJAsI7O3U4hncCZdvlC5GORI2YL9YHZgU9guSPLhAybQ3IGg7LBuQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue></KeyValue></KeyInfo></Signature><r:details><r:timeOfIssue>2016-01-01T00:00:00Z</r:timeOfIssue></r:details></r:issuer><r:otherInfo xmlns:r=""urn:mpeg:mpeg21:2003:01-REL-R-NS""><tm:infoTables xmlns:tm=""http://www.microsoft.com/DRM/XrML2/TM/v2""><tm:infoList tag=""#global""><tm:infoStr name=""licenseType"">msft:sl/PL/GENERIC/PRIVATE</tm:infoStr><tm:infoStr name=""licenseVersion"">2.0</tm:infoStr><tm:infoStr name=""licensorUrl"">http://licensing.microsoft.com</tm:infoStr><tm:infoStr name=""licenseCategory"">msft:sl/PL/GENERIC/PRIVATE</tm:infoStr><tm:infoStr name=""publicCertificateId"">{add96a1a-5ae7-425d-935d-3b6effd43a92}</tm:infoStr><tm:infoStr name=""clientIssuanceCertificateId"">{4961cc30-d690-43be-910c-8e2db01fc5ad}</tm:infoStr><tm:infoStr name=""hwid:ootGrace"">0</tm:infoStr><tm:infoStr name=""win:branding"">125</tm:infoStr></tm:infoList></tm:infoTables></r:otherInfo></r:license></rg:licenseGroup>";

            string payload = string.Format(ATO_REQ_TEMPLATE,
                HttpUtility.HtmlEncode(pl_data),
                GenerateBinding(),
                pkey,
                HttpUtility.HtmlEncode(act_config_id),
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Guid.NewGuid().ToString()
            );

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(payload, Encoding.UTF8, "text/xml");
                    content.Headers.Add("SOAPAction", "http://microsoft.com/SL/ProductActivationService/IssueToken");
                    client.DefaultRequestHeaders.Add("Accept", "text/*");
                    client.DefaultRequestHeaders.Add("User-Agent", "SLSSoapClient");
                    var resp = await client.PostAsync($"https://activation.sls.microsoft.com/SLActivateProduct/SLActivateProduct.asmx?configextension={config_ext}", content);
                    string respText = await resp.Content.ReadAsStringAsync();
                    XElement data = XElement.Parse(respText);

                    var fault = data.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
                    if (fault == null)
                        return new ErrorCodeCheckResultModel() { CheckResult = "Online Key", ReturnMessage = "Online Key", Status = true , ConfigId = act_config_id };
                    else
                    {
                        var hresult = fault.Descendants().FirstOrDefault(e => e.Name.LocalName == "HRESULT")?.Value;
                        var message = fault.Descendants().FirstOrDefault(e => e.Name.LocalName == "Message")?.Value;
                        return new ErrorCodeCheckResultModel() { CheckResult = hresult, ReturnMessage =message, Status = false,ConfigId = act_config_id };
                    }
                }
            }
            catch (Exception)
            {
                return new ErrorCodeCheckResultModel() { CheckResult = "", ReturnMessage = "向MS激活服务器发送请求时发生异常", Status = false };
            }
        }
        public static async Task<(string, string, bool)> QueryKey(string pkey, string configpath)
        {
            if (!pkey.Contains("N"))
                return ("N/A", "Product key is not PKEY2009.", false);

            dynamic pkey_data = new ProductKeyDecoder(pkey);
            string skuid;
            PKeyConfig pkc = new PKeyConfig(XElement.Parse(File.ReadAllText(configpath)));
            try
            {
                skuid = pkc.ConfigForGroup(pkey_data.Group).ConfigId.Substring(1, pkc.ConfigForGroup(pkey_data.Group).ConfigId.Length - 2);
            }
            catch
            {
                return ("N/A", "Product key not compatible with provided pkeyconfig", false);
            }
            string act_data = EncodeKeyData(pkey_data.Group, pkey_data.Serial, pkey_data.Security, pkey_data.Upgrade);
            string act_config_id = $"msft2009:{skuid}&{act_data};";

            string payload = string.Format(PKC_REQ_TEMPLATE, pkey, HttpUtility.HtmlEncode(act_config_id));
            using (var client = new HttpClient())
            {
                var content = new StringContent(payload, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://microsoft.com/SL/ProductCertificationService/IssueToken");
                client.DefaultRequestHeaders.Add("Accept", "text/*");
                client.DefaultRequestHeaders.Add("User-Agent", "SLSSoapClient");
                var resp = await client.PostAsync("https://activation.sls.microsoft.com/slpkc/SLCertifyProduct.asmx", content);
                string respText = await resp.Content.ReadAsStringAsync();
                XElement data = XElement.Parse(respText);

                var fault = data.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
                if (fault == null)
                    return ("0x0", "", true);
                else
                {
                    var hresult = fault.Descendants().FirstOrDefault(e => e.Name.LocalName == "HRESULT")?.Value;
                    var message = fault.Descendants().FirstOrDefault(e => e.Name.LocalName == "Message")?.Value;
                    return (hresult, message, false);
                }
            }
        }
        public static string GenerateBinding()
        {
            // 固定前缀
            byte[] binding = Convert.FromHexString("2A0000000100020001000100000000000000010001000100");

            // 随机 18 字节
            byte[] randomBytes = new byte[18];
            RandomNumberGenerator.Fill(randomBytes);

            // 合并
            byte[] result = new byte[binding.Length + randomBytes.Length];
            Buffer.BlockCopy(binding, 0, result, 0, binding.Length);
            Buffer.BlockCopy(randomBytes, 0, result, binding.Length, randomBytes.Length);

            // Base64 编码
            return Convert.ToBase64String(result);
        }

        public static string EncodeKeyData(BigInteger group, BigInteger serial, BigInteger security, BigInteger upgrade)
        {
            BigInteger actHash = upgrade & 1;
            actHash |= (serial & ((1UL << 30) - 1)) << 1;
            actHash |= (group & ((1UL << 20) - 1)) << 31;
            actHash |= (security &((1UL << 53) - 1)) << 51;

            byte[] bytes = actHash.ToByteArray();
            Array.Resize(ref bytes, 13);
            return Convert.ToBase64String(bytes);
        }
        public class ProductKeyDecoder
        {
            public const string ALPHABET = "BCDFGHJKMPQRTVWXY2346789";

            public string Key5x5 { get; }
            public BigInteger Key { get; }
            public BigInteger Group { get; }
            public BigInteger Serial { get; }
            public BigInteger Security { get; }
            public BigInteger Checksum { get; }
            public BigInteger Upgrade { get; }
            public BigInteger Extra { get; }

            public ProductKeyDecoder(string key)
            {
                Key5x5 = key;
                Key = Decode5x5(key, ALPHABET);

                Group = Key & ParseHex("000000000000000000000000000fffff");
                Serial = (Key & ParseHex("00000000000000000003fffffff00000")) >> 20;
                Security = (Key & ParseHex("0000007ffffffffffffc000000000000")) >> 50;
                Checksum = (Key & ParseHex("0001ff80000000000000000000000000")) >> 103;
                Upgrade = (Key & ParseHex("00020000000000000000000000000000")) >> 113;
                Extra = (Key & ParseHex("00040000000000000000000000000000")) >> 114;
            }

            public static BigInteger Decode5x5(string key, string alphabet)
            {
                key = key.Replace("-", "");

                var dec = new List<int> { key.IndexOf('N') };
                foreach (var l in key.Replace("N", ""))
                {
                    dec.Add(alphabet.IndexOf(l));
                }

                BigInteger result = 0;
                foreach (var x in dec)
                {
                    result = (result * 24) + x;
                }
                return result;
            }

            private static BigInteger ParseHex(string hex)
            {
                return BigInteger.Parse(hex, NumberStyles.HexNumber);
            }

            public override string ToString() => Key5x5;
        }
        public class PKeyConfig
        {
            public const string PKEYCONFIGDATA_XPATH = "./license/otherInfo/infoTables/infoList/infoBin[@name='pkeyConfigData']";

            public class Configuration
            {
                public string ConfigId { get; set; }
                public int GroupId { get; set; }
                public string EditionId { get; set; }
                public string Desc { get; set; }
                public string KeyType { get; set; }
                public bool Randomized { get; set; }
            }

            public class KeyRange
            {
                public string ConfigId { get; set; }
                public string PartNumber { get; set; }
                public string EulaType { get; set; }
                public bool IsValid { get; set; }
                public int Start { get; set; }
                public int End { get; set; }
            }

            public class PublicKey
            {
                public int GroupId { get; set; }
                public string Algorithm { get; set; }
                public string PubKey { get; set; }
            }

            public List<Configuration> Configs { get; private set; }
            public List<KeyRange> Ranges { get; private set; }
            public List<PublicKey> PubKeys { get; private set; }

            // Initializes via License XrML (as XML string or XElement)
            public PKeyConfig(XElement xml)
            {
                // Find the pkeyConfigData node and decode base64
                var ns = xml.GetDefaultNamespace();
                var pkeyconfigDataNode = xml
                    .Descendants()
                    .FirstOrDefault(
                        x => x.Name.LocalName == "infoBin" &&
                             (string)x.Attribute("name") == "pkeyConfigData"
                    );

                if (pkeyconfigDataNode == null)
                    throw new Exception("pkeyConfigData node not found!");

                var decoded = Convert.FromBase64String(pkeyconfigDataNode.Value);
                var decodedXmlStr = Encoding.UTF8.GetString(decoded);
                var pkeyconfigdata = XElement.Parse(decodedXmlStr);

                // Parse configurations
                Configs = pkeyconfigdata
                    .Descendants()
                    .Where(x => x.Name.LocalName == "Configuration")
                    .Select(x => new Configuration
                    {
                        ConfigId   = (string)x.Element(x.Name.Namespace + "ActConfigId"),
                        GroupId    = int.Parse(x.Element(x.Name.Namespace + "RefGroupId").Value),
                        EditionId  = (string)x.Element(x.Name.Namespace + "EditionId"),
                        Desc       = (string)x.Element(x.Name.Namespace + "ProductDescription"),
                        KeyType    = (string)x.Element(x.Name.Namespace + "ProductKeyType"),
                        Randomized = ((string)x.Element(x.Name.Namespace + "IsRandomized")).ToLower() == "true"
                    })
                    .ToList();

                // Parse key ranges
                Ranges = pkeyconfigdata
                    .Descendants()
                    .Where(x => x.Name.LocalName == "KeyRange")
                    .Select(x => new KeyRange
                    {
                        ConfigId   = (string)x.Element(x.Name.Namespace + "RefActConfigId"),
                        PartNumber = (string)x.Element(x.Name.Namespace + "PartNumber"),
                        EulaType   = (string)x.Element(x.Name.Namespace + "EulaType"),
                        IsValid    = ((string)x.Element(x.Name.Namespace + "IsValid")).ToLower() == "true",
                        Start      = int.Parse(x.Element(x.Name.Namespace + "Start").Value),
                        End        = int.Parse(x.Element(x.Name.Namespace + "End").Value)
                    })
                    .ToList();

                // Parse public keys
                PubKeys = pkeyconfigdata
                    .Descendants()
                    .Where(x => x.Name.LocalName == "PublicKey")
                    .Select(x => new PublicKey
                    {
                        GroupId = int.Parse(x.Element(x.Name.Namespace + "GroupId").Value),
                        Algorithm = (string)x.Element(x.Name.Namespace + "AlgorithmId"),
                        PubKey = (string)x.Element(x.Name.Namespace + "PublicKeyValue")
                    })
                    .ToList();
            }

            public Configuration ConfigForGroup(BigInteger group)
            {
                return Configs.FirstOrDefault(x => x.GroupId == group);
            }         
            public Configuration ConfigForGroup(string productName)
            {
                return Configs.FirstOrDefault(x => x.Desc == productName);
            }

            public List<KeyRange> RangesForGroup(int group)
            {
                var conf = ConfigForGroup(group);
                if (conf == null) return new List<KeyRange>();
                return Ranges.Where(x => x.ConfigId == conf.ConfigId).ToList();
            }

            public List<KeyRange> RangesForConfig(Configuration config)
            {
                return Ranges.Where(x => x.ConfigId == config.ConfigId).ToList();
            }

            public PublicKey PubKeyForGroup(int group)
            {
                return PubKeys.FirstOrDefault(x => x.GroupId == group);
            }

            public (Configuration, List<KeyRange>, PublicKey) AllForGroup(int group)
            {
                var conf = ConfigForGroup(group);
                var keyRanges = RangesForGroup(group);
                var pubKey = PubKeyForGroup(group);
                return (conf, keyRanges, pubKey);
            }
        }
        public class ErrorCodeCheckResultModel
        {
            public string CheckResult { get; set; }
            public string ReturnMessage { get; set; }
            public bool Status { get; set; }
            public string ConfigId { get; set; } = "";
        }

    }

}
