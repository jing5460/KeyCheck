var usertoken = "";
window.onload = function () {

}

function keyboxF() {
    document.querySelector(".keybox").placeholder = "";
}
function keyboxB() {
    document.querySelector(".keybox").placeholder = "Product Key";
}


function checkText() {
    if (document.getElementById('keybox').Value.length != 0) {
        document.getElementById('clearBtn').hidden = false;
    }
    else {
        document.getElementById('clearBtn').hidden = true;
    }
}
function clearText() {
    document.getElementById('keybox').Value = "";
}

function CheckKeys() {
    ResetUIState();
    var keys = document.querySelector("#keybox").value;   
    var coem = document.querySelector("#coOEM").checked ? 1 : 0;
    var skipdb = document.querySelector("#coValid").checked ? 1 : 0;
    var getsimp = document.querySelector("#coSimp").checked ? 1 : 0;
    var makcode = document.querySelector("#coMAKCode").checked ? 1 : 0;
    var cindex = document.querySelector("#ConfigList").value;   

    document.querySelector("#submit").style.display = "none";

    if (keys == undefined || keys == "") {
        document.querySelector("#submit").style.display = "block";
        return;
    }
    $.ajax({
        type: "post",
        url: "/api/Check?k=" + keys + "&coem=" + coem + "&skipdb=" + skipdb + "&getsimp=" + getsimp + "&makcode=" + makcode + "&cindex=" + cindex + "&token=" + usertoken,
        error: function () {
            alert("出现异常！请稍后重试");
            document.querySelector("#submit").style.display = "block";
        }
    }).then(function (value) {
        if (value != undefined) {
            var parser = new DOMParser();
            var xmlDoc = parser.parseFromString(value, "text/xml");
            ReadCheckResult(xmlDoc);
        }
        else {
            alert("获取数据失败！");
        }

        document.querySelector("#submit").style.display = "block";

    });
}
function CheckClearBtn() {
    document.querySelector(".keybox").value = "";
}
function CheckMoreBtn() {
    var visible = document.querySelector('.optionPanel').style.display;
    if (visible == "none") {
        document.querySelector('.optionPanel').style.display = "";
        document.querySelector('.optionTD').style.display = "block";
    }
    else {
        document.querySelector('.optionPanel').style.display = "none";
        document.querySelector('.optionTD').style.display = "inherit";
    }

}

function ShareKeyBtn() {

    document.querySelector("#TopBkg").style.display = "block";

    document.querySelector("#TopBkgText").innerText = "您确定要分享当前密钥吗？";
    document.querySelector("#SettingPanel").style.display = "block";
    document.querySelector("#BCancelBtn").style.display = "";

}
function BOKBtnFunc() {
    if (document.querySelector("#SettingPanel").style.display == "block") {
        var sname = document.querySelector("#ShareNameT").value;

        $.ajax({
            type: "post",
            url: "/api/SK?sname=" + sname + "&token=" + usertoken,
            error: function () {
                alert("出现异常！请稍后重试");
                ResetUIState();
            }
        }).then(function (value) {
            if (value != undefined) {
                document.querySelector("#TopBkgText").innerText = value;
                document.querySelector("#SettingPanel").style.display = "none";
                document.querySelector("#BCancelBtn").style.display = "none";
            }
            else {
                alert("获取分享结果失败！");
                ResetUIState();
            }

        });
    } else {
        ResetUIState();
    }
}
function BCancelBtnFunc() {
    ResetUIState();
}
function CopyResult() {
    var result="";
    if (document.querySelector("#CKResult").style.display == "none") {
        result = "Key： " + document.querySelector(".keybox").value + "\n" + "产品名称： " + document.querySelector(".KeyProduN").value
            + "\n" + "Edition ID： " + document.querySelector(".KeyEdition").value + "\n" + "ePID： " + document.querySelector(".ePID").value
            + "\n" + "产品ID： " + document.querySelector(".ProductID").value + "\n" + "SKU ID： " + document.querySelector(".KeySKU").value
            + "\n" + "授权类型： " + document.querySelector(".LicenseType").value + "\n" + "授权通道： " + document.querySelector(".LicenseChannel").value
            + "\n" + "Part Number： " + document.querySelector(".PartNum").value + "\n" + "可用次数/错误代码： " + document.querySelector(".ActiveCount").value;
    }
    else {
        result = document.querySelector("#CKResult").value;
    }

    var newTB = document.createElement("textarea");
    newTB.value = result;
    document.body.appendChild(newTB);

    newTB.select();
    document.execCommand("copy");

    document.body.removeChild(newTB);
}
function CopySimpResult() {
    if (document.querySelector("#CKResult").style.display == "none") {
        var result = "Key： " + document.querySelector(".keybox").value + "\n" + "产品名称： " + document.querySelector(".KeyProduN").value
            + "\n" + "授权类型： " + document.querySelector(".LicenseType").value + "\n" + "可用次数/错误代码： " + document.querySelector(".ActiveCount").value;

        var newTB = document.createElement("textarea");
        newTB.value = result;
        document.body.appendChild(newTB);

        newTB.select();
        document.execCommand("copy");

        document.body.removeChild(newTB);
    }
}

