window.onload = function () {
    GetMacVer();
}
function ProdSelectChanged() {
    var index = document.querySelector("#ProdSelect").selectedIndex;
    document.querySelector("#VerSelect").innerText = "";
    document.querySelector("#LangSelect").innerText = "";

    var vselect = document.querySelector("#VerSelect");
    vselect.append(new Option("请选择", "", false, false));

    if (index == 1) {
        vselect.append(new Option("Windows 11 25H2", "3262", false, false));
        vselect.append(new Option("Windows 11 24H2", "3113", false, false));
        vselect.append(new Option("Windows 11 23H2_v2", "2935", false, false));
        vselect.append(new Option("Windows 11 23H2", "2860", false, false));
        vselect.append(new Option("Windows 11 22H2_v2", "2616", false, false));
        vselect.append(new Option("Windows 10 22H2 v1", "2618", false, false));

        vselect.append(new Option("Windows 8.1 Update 3", "52", false, false));
        vselect.append(new Option("Windows 8.1 Single Language", "48", false, false));
    }
    else if (index == 2) {
        vselect.append(new Option("Office 2024", "9", false, false));
        vselect.append(new Option("Office 2021", "1", false, false));
        vselect.append(new Option("Office 2019", "2", false, false));
        vselect.append(new Option("Office 2016", "3", false, false));
        vselect.append(new Option("Office 365", "5", false, false));
        vselect.append(new Option("Office 2013", "4", false, false));
        vselect.append(new Option("Office for Mac " + officeMacVersion, "6", false, false));
        vselect.append(new Option("Office 2019 for Mac", "10", false, false));
        vselect.append(new Option("Office 2016 for Mac", "7", false, false));
        vselect.append(new Option("Office 2011 for Mac", "8", false, false));
    }
    else if (index == 3) {
        vselect.append(new Option("SQL Server", "1", false, false));
        vselect.append(new Option("Visual Studio", "2", false, false));
        //vselect.append(new Option("", "2", false, false));
    }
    else if (index == 4) {
        vselect.append(new Option("Edge for Windows 11/10", "1", false, false));
        vselect.append(new Option("Edge for Windows 8.1 - 7", "2", false, false));

        vselect.append(new Option("Edge for macOS (Intel)", "3", false, false));
        vselect.append(new Option("Edge for macOS (Apple)", "4", false, false));

        vselect.append(new Option("Edge for Linux (Debian / Ubuntu)", "5", false, false));
        vselect.append(new Option("Edge for Linux (Fedora / openSUSE)", "6", false, false));
    }
    /*
         else if (index == 3) {
        vselect.append(new Option("Server 2022", "1", false, false));
        vselect.append(new Option("Server 2019", "2", false, false));
        vselect.append(new Option("Server 2016", "3", false, false));
        vselect.append(new Option("Server 2012", "4", false, false));
        vselect.append(new Option("Server 2008", "5", false, false));
    }
     */
}

