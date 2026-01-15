using Microsoft.Win32;
using PKeyTools.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Serialization;
using static PKeyTools.Model.AllModel;
using static PKeyTools.PKeyConfigModel;
using static PKeyTools.ProductKeyBaseModel;

namespace PKeyTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
        }
        public const string KeyFormat = @"(?!^.*N.*N.*$)([BCDFGHJKMPQRTVWXY2-9N]{5}\-){4}[BCDFGHJKMPQRTVWXY2-9N]{4}[BCDFGHJKMPQRTVWXY2-9]";
        public CheckModel checkModel;
        public PkeyData pkconfigModel;
        public KeyBase keyBase;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadList();
            checkModel = new CheckModel(pkconfigModel.ConfigTypes,GlobalParameters.configFolder);
        }
        public void LoadList()
        {
            pkconfigModel =GlobalParameters.GetPkeyData();

            if (pkconfigModel!=null)
            {
                var selectedIndex= SelectConfig.SelectedIndex;

                List<ConfigType> conList = pkconfigModel.ConfigTypes;
                conList.Insert(0, new ConfigType { ConfigName="自动遍历所有证书", ConfigID=0, ConfigPath="" });
                SelectConfig.ItemsSource = conList;

                SelectConfig.SelectedIndex = selectedIndex<0 ? 0 : selectedIndex;

            }
        }
        private void Test_TextChanged(object sender, TextChangedEventArgs e)
        {
            Test.IsEnabled = false;
            SelectConfig.IsEnabled = false;
            CheckOpt.IsEnabled = false;
            Test.Foreground = new SolidColorBrush(Colors.Black);

            //如果用户是手动输入密钥检测的，则用户输入完一个完整的密钥时就自动开始检测
            if (Test.Text.Length !=29)
            {
                Test.Foreground = new SolidColorBrush(Colors.Red);
                ResetCheckUI();

                return;
            }

            var tproductKey = Test.Text.ToUpper();
            this.Height = 150;
            KeyConfig.Text = KeyProduN.Text = KeyEdition.Text = ePID.Text = ProductID.Text = KeySKU.Text = LicenseType.Text = LicenseChannel.Text = PartNum.Text = ActiveCount.Text = "";

            var alltext=File.ReadAllText(GlobalParameters.GetKeyBaseFilePath());
            if (alltext.Contains(FileTrans.PAAA(tproductKey)))
            {
                MessageBoxResult result = MessageBox.Show("该密钥 " + tproductKey + " 已经在您的数据库里了,是否重新检测？", "提示", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    ResetCheckUI();

                    return;
                }
            }

            RefreshCheckModelData();

            MsgTitle.Text= "正在检测密钥信息,请耐心等待...";     
            var tconfigID = (SelectConfig.SelectedItem as ConfigType).ConfigID;
            int optionOEM = coOEM.IsChecked==true ? 1 : 0;
            int optionSimp = coSimp.IsChecked==true ? 1 : 0;
            int optionMAKCode = coMAKCode.IsChecked==true ? 1 : 0;

            Task.Run(async ()=>
            {
                List<CheckResultModel> checkResult = await checkModel.CheckKeysClient(tproductKey,optionOEM , 0, optionSimp, optionMAKCode, tconfigID);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MsgTitle.Text= "检测完成";
                });

                if (checkResult != null && checkResult.Count > 0)
                {
                    var checkDetail = checkResult[0];

                    if (checkDetail.IsShowPopup)
                    {
                        MessageBox.Show(checkDetail.ShowMessage, "提示");
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            KeyConfig.Text = checkDetail.KeyConfig;
                            KeyProduN.Text = checkDetail.KeyProduN;
                            KeyEdition.Text = checkDetail.KeyEdition;
                            ePID.Text = checkDetail.ePID;
                            ProductID.Text = checkDetail.ProductID;
                            KeySKU.Text = checkDetail.KeySKU;
                            LicenseType.Text = checkDetail.LicenseType;
                            LicenseChannel.Text = checkDetail.LicenseChannel;
                            PartNum.Text = checkDetail.PartNum;
                            ActiveCount.Text = checkDetail.ActiveCount;
                        });

                        if (checkDetail.ActiveCount.Contains("Blocked")||checkDetail.ActiveCount.Contains("0xC004C003")||checkDetail.ActiveCount.Contains("0xC004C060"))
                        {
                            RemoveKey(checkDetail);
                            MessageBox.Show("当前密钥 " + checkDetail.ProductKey + " 检测为被封密钥，已从本地密钥库中移除！", "提示");
                        }
                        else
                        {
                            AddOrUpdateKey(checkDetail);
                        }

                    }

                }

                //检测完成后需要切换单个密钥和多个密钥的UI状态
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TestPanel.Visibility=(Visibility)2;
                    Panel1.Visibility=0;
                    Panel2.Visibility=(Visibility)2;

                    ResetCheckUI();
                });
            });

        }

        public void ResetCheckUI()
        {
            Test.IsEnabled = true;
            SelectConfig.IsEnabled = true;
            CheckOpt.IsEnabled = true;

            this.Height = 450;
            MsgTitle.Text = "";
        }
        /// <summary>
        /// 每进行一次检测都要把最新的数据传给CheckModel，避免数据被修改发生异常
        /// </summary>
        public void RefreshCheckModelData()
        {
            LoadList();
            checkModel.configList=pkconfigModel.ConfigTypes;

            keyBase =GlobalParameters.GetKeyBaseData();
        }
        public void AddOrUpdateKey(CheckResultModel checkResult)
        {
            if(string.IsNullOrWhiteSpace(checkResult.ActiveCount))
            {
                return;
            }
            if (keyBase!=null)
            {
                try
                {
                    ProductKeysCollection tProdType = null;
                    string licenseTypeTag = "";
                    string productTypeTag = "";

                    if (checkResult.LicenseType.Contains("Retail") ||checkResult.LicenseType.Contains("OEM"))
                    {
                        if (keyBase.RetailLicense != null)
                        {
                            licenseTypeTag="Retail-License";

                            var retailLicense = keyBase.RetailLicense;
                            if(checkResult.KeyProduN.Contains("Server"))
                            {
                                tProdType= retailLicense.ServerKeys;
                                productTypeTag="Server-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Win") || checkResult.KeyProduN.Contains("Windows"))
                            {
                                tProdType= retailLicense.WindowsKeys;
                                productTypeTag="Windows-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Office")||checkResult.KeyProduN.StartsWith("RTM"))
                            {
                                tProdType= retailLicense.OfficeKeys;
                                productTypeTag="Office-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Visual Studio"))
                            {
                                tProdType= retailLicense.VSKeys;
                                productTypeTag="VS-Keys";
                            }
                            else
                            {
                                tProdType= retailLicense.OtherKeys;
                                productTypeTag="Other-Keys";
                            }

                        }

                    }
                    else if (checkResult.LicenseType.Contains("Volume:CSVLK") || checkResult.LicenseType.Contains("Volume:GVLK"))
                    {
                        if (keyBase.KMSLicense != null)
                        {
                            licenseTypeTag="KMS-License";

                            var kmsLicense = keyBase.KMSLicense;
                            if (checkResult.KeyProduN.Contains("Server"))
                            {
                                tProdType= kmsLicense.ServerKeys;
                                productTypeTag="Server-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Win") || checkResult.KeyProduN.Contains("Windows"))
                            {
                                tProdType= kmsLicense.WindowsKeys;
                                productTypeTag="Windows-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Office")||checkResult.KeyProduN.StartsWith("RTM"))
                            {
                                tProdType= kmsLicense.OfficeKeys;
                                productTypeTag="Office-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Visual Studio"))
                            {
                                tProdType= kmsLicense.VSKeys;
                                productTypeTag="VS-Keys";
                            }
                            else
                            {
                                tProdType= kmsLicense.OtherKeys;
                                productTypeTag="Other-Keys";
                            }
                        }
                    }
                    else if (checkResult.LicenseType.Contains("Volume"))
                    {
                        if (keyBase.MAKLicense != null)
                        {
                            licenseTypeTag="MAK-License";

                            var makLicense = keyBase.MAKLicense;
                            if (checkResult.KeyProduN.Contains("Server"))
                            {
                                tProdType= makLicense.ServerKeys;
                                productTypeTag="Server-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Win") || checkResult.KeyProduN.Contains("Windows"))
                            {
                                tProdType= makLicense.WindowsKeys;
                                productTypeTag="Windows-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Office")||checkResult.KeyProduN.StartsWith("RTM"))
                            {
                                tProdType= makLicense.OfficeKeys;
                                productTypeTag="Office-Keys";
                            }
                            else if (checkResult.KeyProduN.Contains("Visual Studio"))
                            {
                                tProdType= makLicense.VSKeys;
                                productTypeTag="VS-Keys";
                            }
                            else
                            {
                                tProdType= makLicense.OtherKeys;
                                productTypeTag="Other-Keys";
                            }
                        }
                    }
                    else
                    {
                        if (keyBase.OtherLicense != null)
                        {
                            licenseTypeTag="Other-License";

                            tProdType = keyBase.OtherLicense;
                            productTypeTag="Other-Keys";
                        }
                    }

                    if (tProdType!=null)
                    {
                        ProductKeyBaseModel.Key foundKey = tProdType.Keys.FirstOrDefault(p => p.ProductKey == FileTrans.PAAA(checkResult.ProductKey));
                        if (foundKey!=null)
                        {
                            foundKey.ConfigID = checkResult.ConfigID;   //仅测试使用
                            foundKey.ActiveCount = checkResult.ActiveCount;
                        }
                        else
                        {
                            ProductKeyBaseModel.Key newKey = new ProductKeyBaseModel.Key
                            {
                                ConfigID = checkResult.ConfigID,
                                EPID = checkResult.ePID,
                                ActiveCount = checkResult.ActiveCount,
                                ProductKey = FileTrans.PAAA(checkResult.ProductKey),
                                ProductName = checkResult.KeyProduN,
                                LicenseTypeTag = licenseTypeTag,
                                ProductTypeTag =productTypeTag 
                            };
                            tProdType.Keys.Add(newKey);
                        }
    
                        GlobalParameters.SaveKeyBaseFile(keyBase);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("检索本地密钥库时发生异常，请重启软件稍后再试！");
                }
            }
            else
            {
                MessageBox.Show("无法检索到本地密钥库，请重启软件或重置密钥库！");
            }
        }
        public void RemoveKey(CheckResultModel checkResult)
        {
            if (keyBase!=null)
            {
                try
                {
                    ProductKeysCollection tProdType = null;

                    if (checkResult.LicenseType.Contains("Retail") ||checkResult.LicenseType.Contains("OEM"))
                    {
                        if (keyBase.RetailLicense != null)
                        {
                            var retailLicense = keyBase.RetailLicense;
                            if(checkResult.KeyProduN.Contains("Server"))
                            {
                                tProdType= retailLicense.ServerKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Win") || checkResult.KeyProduN.Contains("Windows"))
                            {
                                tProdType= retailLicense.WindowsKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Office")||checkResult.KeyProduN.StartsWith("RTM"))
                            {
                                tProdType= retailLicense.OfficeKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Visual Studio"))
                            {
                                tProdType= retailLicense.VSKeys;
                            }
                            else
                            {
                                tProdType= retailLicense.OtherKeys;
                            }

                        }

                    }
                    else if (checkResult.LicenseType.Contains("Volume:CSVLK") || checkResult.LicenseType.Contains("Volume:GVLK"))
                    {
                        if (keyBase.KMSLicense != null)
                        {
                            var kmsLicense = keyBase.KMSLicense;
                            if (checkResult.KeyProduN.Contains("Server"))
                            {
                                tProdType= kmsLicense.ServerKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Win") || checkResult.KeyProduN.Contains("Windows"))
                            {
                                tProdType= kmsLicense.WindowsKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Office")||checkResult.KeyProduN.StartsWith("RTM"))
                            {
                                tProdType= kmsLicense.OfficeKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Visual Studio"))
                            {
                                tProdType= kmsLicense.VSKeys;
                            }
                            else
                            {
                                tProdType= kmsLicense.OtherKeys;
                            }
                        }
                    }
                    else if (checkResult.LicenseType.Contains("Volume"))
                    {
                        if (keyBase.MAKLicense != null)
                        {
                            var makLicense = keyBase.MAKLicense;
                            if (checkResult.KeyProduN.Contains("Server"))
                            {
                                tProdType= makLicense.ServerKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Win") || checkResult.KeyProduN.Contains("Windows"))
                            {
                                tProdType= makLicense.WindowsKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Office")||checkResult.KeyProduN.StartsWith("RTM"))
                            {
                                tProdType= makLicense.OfficeKeys;
                            }
                            else if (checkResult.KeyProduN.Contains("Visual Studio"))
                            {
                                tProdType= makLicense.VSKeys;
                            }
                            else
                            {
                                tProdType= makLicense.OtherKeys;
                            }
                        }
                    }
                    else
                    {
                        if (keyBase.OtherLicense != null)
                        {
                            tProdType = keyBase.OtherLicense;
                        }
                    }

                    if (tProdType!=null)
                    {
                        ProductKeyBaseModel.Key foundKey = tProdType.Keys.FirstOrDefault(p => p.ProductKey == FileTrans.PAAA(checkResult.ProductKey));
                        if (foundKey!=null)
                        {
                            tProdType.Keys.Remove(foundKey);

                            GlobalParameters.SaveKeyBaseFile(keyBase);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("检索本地密钥库时发生异常，请重启软件稍后再试！");
                }
            }
            else
            {
                MessageBox.Show("无法检索到本地密钥库，请重启软件或重置密钥库！");
            }
        }

        private void KeySelectBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Test.TextChanged-=Test_TextChanged;

            var content = Clipboard.GetText().ToUpper();
            MatchCollection abc = Regex.Matches(content, CheckModel.KeyFormat);
            if(abc.Count>0)
            {
                var keyList = abc.GroupBy(p => p.Value).Select(g => g.First()).ToList();

                if (keyList.Count>1)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RawResult.Text = "已经从您粘贴的文本上获得了" + keyList.Count + "个密钥，下面是检测结果\n\n\n";
                    });
                }

                Test.IsEnabled = false;
                SelectConfig.IsEnabled = false;
                CheckOpt.IsEnabled = false;

                Test.Text = keyList.Count+" Keys";
                this.Height = 150;
                KeyConfig.Text = KeyProduN.Text = KeyEdition.Text = ePID.Text = ProductID.Text = KeySKU.Text = LicenseType.Text = LicenseChannel.Text = PartNum.Text = ActiveCount.Text = "";

                var alltext = File.ReadAllText(GlobalParameters.GetKeyBaseFilePath());
                RefreshCheckModelData();
                var tconfigID = (SelectConfig.SelectedItem as ConfigType).ConfigID;
                int optionOEM = coOEM.IsChecked==true ? 1 : 0;
                int optionSimp = coSimp.IsChecked==true ? 1 : 0;
                int optionMAKCode = coMAKCode.IsChecked==true ? 1 : 0;

                Task.Run(async ()=>
                {
                    for (int i = 0; i < keyList.Count; i++)
                    {
                        if (alltext.Contains(FileTrans.PAAA(keyList[i].Value)))
                        {
                            MessageBoxResult result = MessageBox.Show("该密钥 " + keyList[i].Value + " 已经在您的数据库里了,是否重新检测？", "提示", MessageBoxButton.YesNo);
                            if (result != MessageBoxResult.Yes)
                            {
                                if (keyList.Count>1)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        RawResult.Text += "密钥 " + keyList[i].Value + " 已跳过检测 \n\n";
                                    });
                                }

                                continue;
                            }
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MsgTitle.Text= "正在检测密钥"+keyList[i].Value+"的信息...";
                        });
                        List<CheckResultModel> checkResult = await checkModel.CheckKeysClient(keyList[i].Value, optionOEM, 0, optionSimp, optionMAKCode, tconfigID);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MsgTitle.Text= "当前密钥检测完成";
                        });

                        if (keyList.Count>1)
                        {
                            if (checkResult != null && checkResult.Count > 0)
                            {
                                var checkDetail = checkResult[0];
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    RawResult.Text += "Key： " + checkDetail.ProductKey +"\n";
                                });

                                if (checkDetail.IsShowPopup)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        RawResult.Text += checkDetail.ShowMessage+"\n\n";
                                    });
                                }
                                else
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(checkDetail.KeyConfig))
                                        {
                                            RawResult.Text += "证书名称：" + checkDetail.KeyConfig + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.KeyProduN))
                                        {
                                            RawResult.Text += "产品名称：" + checkDetail.KeyProduN + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.KeyEdition))
                                        {
                                            RawResult.Text += "产品版本：" + checkDetail.KeyEdition + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.ePID))
                                        {
                                            RawResult.Text += "ePID：" + checkDetail.ePID + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.ProductID))
                                        {
                                            RawResult.Text += "产品ID：" + checkDetail.ProductID + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.KeySKU))
                                        {
                                            RawResult.Text += "SKU ID：" + checkDetail.KeySKU + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.LicenseType))
                                        {
                                            RawResult.Text += "授权类型：" + checkDetail.LicenseType + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.LicenseChannel))
                                        {
                                            RawResult.Text += "授权通道：" + checkDetail.LicenseChannel + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.PartNum))
                                        {
                                            RawResult.Text += "Part Number：" + checkDetail.PartNum + "\n";
                                        }

                                        if (!string.IsNullOrWhiteSpace(checkDetail.ActiveCount))
                                        {
                                            RawResult.Text += "可用次数/错误代码：" + checkDetail.ActiveCount + "\n\n";
                                        }
                                        else
                                        {
                                            RawResult.Text += "\n";
                                        }
                                    });

                                    if (checkDetail.ActiveCount.Contains("Blocked")||checkDetail.ActiveCount.Contains("0xC004C003")||checkDetail.ActiveCount.Contains("0xC004C060"))
                                    {
                                        RemoveKey(checkDetail);
                                        //MessageBox.Show("当前密钥 " + checkDetail.ProductKey + " 检测为被封密钥，已从本地密钥库中移除！", "提示");
                                    }
                                    else
                                    {
                                        AddOrUpdateKey(checkDetail);
                                    }

                                }

                            }

                        }
                        else
                        {
                            if (checkResult != null && checkResult.Count > 0)
                            {
                                var checkDetail = checkResult[0];

                                if (checkDetail.IsShowPopup)
                                {
                                    MessageBox.Show(checkDetail.ShowMessage, "提示");
                                }
                                else
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        KeyConfig.Text = checkDetail.KeyConfig;
                                        KeyProduN.Text = checkDetail.KeyProduN;
                                        KeyEdition.Text = checkDetail.KeyEdition;
                                        ePID.Text = checkDetail.ePID;
                                        ProductID.Text = checkDetail.ProductID;
                                        KeySKU.Text = checkDetail.KeySKU;
                                        LicenseType.Text = checkDetail.LicenseType;
                                        LicenseChannel.Text = checkDetail.LicenseChannel;
                                        PartNum.Text = checkDetail.PartNum;
                                        ActiveCount.Text = checkDetail.ActiveCount;
                                    });

                                    if (checkDetail.ActiveCount.Contains("Blocked")||checkDetail.ActiveCount.Contains("0xC004C003")||checkDetail.ActiveCount.Contains("0xC004C060"))
                                    {
                                        RemoveKey(checkDetail);
                                        MessageBox.Show("当前密钥 " + checkDetail.ProductKey + " 检测为被封密钥，已从本地密钥库中移除！", "提示");
                                    }
                                    else
                                    {
                                        AddOrUpdateKey(checkDetail);
                                    }

                                }

                            }

                        }

                    }

                    if (keyList.Count>1)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RawResult.Text += "\n All done. 以上是所有检测数据。";

                            TestPanel.Visibility=(Visibility)2;
                            Panel1.Visibility=(Visibility)2;
                            Panel2.Visibility=0;

                            ResetCheckUI();
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TestPanel.Visibility=(Visibility)2;
                            Panel1.Visibility=0;
                            Panel2.Visibility=(Visibility)2;

                            ResetCheckUI();
                        });
                    }

                    Test.TextChanged+=Test_TextChanged;
                });

            }
            else
            {
                Test.TextChanged+=Test_TextChanged;
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        Visibility panel1Visible;
        Visibility panel2Visible;
        private void CheckOpt_Click(object sender, RoutedEventArgs e)
        {
            if(TestPanel.Visibility==0)
            {
                Panel1.Visibility = panel1Visible;
                Panel2.Visibility = panel2Visible;

                TestPanel.Visibility =(Visibility)2;
            }
            else
            {
                TestPanel.Visibility = 0;

                panel1Visible=Panel1.Visibility;
                panel2Visible= Panel2.Visibility;

                Panel1.Visibility =(Visibility)2;
                Panel2.Visibility = (Visibility)2;
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            string copyContent;
            if (Panel1.Visibility==0)
            {
                copyContent = KeyConfig.Text + "\n" + KeyProduN.Text + "\n" + KeyEdition.Text + "\n" + ePID.Text + "\n" + ProductID.Text + "\n" + KeySKU.Text + "\n" + LicenseType.Text
                + "\n" + LicenseChannel.Text + "\n" + PartNum.Text + "\n" + ActiveCount.Text;
            }
            else
            {
                copyContent = RawResult.Text;
            }

            Clipboard.SetText(copyContent);
        }

        private void ConfigManager_Click(object sender, RoutedEventArgs e)
        {
            ConfigMP configMP = new ConfigMP();
            configMP.ShowDialog();
        }

        private void KeyManager_Click(object sender, RoutedEventArgs e)
        {
            ProductKP productKP = new ProductKP();
            productKP.ShowDialog();
        }
    }
}