function ReadCheckResult(xmlDoc) {
    try {
        if (xmlDoc.getElementsByTagName("IsShowPopup")[0].textContent == "false") {

            var resultList = xmlDoc.getElementsByTagName("CheckResultList")[0].getElementsByTagName("CheckResultModel");

            if (xmlDoc.getElementsByTagName("CheckKeyNumMode")[0].textContent == "0") {
                //单个密钥
                document.querySelector("#ResultTable").style.display = "block";
                document.querySelector("#CKResult").style.display = "none";

                if (resultList.length > 0) {
                    var item = resultList[0];
                    document.querySelector("#KeyConfig").value = item.getElementsByTagName("KeyConfig")[0] != undefined ? item.getElementsByTagName("KeyConfig")[0].textContent : "";
                    document.querySelector("#KeyProduN").value = item.getElementsByTagName("KeyProduN")[0] != undefined ? item.getElementsByTagName("KeyProduN")[0].textContent : "";
                    document.querySelector("#KeyEdition").value = item.getElementsByTagName("KeyEdition")[0] != undefined ? item.getElementsByTagName("KeyEdition")[0].textContent : "";
                    document.querySelector("#ePID").value = item.getElementsByTagName("ePID")[0] != undefined ? item.getElementsByTagName("ePID")[0].textContent : "";
                    document.querySelector("#ProductID").value = item.getElementsByTagName("ProductID")[0] != undefined ? item.getElementsByTagName("ProductID")[0].textContent : "";
                    document.querySelector("#KeySKU").value = item.getElementsByTagName("KeySKU")[0] != undefined ? item.getElementsByTagName("KeySKU")[0].textContent : "";
                    document.querySelector("#LicenseType").value = item.getElementsByTagName("LicenseType")[0] != undefined ? item.getElementsByTagName("LicenseType")[0].textContent : "";
                    document.querySelector("#LicenseChannel").value = item.getElementsByTagName("LicenseChannel")[0] != undefined ? item.getElementsByTagName("LicenseChannel")[0].textContent : "";
                    document.querySelector("#PartNum").value = item.getElementsByTagName("PartNum")[0] != undefined ? item.getElementsByTagName("PartNum")[0].textContent : "";
                    document.querySelector("#ActiveCount").value = item.getElementsByTagName("ActiveCount")[0] != undefined? item.getElementsByTagName("ActiveCount")[0].textContent:"";
                }

            }
            else {
                //多个密钥
                document.querySelector("#ResultTable").style.display = "none";
                document.querySelector("#CKResult").style.display = "block";

                var checkresultStr = "已经从您粘贴的文本上获得了" + resultList.length + "个密钥，下面是检测结果\n\n\n";
                for (var i = 0; i < resultList.length; i++) {
                    var item = resultList[i];
                    checkresultStr += "Key： " + item.getElementsByTagName("ProductKey")[0].textContent + "\n";

                    if (item.getElementsByTagName("IsShowPopup")[0].textContent == "false") {
                        if (item.getElementsByTagName("KeyConfig")[0] != undefined) {
                            checkresultStr += "证书名称：" + item.getElementsByTagName("KeyConfig")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("KeyProduN")[0] != undefined) {
                            checkresultStr += "产品名称：" + item.getElementsByTagName("KeyProduN")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("KeyEdition")[0] != undefined) {
                            checkresultStr += "产品版本：" + item.getElementsByTagName("KeyEdition")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("ePID")[0] != undefined) {
                            checkresultStr += "ePID：" + item.getElementsByTagName("ePID")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("ProductID")[0] != undefined) {
                            checkresultStr += "产品ID：" + item.getElementsByTagName("ProductID")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("KeySKU")[0] != undefined) {
                            checkresultStr += "SKU ID：" + item.getElementsByTagName("KeySKU")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("LicenseType")[0] != undefined) {
                            checkresultStr += "授权类型：" + item.getElementsByTagName("LicenseType")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("LicenseChannel")[0] != undefined) {
                            checkresultStr += "授权通道：" + item.getElementsByTagName("LicenseChannel")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("PartNum")[0] != undefined) {
                            checkresultStr += "Part Number：" + item.getElementsByTagName("PartNum")[0].textContent + "\n";
                        }
                        if (item.getElementsByTagName("ActiveCount")[0] != undefined) {
                            checkresultStr += "可用次数/错误代码：" + item.getElementsByTagName("ActiveCount")[0].textContent + "\n\n";
                        }
                        else {
                            checkresultStr += "\n";
                        }

                    } else {
                        checkresultStr += item.getElementsByTagName("ShowMessage")[0].textContent + "\n\n";
                    }
                }
                checkresultStr += "\n All done. 以上是所有检测数据。";

                document.querySelector("#CKResult").value = checkresultStr;
            }

        }
        else {
            document.querySelector("#TopBkg").style.display = "block";

            document.querySelector("#TopBkgText").innerText = xmlDoc.getElementsByTagName("ShowMessage")[0].textContent;
            document.querySelector("#SettingPanel").style.display = xmlDoc.getElementsByTagName("IsTextboxVisible")[0].textContent == "false" ? "none" : "block";
            document.querySelector("#BCancelBtn").style.display = xmlDoc.getElementsByTagName("IsCancelButtonVisible")[0].textContent == "false" ? "none" : "";

            document.querySelector("#ResultTable").style.display = "block";
            document.querySelector("#CKResult").style.display = "none";

        }
    }
    catch (ex) {
        alert("解析数据失败！");
        console.log(ex);
    }
}
function ResetUIState() {
    document.querySelector("#TopBkg").style.display = "none";

    document.querySelector("#TopBkgText").innerText = "";
    document.querySelector("#SettingPanel").style.display = "none";
    document.querySelector("#BCancelBtn").style.display = "none";

    //document.querySelector("#ResultTable").style.display = "block";
    //document.querySelector("#CKResult").style.display = "none";
}