var msGUID = "";
function VerSelectChanged() {
    document.querySelector("#LangSelect").innerText = "";
    var langselect = document.querySelector("#LangSelect");
    langselect.append(new Option("请选择", "", false, false));

    var index = document.querySelector("#ProdSelect").selectedIndex;
    var index2 = document.querySelector("#VerSelect").selectedIndex;
    if (index < 1 || index2 < 1) {
        return;
    }

    var pdLinks = document.querySelector("#ProdLinks");
    var vselect = document.querySelector("#VerSelect");

    switch (index) {
        case 1:

            //if (vselect.value == "1") {

            //    pdLinks.value =
            //        "Windows 7 SP1 Professional \nhttps://download.microsoft.com/download/0/6/3/06365375-C346-4D65-87C7-EE41F55F736B/7601.24214.180801-1700.win7sp1_ldr_escrow_CLIENT_PROFESSIONAL_x64FRE_en-us.iso\nhttps://download.microsoft.com/download/C/0/6/C067D0CD-3785-4727-898E-60DC3120BB14/7601.24214.180801-1700.win7sp1_ldr_escrow_CLIENT_PROFESSIONAL_x86FRE_en-us.iso\n\n" +
            //        "Windows 7 SP1 Ultimate \nhttps://download.microsoft.com/download/5/1/9/5195A765-3A41-4A72-87D8-200D897CBE21/7601.24214.180801-1700.win7sp1_ldr_escrow_CLIENT_ULTIMATE_x64FRE_en-us.iso\nhttps://download.microsoft.com/download/1/E/6/1E6B4803-DD2A-49DF-8468-69C0E6E36218/7601.24214.180801-1700.win7sp1_ldr_escrow_CLIENT_ULTIMATE_x86FRE_en-us.iso\n\n" +
            //        "Windows 7 SP1 Home Premium \nhttps://download.microsoft.com/download/E/A/8/EA804D86-C3DF-4719-9966-6A66C9306598/7601.24214.180801-1700.win7sp1_ldr_escrow_CLIENT_HOMEPREMIUM_x64FRE_en-us.iso\nhttps://download.microsoft.com/download/E/D/A/EDA6B508-7663-4E30-86F9-949932F443D0/7601.24214.180801-1700.win7sp1_ldr_escrow_CLIENT_HOMEPREMIUM_x86FRE_en-us.iso";

            //    return;
            //}

            //if (vselect.selectedOptions[0].innerText.match("Preview")) {
            //    pdLinks.value = "如果您要下载预览版本，则您需要先在浏览器里打开微软官网，登录微软预览账户后使用该ID" + vselect.value + "来获取资源。";
            //    return;
            //}
            GetMSNewVerify();

            $.ajax({
                type: "get",
                url: "https://www.microsoft.com/software-download-connector/api/getskuinformationbyproductedition?profile=606624d44113&ProductEditionId=" + vselect.value + "&SKU=undefined&friendlyFileName=undefined&Locale=zh-CN&sessionID=" + msGUID,
                error: function () {
                    alert("发送请求时发生异常，请稍后再试！");
                    return;
                }
            }).then(function (data) {
                try {
                    var tdata = JSON.parse(data);
                    for (var i = 0; i < tdata.Skus.length; i++) {
                        langselect.append(new Option(tdata.Skus[i].Language, tdata.Skus[i].Id, false, false));
                    }
                }
                catch {
                    alert("获取直链失败！");
                    return;
                }
            });

            break;
        case 2:

            if (vselect.value == "6") {

                var tmaclink = "Office for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_365_and_Office_{0}_Installer.pkg\n\nOffice for Mac (with Teams) \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_365_and_Office_{0}_BusinessPro_Installer.pkg\n\nWord for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_Word_{0}_Installer.pkg\n\nExcel for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_Excel_{0}_Installer.pkg\n\n" +
                    "PowerPoint for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_PowerPoint_{0}_Installer.pkg\n\nOutlook for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_Outlook_{0}_Installer.pkg\n\nOneNote for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_OneNote_{0}_Updater.pkg";

                pdLinks.value = tmaclink.replaceAll("{0}", officeMacVersion);

                return;
            }
            else if (vselect.value == "10") {

                pdLinks.value = "Office 2019 for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_365_and_Office_16.78.23102801_Installer.pkg\n\nOffice 2019 for Mac (with Teams) \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_365_and_Office_16.78.23102801_BusinessPro_Installer.pkg\n\nWord 2019 for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_Word_16.78.23102801_Installer.pkg\n\nExcel 2019 for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_Excel_16.78.23102801_Installer.pkg\n\n" +
                    "PowerPoint 2019 for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_PowerPoint_16.78.23102801_Installer.pkg\n\nOutlook 2019 for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_Outlook_16.78.23102801_Installer.pkg\n\nOneNote 2019 for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_OneNote_16.78.23100802_Updater.pkg";

                return;
            }
            else if (vselect.value == "7") {

                pdLinks.value = "Office 2016 for Mac \n(AutoUpdate) https://go.microsoft.com/fwlink/p/?linkid=871743\n\nWord 2016 for Mac \nhttps://officecdn-microsoft-com.akamaized.net/pr/C1297A47-86C4-4C1F-97FA-950631F94777/OfficeMac/Microsoft_Word_2016_Installer.pkg \n(AutoUpdate) https://go.microsoft.com/fwlink/p/?linkid=871748\n\nExcel 2016 for Mac \nhttps://officecdn-microsoft-com.akamaized.net/pr/C1297A47-86C4-4C1F-97FA-950631F94777/OfficeMac/Microsoft_Excel_2016_Installer.pkg \n(AutoUpdate) https://go.microsoft.com/fwlink/p/?linkid=871750\n\n" +
                    "PowerPoint 2016 for Mac \nhttps://officecdn-microsoft-com.akamaized.net/pr/C1297A47-86C4-4C1F-97FA-950631F94777/OfficeMac/Microsoft_PowerPoint_2016_Installer.pkg \n(AutoUpdate) https://go.microsoft.com/fwlink/p/?linkid=871751\n\nOutlook 2016 for Mac \nhttps://officecdn-microsoft-com.akamaized.net/pr/C1297A47-86C4-4C1F-97FA-950631F94777/OfficeMac/Microsoft_Outlook_2016_Installer.pkg \n(AutoUpdate) https://go.microsoft.com/fwlink/p/?linkid=871753\n\nOneNote 2016 for Mac \n(AutoUpdate) https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777/MacAutoupdate/Microsoft_OneNote_16.16.20101200_Updater.pkg";

                return;
            }


            var langGroup = [ "ar-sa", "bg-bg", "zh-cn", "zh-tw", "hr-hr", "cs-cz", "da-dk", "nl-nl", "en-us", "et-ee", "fi-fi", "fr-fr", "de-de", "el-gr", "he-il", "hi-in", "hu-hu", "id-id", "it-it", "ja-jp", "kk-kz", "ko-kr", "lv-lv", "lt-lt", "ms-my", "nb-no", "pl-pl", "pl-br", "pt-pt", "ro-ro", "ru-ru", "sr-latn-RS", "sk-sk", "sl-SI", "es-es", "sv-se", "th-th", "tr-tr", "uk-ua", "vi-vn"];
            var langName = ["Arabic", "Bulgarian", "Chinese Simplified", "Chinese Traditional", "Croatian", "Czech", "Danish", "Dutch", "English", "Estonian", "Finnish", "French", "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian", "Italian", "Japanese", "Kazakh", "Korean", "Latvian", "Lithuanian", "Malay", "Norwegian Bokmål", "Polish", "Portuguese (Brazil)", "Portuguese (Portugal)", "Romanian", "Russian", "Serbian Latin", "Slovak", "Slovenian", "Spanish", "Swedish", "Thai", "Turkish", "Ukrainian", "Vietnamese"];

            for (var i = 0; i < langGroup.length; i++) {
                langselect.append(new Option(langName[i], langGroup[i], false, false));
            }

            break;
        case 3:

            if (index2 == 1) {
                pdLinks.value = "SQL Serve 2019\n Express\n https://download.microsoft.com/download/2/1/6/216eb471-e637-4517-97a6-b247d8051759/SQL2019-SSEI-Expr.exe \n\n Developer Edition\n Chinese (Simplified)\n https://download.microsoft.com/download/c/d/4/cd466ca8-ad1e-4ba9-8590-cda56802ac85/SQLServer2019-x64-CHS-Dev.iso\n English\n https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SQLServer2019-x64-ENU-Dev.iso\n Chinese Traditional\n https://download.microsoft.com/download/1/c/e/1cef21d3-9a3f-40ba-97b2-12fac0252c86/SQLServer2019-x64-CHT-Dev.iso \n\n\n" +
                    "SQL Server 2017 \n Developer Edition \n Chinese (Simplified)\n https://download.microsoft.com/download/6/4/A/64A05A0F-AB28-4583-BD7F-139D0495E473/SQLServer2017-x64-CHS-Dev.iso\n English\n https://download.microsoft.com/download/E/F/2/EF23C21D-7860-4F05-88CE-39AA114B014B/SQLServer2017-x64-ENU-Dev.iso\n Chinese Traditional\n https://download.microsoft.com/download/E/E/1/EE15732E-7103-48B5-9CC0-237C088E5FF3/SQLServer2017-x64-CHT-Dev.iso";

                /*
                document.querySelector("#maji").innerHTML += "<h3 style=\"color:blue;\">SQL Serve 2019</h3> <p style=\"font-weight:bold;\">Express</p> <p> https://download.microsoft.com/download/2/1/6/216eb471-e637-4517-97a6-b247d8051759/SQL2019-SSEI-Expr.exe <p>  <p style=\"font-weight:bold;\">Developer Edition </p> <p> Chinese (Simplified)<br/> https://download.microsoft.com/download/c/d/4/cd466ca8-ad1e-4ba9-8590-cda56802ac85/SQLServer2019-x64-CHS-Dev.iso<br/> English<br/> https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SQLServer2019-x64-ENU-Dev.iso<br/> " +
                    "Chinese Traditional < br /> https://download.microsoft.com/download/1/c/e/1cef21d3-9a3f-40ba-97b2-12fac0252c86/SQLServer2019-x64-CHT-Dev.iso </p><br/>、  <h3 style=\"color:blue;\">SQL Server 2017</h3> <p style=\"font-weight:bold;\">Developer Edition </p> <p> Chinese (Simplified)<br/> https://download.microsoft.com/download/6/4/A/64A05A0F-AB28-4583-BD7F-139D0495E473/SQLServer2017-x64-CHS-Dev.iso<br/> English<br/> https://download.microsoft.com/download/E/F/2/EF23C21D-7860-4F05-88CE-39AA114B014B/SQLServer2017-x64-ENU-Dev.iso<br/> Chinese Traditional<br/> https://download.microsoft.com/download/E/E/1/EE15732E-7103-48B5-9CC0-237C088E5FF3/SQLServer2017-x64-CHT-Dev.iso <p/>";
                */
            }
            else if (index2 == 2) {
                pdLinks.value = " Visual Studio 2022 Community Web Installer \n https://aka.ms/vs/17/release/vs_community.exe \n\n Visual Studio 2022 Professional Web Installer \n https://aka.ms/vs/17/release/vs_professional.exe \n\n Visual Studio 2022 Enterprise Web Installer \n https://aka.ms/vs/17/release/vs_enterprise.exe \n\n\n Visual Studio 2019 Community Web Installer \n https://aka.ms/vs/16/release/vs_community.exe \n\n Visual Studio 2019 Professional Web Installer \n https://aka.ms/vs/16/release/vs_professional.exe \n\n Visual Studio 2019 Enterprise Web Installer \n " +
                    "https://aka.ms/vs/16/release/vs_enterprise.exe \n\n\n Visual Studio 2017 Community Web Installer \n https://aka.ms/vs/15/release/vs_community.exe \n\n Visual Studio 2017 Professional Web Installer \n https://aka.ms/vs/15/release/vs_professional.exe \n\n Visual Studio 2017 Enterprise Web Installer \n https://aka.ms/vs/15/release/vs_enterprise.exe \n\n\n Visual Studio 2015 Update 3 ISO \n https://go.microsoft.com/fwlink/?Linkid=708984 \n\n Visual Studio 2015 Community Web Installer \n https://go.microsoft.com/fwlink/?LinkId=532606 \n\n Visual Studio 2015 Community ISO \n " +
                    "https://go.microsoft.com/fwlink/?LinkId=615448 \n\n Visual Studio 2015 Professional Web Installer \n https://go.microsoft.com/fwlink/?LinkId=615435 \n\n Visual Studio 2015 Professional ISO \n https://go.microsoft.com/fwlink/?LinkId=615436 \n\n Visual Studio 2015 Enterprise Web Installer \n https://go.microsoft.com/fwlink/?LinkId=615437 \n\n Visual Studio 2015 Enterprise ISO \n https://go.microsoft.com/fwlink/?LinkId=615436 \n\n\n Visual Studio 2013 Update 5 ISO \n https://go.microsoft.com/fwlink/?LinkId=532504";
            }

            break;
        case 4:

            switch (index2) {
                case 1:
                case 2:

                    var langGroup = ["ar-sa", "bg-bg", "zh-cn", "zh-tw", "hr-hr", "cs-cz", "da-dk", "nl-nl", "en-us", "et-ee", "fi-fi", "fr-fr", "de-de", "el-gr", "he-il", "hi-in", "hu-hu", "id-id", "it-it", "ja-jp", "kk-kz", "ko-kr", "lv-lv", "lt-lt", "ms-my", "nb-no", "pl-pl", "pl-br", "pt-pt", "ro-ro", "ru-ru", "sr-latn-RS", "sk-sk", "sl-SI", "es-es", "sv-se", "th-th", "tr-tr", "uk-ua", "vi-vn"];
                    var langName = ["Arabic", "Bulgarian", "Chinese Simplified", "Chinese Traditional", "Croatian", "Czech", "Danish", "Dutch", "English", "Estonian", "Finnish", "French", "German", "Greek", "Hebrew", "Hindi", "Hungarian", "Indonesian", "Italian", "Japanese", "Kazakh", "Korean", "Latvian", "Lithuanian", "Malay", "Norwegian Bokmål", "Polish", "Portuguese (Brazil)", "Portuguese (Portugal)", "Romanian", "Russian", "Serbian Latin", "Slovak", "Slovenian", "Spanish", "Swedish", "Thai", "Turkish", "Ukrainian", "Vietnamese"];

                    for (var i = 0; i < langGroup.length; i++) {
                        langselect.append(new Option(langName[i], langGroup[i], false, false));
                    }

                    break;

                case 3:

                    pdLinks.value = "Intel\n(Stable) https://go.microsoft.com/fwlink/?linkid=2069148\n(Beta) https://go.microsoft.com/fwlink/?linkid=2069439\n(Dev) https://go.microsoft.com/fwlink/?linkid=2069340\n(Canary) https://go.microsoft.com/fwlink/?linkid=2069147";

                    break;
                case 4:

                    pdLinks.value = "Apple\n(Stable) https://go.microsoft.com/fwlink/?linkid=2093504\n(Beta) https://go.microsoft.com/fwlink/?linkid=2099618\n(Dev) https://go.microsoft.com/fwlink/?LinkId=2099619\n(Canary) https://go.microsoft.com/fwlink/?LinkId=2093293";

                    break;

                case 5:

                    pdLinks.value = "Debian / Ubuntu \n(Stable) https://go.microsoft.com/fwlink/?linkid=2149051\n(Beta) https://go.microsoft.com/fwlink/?linkid=2149139\n(Dev) https://go.microsoft.com/fwlink/?linkid=2124602";

                    break;
                case 6:

                    pdLinks.value = "Fedora / openSUSE \n(Stable) https://go.microsoft.com/fwlink/?linkid=2149137\n(Beta) https://go.microsoft.com/fwlink/?linkid=2149138\n(Dev) https://go.microsoft.com/fwlink/?linkid=2124702";

                    break;
            }

            break;
    }
}

function newGuid() {
    var guid = "";
    for (var i = 1; i <= 32; i++) {
        var n = Math.floor(Math.random() * 16.0).toString(16);
        guid += n;
        if ((i == 8) || (i == 12) || (i == 16) || (i == 20))
            guid += "-";
    }
    return guid;
}
var officeMacVersion;
function GetMacVer() {
    officeMacVersion = "16.71.23031200";
    var mspallMAC = document.querySelector("#offMacValue").value;
    if (mspallMAC != "") {
        var tdiv = document.createElement("div");
        tdiv.innerHTML = mspallMAC;

        var tlinks = tdiv.querySelectorAll("a");
        var offindex = Array.from(tlinks).findIndex(p => p.href.includes("https://officecdn.microsoft.com/pr/C1297A47-86C4-4C1F-97FA-950631F94777") && p.innerText.includes("包"));
        var tresult = tlinks[offindex].href;
        const match = tresult.match(/(\d+\.\d+\.\d+)/);

        if (match) {
            officeMacVersion = match[1];
        }
    }

    document.querySelector("#OMacPanel").innerHTML = "";
}

function LangSelectChanged() {
    var index = document.querySelector("#ProdSelect").selectedIndex;
    var index2 = document.querySelector("#VerSelect").selectedIndex;
    var index3 = document.querySelector("#LangSelect").selectedIndex;
    if (index < 1 || index2 < 1 || index3 < 1) {
        alert("请检查您选择的选项！");
        return;
    }

    var vselect = document.querySelector("#VerSelect");
    var langselect = document.querySelector("#LangSelect");
    var pdLinks = document.querySelector("#ProdLinks");
    pdLinks.value = "";

    switch (index) {
        case 1:

            $.ajax({
                type: "get",
                url: "https://www.microsoft.com/software-download-connector/api/GetProductDownloadLinksBySku?profile=606624d44113&ProductEditionId=undefined&SKU=" + langselect.value + "&friendlyFileName=undefined&Locale=zh-CN&sessionID=" + msGUID,
                error: function () {
                    alert("发送请求时发生异常，请稍后再试！");
                    return;
                }
            }).then(function (data) {
                try {
                    var tdata = JSON.parse(data);
                    var tproducts = tdata.ProductDownloadOptions;

                    var tstr = "";
                    for (var i = 0; i < tproducts.length; i++) {
                        tstr += "产品名称：" + tproducts[i].ProductDisplayName + "\n下载链接：" + tproducts[i].Uri + "\n\n";
                    }
                    tstr += "\n过期时间：" + tdata.DownloadExpirationDatetime;
                    pdLinks.value = tstr;
                }
                catch
                {
                    alert("获取直链失败！");
                }
            });


            break;
        case 2:

            if (vselect.value == "8") {

                pdLinks.value = "https://officecdn.microsoft.com/pr/MacOffice2011/" + langselect.value+"/MicrosoftOffice2011.dmg";
                return;
            }

            var officeDL = new Array();
            var todata = " https://officecdn.microsoft.com/db/{0}/media/{1}/ProPlus{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/ProjectPro{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/VisioPro{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/HomeStudent{2}Retail.img* " +"\n"+
                " https://officecdn.microsoft.com/db/{0}/media/{1}/HomeBusiness{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/Access{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/Excel{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/Outlook{2}Retail.img* " + "\n" +
                " https://officecdn.microsoft.com/db/{0}/media/{1}/PowerPoint{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/ProjectStd{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/Professional{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/Publisher{2}Retail.img* " + "\n" +
                " https://officecdn.microsoft.com/db/{0}/media/{1}/VisioStd{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/Word{2}Retail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/O365HomePremRetail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/O365BusinessRetail.img* https://officecdn.microsoft.com/db/{0}/media/{1}/O365ProPlusRetail.img";
            var tnoVP = ["bg-bg", "hr-hr", "et-ee", "hi-in", "id-id", "kk-kz", "lv-lv", "lt-lt", "ms-my", "sr-latn-RS", "th-th", "vi-vn"];

            if (vselect.value == "5") {
                todata = todata.replaceAll("{0}", "492350F6-3A01-4F97-B9C0-C7C6DDF67D60").replaceAll("{1}", langselect.value);
                officeDL = todata.split('*');
                pdLinks.value = officeDL[officeDL.length - 1] +"\n"+ officeDL[officeDL.length - 2] +"\n"+ officeDL[officeDL.length - 3];

                return;
            }

            officeDL = todata.split('*');
            for (var t = 0; t < tnoVP.length; t++) {
                if (langselect.value == tnoVP[t]) {

                    officeDL.splice(12, 1);
                    officeDL.splice(9, 1);
                    officeDL.splice(1, 2);

                    break;
                }
            }
            if (vselect.value == "4") {
                var tov = officeDL.toString().replaceAll("{0}", "39168D7E-077B-48E7-872C-B232C3E72675").replaceAll("{1}", langselect.value).replaceAll("{2}", "");
                officeDL = tov.split(',');
                officeDL.splice(0, 1);
                officeDL.splice(officeDL.length - 2, 2);

                for (var t = 0; t < officeDL.length; t++) {
                    pdLinks.value += officeDL[t] + "\n";
                }

                return;
            }

            if (vselect.value == "9") {
                var tov = officeDL.toString().replaceAll("{0}", "492350F6-3A01-4F97-B9C0-C7C6DDF67D60").replaceAll("{1}", langselect.value).replaceAll("{2}", "2024");
                officeDL = tov.split(",");

                officeDL.splice(3, 1);
                officeDL.splice(officeDL.length - 7, 2);

            }
            else if (vselect.value == "1") {
                var tov = officeDL.toString().replaceAll("{0}", "492350F6-3A01-4F97-B9C0-C7C6DDF67D60").replaceAll("{1}", langselect.value).replaceAll("{2}", "2021");
                officeDL = tov.split(",");
            }
            else if (vselect.value == "2") {
                var tov = officeDL.toString().replaceAll("{0}", "492350F6-3A01-4F97-B9C0-C7C6DDF67D60").replaceAll("{1}", langselect.value).replaceAll("{2}", "2019");
                officeDL = tov.split(",");
            }
            else {
                var tov = officeDL.toString().replaceAll("{0}", "492350F6-3A01-4F97-B9C0-C7C6DDF67D60").replaceAll("{1}", langselect.value).replaceAll("{2}", "");
                officeDL = tov.split(",");
            }

            officeDL.splice(officeDL.length - 3, 3);

            for (var t = 0; t < officeDL.length; t++) {
                pdLinks.value += officeDL[t] + "\n";
            }


            break;
        case 4:

            if (index2 == 1) {

                var tedgelink = "Windows 11/10 \nhttps://go.microsoft.com/fwlink/?linkid=2108834&Channel=Stable&language={0}\nhttps://go.microsoft.com/fwlink/?linkid=2108834&Channel=Beta&language={0}\nhttps://go.microsoft.com/fwlink/?linkid=2108834&Channel=Dev&language={0}\nhttps://go.microsoft.com/fwlink/?linkid=2108834&Channel=Canary&language={0}";
                pdLinks.value = tedgelink.replaceAll("{0}", langselect.value);

            }
            else if (index2 == 2) {

                var tedgelink = "Windows 8.1/8/7 \nhttps://go.microsoft.com/fwlink/?linkid=2109047&Channel=Stable&language={0}\nhttps://go.microsoft.com/fwlink/?linkid=2109047&Channel=Beta&language={0}\nhttps://go.microsoft.com/fwlink/?linkid=2109047&Channel=Dev&language={0}\nhttps://go.microsoft.com/fwlink/?linkid=2109047&Channel=Canary&language={0}";
                pdLinks.value = tedgelink.replaceAll("{0}", langselect.value);

            }

            break;

    }
}


function GetMSNewVerify() {
    msGUID = newGuid();
    $.ajax({
        type: "get",
        dataType: "jsonp", jsonp: "callback",
        url: "https://vlscppe.microsoft.com/fp/tags.js?org_id=y6jn8c31&session_id=" + msGUID,
        referer: "https://www.microsoft.com/zh-cn/software-download/windows11",
        error: function () {
            //暂时不处理
        }
    });
}