function SKSearchClearBtn() {
    document.querySelector(".CKListSearchTb").value = "";
}
function SKRecheckFunc(key) {
    if (key == undefined || key == "") {
        return;
    }
    disableSKRecheckBtn();
    var coem = document.querySelector("#sk_coOEM").checked ? 1 : 0;

    $.ajax({
        type: "post",
        url: "/api/RecheckSK?k=" + key + "&coem=" + coem + "&token=" + usertoken,
        error: function () {
            alert("出现异常！请稍后重试");
            enableSKRecheckBtn();
        }
    }).then(function (value) {
        if (value != undefined) {
            var parser = new DOMParser();
            var xmlDoc = parser.parseFromString(value, "text/xml");
            ReadSKRecheckResult(xmlDoc);
        }
        else {
            alert("获取数据失败！");
        }

        enableSKRecheckBtn();
    });
}
function SKQRCodeFunc(key) {
    var tdiv = document.createElement("div");
    tdiv.innerHTML = "<canvas id=\"keyqrcode\"></canvas>";
    tdiv.className = "qrcodepanel";
    tdiv.onclick = removeSKQRCode;
    document.body.appendChild(tdiv);

    var tcount;
    var tpn;
    var elements = document.querySelectorAll(".skrecheckkey");
    var kindex = Array.from(elements).findIndex(el => el.innerText == key);
    if (kindex >= 0) {
        tcount = document.querySelectorAll(".skrecheckcount")[kindex].innerText;
        tpn = document.querySelectorAll(".skrecheckPN")[kindex].innerText;
    }

    var tqrtext = "Key： " + key + "\n" + "产品名称： " + tpn + "\n" + "可用次数/错误代码： " + tcount + "\n" + "本地时间： " + Date().toLocaleString();
    //var tqrtext = key + "\n" + tpn + "\n" + tcount + "\n" + Date().toLocaleString();
    QRCode.toCanvas(document.querySelector("#keyqrcode"), tqrtext, { errorCorrectionLevel: 'L', width: 200, height: 200 });
    //new QRCode(document.querySelector("#keyqrcode"), { text: tqrtext, width: 200, height: 200 });
    //document.querySelector("#keyqrcode").title = "Key";
}

function ReadSKRecheckResult(xmlDoc) {
    try {
        if (xmlDoc.getElementsByTagName("Status")[0].textContent == "false") {
            alert(xmlDoc.getElementsByTagName("ReturnMessage")[0].textContent);
        }
        else {
            var returnvalueCR = xmlDoc.getElementsByTagName("CheckResult")[0].textContent;
            var returnvalueK = xmlDoc.getElementsByTagName("Key")[0].textContent;

            var elements = document.querySelectorAll(".skrecheckkey");
            var kindex = Array.from(elements).findIndex(el => el.innerText == returnvalueK);
            if (kindex >= 0) {
                document.querySelectorAll(".skrecheckcount")[kindex].innerText = returnvalueCR;
            }
            else {
                alert("未更新界面数据，请尝试刷新页面！");
            }
        }
    }
    catch (ex) {
        alert("解析数据失败！");
        console.log(ex);
    }
}
function disableSKRecheckBtn() {
    document.querySelectorAll(".RecheckBtn").forEach(btn => { btn.disabled = true; })
}
function enableSKRecheckBtn() {
    document.querySelectorAll(".RecheckBtn").forEach(btn => { btn.disabled = false; })
}

function removeSKQRCode() {
    var qrcdiv = document.querySelector(".qrcodepanel");
    if (qrcdiv != undefined) {
        document.body.removeChild(qrcdiv);
    }
}
function DQBlockedKeyFunc() {
    var querystr = document.getElementById("BlockedKeyTb").value;
    var messageDiv = document.getElementById("BKMsgPanel");
    var tableBody = document.querySelector("#BKResultTable tbody");

    // 校验输入
    if (querystr.length < 3) {
        messageDiv.className = "alert alert-warning";
        messageDiv.innerText = "搜索字符串不得少于3个字符";
        messageDiv.classList.remove("d-none");
        return;
    }

    // 清空旧数据
    tableBody.innerHTML = "";
    messageDiv.classList.add("d-none");
    messageDiv.innerText = "";
    document.querySelector("#DQBlockedKeyBtn").style.display = "none";

    try {
        $.ajax({
            type: "post",
            url: "/api/BlockKeyQuery?q=" + querystr + "&token=" + usertoken,
            error: function () {
                DQTableFailUI("出现异常！请稍后重试");
            }
        }).then(function (value) {
            if (value != undefined) {
                var parser = new DOMParser();
                var xmlDoc = parser.parseFromString(value, "text/xml");
                ReadQueryBlockedKeyResult(xmlDoc);

                document.querySelector("#DQBlockedKeyBtn").style.display = "block";
            }
            else {
                DQTableFailUI("获取数据失败！");
            }

        });

    } catch {
        DQTableFailUI("获取数据时发生异常，请稍后再试");
    }
}

function DQTableFailUI(str) {
    var messageDiv = document.getElementById("BKMsgPanel");
    document.querySelector("#DQBlockedKeyBtn").style.display = "block";
    document.querySelector("#BKQueryCountTb").innerText = "";

    messageDiv.className = "alert alert-danger";
    messageDiv.innerText = str;
    messageDiv.classList.remove("d-none");

}

function ReadQueryBlockedKeyResult(xmlDoc) {
    try {
        if (xmlDoc.getElementsByTagName("Result")[0].textContent == "false") {
            DQTableFailUI(xmlDoc.getElementsByTagName("ReturnMessage")[0].textContent);
        }
        else {
            var keys = xmlDoc.getElementsByTagName("Keys")[0].querySelectorAll("BlockedKeys");
            var tableBody = document.querySelector("#BKResultTable tbody");

            keys.forEach(item => {
                const A = item.querySelector("A")?.textContent ?? "";
                const B = item.querySelector("B")?.textContent ?? "";
                const C = item.querySelector("C")?.textContent ?? "";
                const D = item.querySelector("D")?.textContent ?? "";

                const row = document.createElement("tr");
                row.innerHTML = `
                        <td>${A}</td>
                        <td>${B}</td>
                        <td>${C}</td>
                        <td>${D}</td>
                    `;
                tableBody.appendChild(row);
            });
            document.querySelector("#BKQueryCountTb").innerText = "查询到了" + keys.length+"条数据";
        }
    }
    catch (ex) {
        DQTableFailUI("解析数据失败！");
        console.log(ex);
    }
}