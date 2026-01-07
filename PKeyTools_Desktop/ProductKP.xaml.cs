using PKeyTools.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using System.Xml;
using System.Xml.XPath;
using static PKeyTools.Model.GetErrorCodeByPost;
using static PKeyTools.PKeyConfigModel;
using static PKeyTools.ProductKeyBaseModel;

namespace PKeyTools
{
    /// <summary>
    /// ProductKP.xaml 的交互逻辑
    /// </summary>
    public partial class ProductKP : Window
    {
        public static ProductKP productKP;
        public ProductKP()
        {
            InitializeComponent();
            productKP = this;
        }
        public KeyBase keyBase;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadList();
        }
        public void LoadList()
        {
            keyBase =GlobalParameters.GetKeyBaseData();

            if (keyBase!=null)
            {
                List<KeySelectModel> list = new List<KeySelectModel>();
                KeySelectBox.ItemsSource=null;


                list.Add(new KeySelectModel() { ShowName="All License", ItemIndex=0 });
                list.Add(new KeySelectModel() { ShowName = "     MAK License ", Name="MAK License", ItemIndex = 1 });
                int makProductCount = 0;
                if (keyBase.MAKLicense != null)
                {
                    var makLicense = keyBase.MAKLicense;
                    var allKeysCollections = new List<KeysCollection>
                {
                    makLicense.OfficeKeys,
                    makLicense.WindowsKeys,
                    makLicense.ServerKeys,
                    makLicense.VSKeys,
                    makLicense.OtherKeys
                };
                    allKeysCollections.ForEach(keysCollection =>
                    {
                        if (keysCollection.Keys != null && keysCollection.Keys.Count > 0)
                        {
                            var items = keysCollection.Keys.GroupBy(k => k.ProductName).Select(g => g).ToList();
                            makProductCount += items.Count;
                            for (int j = 0; j < items.Count; j++)
                            {
                                list.Add(new KeySelectModel() { ShowName = $"                    [{items[j].Count()}]     {items[j].Key}", Name= items[j].Key, ItemIndex = 2, LicenseType="MAK License" });
                            }
                        }
                    });

                }
                var tlist1 = list.FirstOrDefault(p => p.Name=="MAK License");
                tlist1.ShowName += $"[{makProductCount}]";

                list.Add(new KeySelectModel() { ShowName = "     Retail License ", Name="Retail License", ItemIndex = 1 });
                int retailProductCount = 0;
                if (keyBase.RetailLicense != null)
                {
                    var retailLicense = keyBase.RetailLicense;
                    var allKeysCollections = new List<KeysCollection>
                {
                    retailLicense.OfficeKeys,
                    retailLicense.WindowsKeys,
                    retailLicense.ServerKeys,
                    retailLicense.VSKeys,
                    retailLicense.OtherKeys
                };
                    allKeysCollections.ForEach(keysCollection =>
                    {
                        if (keysCollection.Keys != null && keysCollection.Keys.Count > 0)
                        {
                            var items = keysCollection.Keys.GroupBy(k => k.ProductName).Select(g => g).ToList();
                            retailProductCount += items.Count;
                            for (int j = 0; j < items.Count; j++)
                            {
                                list.Add(new KeySelectModel() { ShowName = $"                    [{items[j].Count()}]     {items[j].Key}", Name= items[j].Key, ItemIndex = 2, LicenseType="Retail License" });
                            }
                        }
                    });

                }
                var tlist2 = list.FirstOrDefault(p => p.Name=="Retail License");
                tlist2.ShowName += $"[{retailProductCount}]";

                list.Add(new KeySelectModel() { ShowName = "     KMS License ", Name="KMS License", ItemIndex = 1 });
                int kmsProductCount = 0;
                if (keyBase.KMSLicense != null)
                {
                    var kmsLicense = keyBase.KMSLicense;
                    var allKeysCollections = new List<KeysCollection>
                {
                    kmsLicense.OfficeKeys,
                    kmsLicense.WindowsKeys,
                    kmsLicense.ServerKeys,
                    kmsLicense.VSKeys,
                    kmsLicense.OtherKeys
                };
                    allKeysCollections.ForEach(keysCollection =>
                    {
                        if (keysCollection.Keys != null && keysCollection.Keys.Count > 0)
                        {
                            var items = keysCollection.Keys.GroupBy(k => k.ProductName).Select(g => g).ToList();
                            kmsProductCount += items.Count;
                            for (int j = 0; j < items.Count; j++)
                            {
                                list.Add(new KeySelectModel() { ShowName = $"                    [{items[j].Count()}]     {items[j].Key}", Name= items[j].Key, ItemIndex = 2, LicenseType="KMS License" });
                            }
                        }
                    });

                }
                var tlist3 = list.FirstOrDefault(p => p.Name == "KMS License");
                tlist3.ShowName += $"[{kmsProductCount}]";

                list.Add(new KeySelectModel() { ShowName = "     Other License ", Name="Other License", ItemIndex = 1 });
                int otherProductCount = 0;
                if (keyBase.OtherLicense != null)
                {
                    var otherLicense = keyBase.OtherLicense;
                    if (otherLicense.Keys != null && otherLicense.Keys.Count > 0)
                    {
                        var items = otherLicense.Keys.GroupBy(k => k.ProductName).Select(g => g).ToList();
                        otherProductCount += items.Count;
                        for (int j = 0; j < items.Count; j++)
                        {
                            list.Add(new KeySelectModel() { ShowName = $"                    [{items[j].Count()}]     {items[j].Key}", Name= items[j].Key, ItemIndex = 2, LicenseType="Other License" });
                        }
                    }
                }
                var tlist4 = list.FirstOrDefault(p => p.Name == "Other License");
                tlist4.ShowName += $"[{otherProductCount}]";


                KeySelectBox.ItemsSource=list;
            }
        }
        /*
                 public void InsertItems(string XPath,XmlDocument xmlDocument,string index)
                {
                    KeySelectBox.Items.Add("               Office Keys"+index);
                    XmlNode keyRoot = xmlDocument.SelectSingleNode(XPath+"//Office-Keys");
                    if (keyRoot.ChildNodes.Count > 0)
                    {
                        List<string> list = new List<string>();
                        for (int i = 0; i < keyRoot.ChildNodes.Count; i++)
                        {
                            list.Add(keyRoot.ChildNodes[i].Attributes[3].InnerText);                    
                        }
                        var res = from s in list orderby s group s by s into ss select ss;
                        foreach (IGrouping<string, string> result in res)
                        {
                            KeySelectBox.Items.Add("                         " + result.Key);
                        }
                    }
                    KeySelectBox.Items.Add("               Windows Keys"+index);
                    keyRoot = xmlDocument.SelectSingleNode(XPath+"//Windows-Keys");
                    if (keyRoot.ChildNodes.Count > 0)
                    {
                        List<string> list = new List<string>();
                        for (int i = 0; i < keyRoot.ChildNodes.Count; i++)
                        {
                            list.Add(keyRoot.ChildNodes[i].Attributes[3].InnerText);
                        }
                        var res = from s in list orderby s group s by s into ss select ss;
                        foreach (IGrouping<string, string> result in res)
                        {
                            KeySelectBox.Items.Add("                         " + result.Key);
                        }
                    }
                    KeySelectBox.Items.Add("               Server Keys"+index);
                    keyRoot = xmlDocument.SelectSingleNode(XPath+"//Server-Keys");
                    if (keyRoot.ChildNodes.Count > 0)
                    {
                        List<string> list = new List<string>();
                        for (int i = 0; i < keyRoot.ChildNodes.Count; i++)
                        {
                            list.Add(keyRoot.ChildNodes[i].Attributes[3].InnerText);
                        }
                        var res = from s in list orderby s group s by s into ss select ss;
                        foreach (IGrouping<string, string> result in res)
                        {
                            KeySelectBox.Items.Add("                         " + result.Key);
                        }
                    }
                    KeySelectBox.Items.Add("               VS Keys"+index);
                    keyRoot = xmlDocument.SelectSingleNode(XPath+"//VS-Keys");
                    if (keyRoot.ChildNodes.Count > 0)
                    {
                        List<string> list = new List<string>();
                        for (int i = 0; i < keyRoot.ChildNodes.Count; i++)
                        {
                            list.Add(keyRoot.ChildNodes[i].Attributes[3].InnerText);
                        }
                        var res = from s in list orderby s group s by s into ss select ss;
                        foreach (IGrouping<string, string> result in res)
                        {
                            KeySelectBox.Items.Add("                         " + result.Key);
                        }
                    }
                    KeySelectBox.Items.Add("               Other Keys"+index);
                    keyRoot = xmlDocument.SelectSingleNode(XPath+"//Other-Keys");
                    if (keyRoot.ChildNodes.Count > 0)
                    {
                        List<string> list = new List<string>();
                        for (int i = 0; i < keyRoot.ChildNodes.Count; i++)
                        {
                            list.Add(keyRoot.ChildNodes[i].Attributes[3].InnerText);
                        }
                        var res = from s in list orderby s group s by s into ss select ss;
                        foreach (IGrouping<string, string> result in res)
                        {
                            KeySelectBox.Items.Add("                         " + result.Key);
                        }
                    }
                }


                public static XmlElement AddF1(XmlDocument xmlDocument,XmlElement xmlElement)
        {
            XmlElement xmlElement2 = xmlDocument.CreateElement("Office-Keys");
            xmlElement.AppendChild(xmlElement2);
            xmlElement2 = xmlDocument.CreateElement("Windows-Keys");
            xmlElement.AppendChild(xmlElement2);
            xmlElement2 = xmlDocument.CreateElement("Server-Keys");
            xmlElement.AppendChild(xmlElement2);
            xmlElement2 = xmlDocument.CreateElement("VS-Keys");
            xmlElement.AppendChild(xmlElement2);
            xmlElement2 = xmlDocument.CreateElement("Other-Keys");
            xmlElement.AppendChild(xmlElement2);

            return xmlElement;
        }
         */

        private void KeySelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectItem = KeySelectBox.SelectedItem as KeySelectModel;
            List<KeySelectModel> list = KeySelectBox.ItemsSource as List<KeySelectModel>;
            List<ProductKeyBaseModel.Key> keylist = new List<ProductKeyBaseModel.Key>();

            if (keyBase==null||selectItem==null||KeySelectBox.SelectedIndex<0)
            {
                return;
            }
            if (selectItem.ItemIndex==0)
            {
                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey="All License " });
                int allProductCount = 0;

                for (int i = 0; i<list.Count; i++)
                {
                    if (list[i].ItemIndex==1)
                    {
                        try
                        {
                            switch (list[i].Name)
                            {
                                case "MAK License":
                                    if (keyBase.MAKLicense != null)
                                    {
                                        keylist.Add(new ProductKeyBaseModel.Key() { ProductKey="          MAK License " });
                                        var makLicense = keyBase.MAKLicense;
                                        int makProductCount = 0;
                                        var allKeysCollections = new Dictionary<string, KeysCollection>
                                        {
                                            ["Office Keys"] = makLicense.OfficeKeys,
                                            ["Windows Keys"] = makLicense.WindowsKeys,
                                            ["Server Keys"] = makLicense.ServerKeys,
                                            ["VS Keys"] = makLicense.VSKeys,
                                            ["Other Keys"] = makLicense.OtherKeys
                                        };

                                        foreach (var keysCollection in allKeysCollections)
                                        {
                                            if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                            {
                                                makProductCount += keysCollection.Value.Keys.Count;
                                                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey=$"                    {keysCollection.Key} [{keysCollection.Value.Keys.Count}]" });
                                                keysCollection.Value.Keys.ForEach(p => p.ProductKey=FileTrans.PCCC(p.ProductKey));
                                                keylist.AddRange(keysCollection.Value.Keys);
                                            }
                                        }

                                        var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("MAK License"));
                                        tlist.ProductKey += $"[{makProductCount}]";

                                        allProductCount += makProductCount;

                                    }
                                    break;
                                case "Retail License":
                                    if (keyBase.RetailLicense != null)
                                    {
                                        keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = "          Retail License " });
                                        var retailLicense = keyBase.RetailLicense;
                                        int retailProductCount = 0;
                                        var allKeysCollections = new Dictionary<string, KeysCollection>
                                        {
                                            ["Office Keys"] = retailLicense.OfficeKeys,
                                            ["Windows Keys"] = retailLicense.WindowsKeys,
                                            ["Server Keys"] = retailLicense.ServerKeys,
                                            ["VS Keys"] = retailLicense.VSKeys,
                                            ["Other Keys"] = retailLicense.OtherKeys
                                        };
                                        foreach (var keysCollection in allKeysCollections)
                                        {
                                            if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                            {
                                                retailProductCount += keysCollection.Value.Keys.Count;
                                                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = $"                    {keysCollection.Key} [{keysCollection.Value.Keys.Count}]" });
                                                keysCollection.Value.Keys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                                keylist.AddRange(keysCollection.Value.Keys);
                                            }
                                        }
                                        var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("Retail License"));
                                        tlist.ProductKey += $"[{retailProductCount}]";

                                        allProductCount += retailProductCount;

                                    }
                                    break;
                                case "KMS License":
                                    if (keyBase.KMSLicense != null)
                                    {
                                        keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = "          KMS License " });
                                        var kmsLicense = keyBase.KMSLicense;
                                        int kmsProductCount = 0;
                                        var allKeysCollections = new Dictionary<string, KeysCollection>
                                        {
                                            ["Office Keys"] = kmsLicense.OfficeKeys,
                                            ["Windows Keys"] = kmsLicense.WindowsKeys,
                                            ["Server Keys"] = kmsLicense.ServerKeys,
                                            ["VS Keys"] = kmsLicense.VSKeys,
                                            ["Other Keys"] = kmsLicense.OtherKeys
                                        };
                                        foreach (var keysCollection in allKeysCollections)
                                        {
                                            if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                            {
                                                kmsProductCount += keysCollection.Value.Keys.Count;
                                                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = $"                    {keysCollection.Key} [{keysCollection.Value.Keys.Count}]" });
                                                keysCollection.Value.Keys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                                keylist.AddRange(keysCollection.Value.Keys);
                                            }
                                        }
                                        var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("KMS License"));
                                        tlist.ProductKey += $"[{kmsProductCount}]";

                                        allProductCount += kmsProductCount;

                                    }
                                    break;
                                case "Other License":
                                    if (keyBase.OtherLicense != null)
                                    {
                                        keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = "          Other License " });
                                        var otherLicense = keyBase.OtherLicense;
                                        int otherProductCount = otherLicense.Keys.Count;
                                        if (otherLicense.Keys != null && otherLicense.Keys.Count > 0)
                                        {
                                            keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = $"                    Other Keys [{otherProductCount}]" });
                                            otherLicense.Keys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                            keylist.AddRange(otherLicense.Keys);
                                        }
                                        var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("Other License"));
                                        tlist.ProductKey += $"[{otherProductCount}]";

                                        allProductCount += otherProductCount;

                                    }
                                    break;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }

                var tlistAll = keylist.FirstOrDefault(p => p.ProductKey.Contains("All License"));
                tlistAll.ProductKey += $"[{allProductCount}]";
            }
            else if (selectItem.ItemIndex==1)
            {
                try
                {
                    switch (selectItem.Name)
                    {
                        case "MAK License":
                            if (keyBase.MAKLicense != null)
                            {
                                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey="          MAK License " });
                                var makLicense = keyBase.MAKLicense;
                                int makProductCount = 0;
                                var allKeysCollections = new Dictionary<string, KeysCollection>
                                {
                                    ["Office Keys"] = makLicense.OfficeKeys,
                                    ["Windows Keys"] = makLicense.WindowsKeys,
                                    ["Server Keys"] = makLicense.ServerKeys,
                                    ["VS Keys"] = makLicense.VSKeys,
                                    ["Other Keys"] = makLicense.OtherKeys
                                };

                                foreach (var keysCollection in allKeysCollections)
                                {
                                    if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                    {
                                        makProductCount += keysCollection.Value.Keys.Count;
                                        keylist.Add(new ProductKeyBaseModel.Key() { ProductKey=$"                    {keysCollection.Key} [{keysCollection.Value.Keys.Count}]" });
                                        keysCollection.Value.Keys.ForEach(p => p.ProductKey=FileTrans.PCCC(p.ProductKey));
                                        keylist.AddRange(keysCollection.Value.Keys);
                                    }
                                }

                                var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("MAK License"));
                                tlist.ProductKey += $"[{makProductCount}]";

                            }
                            break;
                        case "Retail License":
                            if (keyBase.RetailLicense != null)
                            {
                                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = "          Retail License " });
                                var retailLicense = keyBase.RetailLicense;
                                int retailProductCount = 0;
                                var allKeysCollections = new Dictionary<string, KeysCollection>
                                {
                                    ["Office Keys"] = retailLicense.OfficeKeys,
                                    ["Windows Keys"] = retailLicense.WindowsKeys,
                                    ["Server Keys"] = retailLicense.ServerKeys,
                                    ["VS Keys"] = retailLicense.VSKeys,
                                    ["Other Keys"] = retailLicense.OtherKeys
                                };
                                foreach (var keysCollection in allKeysCollections)
                                {
                                    if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                    {
                                        retailProductCount += keysCollection.Value.Keys.Count;
                                        keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = $"                    {keysCollection.Key} [{keysCollection.Value.Keys.Count}]" });
                                        keysCollection.Value.Keys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                        keylist.AddRange(keysCollection.Value.Keys);
                                    }
                                }
                                var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("Retail License"));
                                tlist.ProductKey += $"[{retailProductCount}]";

                            }
                            break;
                        case "KMS License":
                            if (keyBase.KMSLicense != null)
                            {
                                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = "          KMS License " });
                                var kmsLicense = keyBase.KMSLicense;
                                int kmsProductCount = 0;
                                var allKeysCollections = new Dictionary<string, KeysCollection>
                                {
                                    ["Office Keys"] = kmsLicense.OfficeKeys,
                                    ["Windows Keys"] = kmsLicense.WindowsKeys,
                                    ["Server Keys"] = kmsLicense.ServerKeys,
                                    ["VS Keys"] = kmsLicense.VSKeys,
                                    ["Other Keys"] = kmsLicense.OtherKeys
                                };
                                foreach (var keysCollection in allKeysCollections)
                                {
                                    if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                    {
                                        kmsProductCount += keysCollection.Value.Keys.Count;
                                        keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = $"                    {keysCollection.Key} [{keysCollection.Value.Keys.Count}]" });
                                        keysCollection.Value.Keys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                        keylist.AddRange(keysCollection.Value.Keys);
                                    }
                                }
                                var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("KMS License"));
                                tlist.ProductKey += $"[{kmsProductCount}]";

                            }
                            break;
                        case "Other License":
                            if (keyBase.OtherLicense != null)
                            {
                                keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = "          Other License " });
                                var otherLicense = keyBase.OtherLicense;
                                int otherProductCount = otherLicense.Keys.Count;
                                if (otherLicense.Keys != null && otherLicense.Keys.Count > 0)
                                {
                                    keylist.Add(new ProductKeyBaseModel.Key() { ProductKey = $"                    Other Keys [{otherProductCount}]" });
                                    otherLicense.Keys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                    keylist.AddRange(otherLicense.Keys);
                                }
                                var tlist = keylist.FirstOrDefault(p => p.ProductKey.Contains("Other License"));
                                tlist.ProductKey += $"[{otherProductCount}]";

                            }
                            break;
                    }
                }
                catch (Exception)
                {

                }
            }
            else if (selectItem.ItemIndex==2)
            {
                switch (selectItem.LicenseType)
                {
                    case "MAK License":
                        if (keyBase.MAKLicense != null)
                        {
                            var makLicense = keyBase.MAKLicense;
                            var allKeysCollections = new Dictionary<string, KeysCollection>
                            {
                                ["Office Keys"] = makLicense.OfficeKeys,
                                ["Windows Keys"] = makLicense.WindowsKeys,
                                ["Server Keys"] = makLicense.ServerKeys,
                                ["VS Keys"] = makLicense.VSKeys,
                                ["Other Keys"] = makLicense.OtherKeys
                            };
                            foreach (var keysCollection in allKeysCollections)
                            {
                                if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                {
                                    var filteredKeys = keysCollection.Value.Keys.Where(p => p.ProductName == selectItem.Name).ToList();
                                    if (filteredKeys != null && filteredKeys.Count > 0)
                                    {
                                        filteredKeys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                        keylist.AddRange(filteredKeys);

                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case "Retail License":
                        if (keyBase.RetailLicense != null)
                        {
                            var retailLicense = keyBase.RetailLicense;
                            var allKeysCollections = new Dictionary<string, KeysCollection>
                            {
                                ["Office Keys"] = retailLicense.OfficeKeys,
                                ["Windows Keys"] = retailLicense.WindowsKeys,
                                ["Server Keys"] = retailLicense.ServerKeys,
                                ["VS Keys"] = retailLicense.VSKeys,
                                ["Other Keys"] = retailLicense.OtherKeys
                            };
                            foreach (var keysCollection in allKeysCollections)
                            {
                                if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                {
                                    var filteredKeys = keysCollection.Value.Keys.Where(p => p.ProductName == selectItem.Name).ToList();
                                    if (filteredKeys != null && filteredKeys.Count > 0)
                                    {
                                        filteredKeys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                        keylist.AddRange(filteredKeys);

                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case "KMS License":
                        if (keyBase.KMSLicense != null)
                        {
                            var kmsLicense = keyBase.KMSLicense;
                            var allKeysCollections = new Dictionary<string, KeysCollection>
                            {
                                ["Office Keys"] = kmsLicense.OfficeKeys,
                                ["Windows Keys"] = kmsLicense.WindowsKeys,
                                ["Server Keys"] = kmsLicense.ServerKeys,
                                ["VS Keys"] = kmsLicense.VSKeys,
                                ["Other Keys"] = kmsLicense.OtherKeys
                            };
                            foreach (var keysCollection in allKeysCollections)
                            {
                                if (keysCollection.Value.Keys != null && keysCollection.Value.Keys.Count > 0)
                                {
                                    var filteredKeys = keysCollection.Value.Keys.Where(p => p.ProductName == selectItem.Name).ToList();
                                    if (filteredKeys != null && filteredKeys.Count > 0)
                                    {
                                        filteredKeys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                        keylist.AddRange(filteredKeys);

                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case "Other License":
                        if (keyBase.OtherLicense != null)
                        {
                            var otherLicense = keyBase.OtherLicense;
                            if (otherLicense.Keys != null && otherLicense.Keys.Count > 0)
                            {
                                var filteredKeys = otherLicense.Keys.Where(p => p.ProductName == selectItem.Name).ToList();
                                if (filteredKeys != null && filteredKeys.Count > 0)
                                {
                                    filteredKeys.ForEach(p => p.ProductKey = FileTrans.PCCC(p.ProductKey));
                                    keylist.AddRange(filteredKeys);
                                }
                            }
                        }
                        break;
                }
            }

            KeyList.ItemsSource = keylist;
        }
        /*
                 private void KeySelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {

                    XmlNode keyRoot = null;
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(ABC.keyFile + "Keybase.xml");


                    if (KeySelectBox.SelectedItem.ToString() == "All License")
                    {
                        List<FileTrans> list = new List<FileTrans>();
                        list.Add(new FileTrans { A= ">>>>>MAK License" });
                        list.AddRange(ABC.SelectBig(keyRoot, xmlDocument, "//MAK-License"));

                        list.Add(new FileTrans { A = ">>>>>Retail License" });
                        list.AddRange(ABC.SelectBig(keyRoot, xmlDocument, "//Retail-License"));

                        list.Add(new FileTrans { A = ">>>>>KMS License" });
                        list.AddRange(ABC.SelectBig(keyRoot, xmlDocument, "//KMS-License"));

                        KeyList.ItemsSource = list;
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("MAK License"))
                    {
                        KeyList.ItemsSource =  ABC.SelectBig(keyRoot, xmlDocument, "//MAK-License");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Retail License"))
                    {
                        KeyList.ItemsSource =  ABC.SelectBig(keyRoot, xmlDocument, "//Retail-License"); ;
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("KMS License"))
                    {
                        KeyList.ItemsSource =  ABC.SelectBig(keyRoot, xmlDocument, "//KMS-License");
                    }

                    else if (KeySelectBox.SelectedItem.ToString().Contains("Office Keys2"))
                    {
                        lists(xmlDocument, "//Retail-License//Office-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Office Keys3"))
                    {
                        lists(xmlDocument, "//KMS-License//Office-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Office Keys"))
                    {
                        lists(xmlDocument, "//Office-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Windows Keys2"))
                    {
                        lists(xmlDocument, "//Retail-License//Windows-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Windows Keys3"))
                    {
                        lists(xmlDocument, "//KMS-License//Windows-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Windows Keys"))
                    {
                        lists(xmlDocument, "//Windows-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Server Keys2"))
                    {
                        lists(xmlDocument, "//Retail-License//Server-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Server Keys3"))
                    {
                        lists(xmlDocument, "//KMS-License//Server-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Server Keys"))
                    {
                        lists(xmlDocument, "//Server-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("VS Keys2"))
                    {
                        lists(xmlDocument, "//Retail-License//VS-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("VS Keys3"))
                    {
                        lists(xmlDocument, "//KMS-License//VS-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("VS Keys"))
                    {
                        lists(xmlDocument, "//VS-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Other Keys2"))
                    {
                        lists(xmlDocument, "//Retail-License//Other-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Other Keys3"))
                    {
                        lists(xmlDocument, "//KMS-License//Other-Keys");
                    }
                    else if (KeySelectBox.SelectedItem.ToString().Contains("Other Keys"))
                    {
                        lists(xmlDocument, "//Other-Keys");
                    }
                    else
                    {
                        List<FileTrans> list = new List<FileTrans>();

                        var a = ABC.SelectBig(keyRoot, xmlDocument, "//MAK-License");
                        foreach (var aa in a)
                        {
                            if (KeySelectBox.SelectedItem.ToString()== "                         " + aa.A)
                            {
                                list.Add(new FileTrans { B=aa.B, C=aa.C, D=aa.D });
                            }
                        }
                        if (list.Count<1)
                        {
                            var b = ABC.SelectBig(keyRoot, xmlDocument, "//Retail-License");
                            foreach (var aa in b)
                            {
                                if (KeySelectBox.SelectedItem.ToString()== "                         " + aa.A)
                                {
                                    list.Add(new FileTrans { B = aa.B, C = aa.C, D = aa.D });
                                }
                            }
                        }
                        if (list.Count < 1)
                        {
                            var c = ABC.SelectBig(keyRoot, xmlDocument, "//KMS-License");
                            foreach (var aa in c)
                            {
                                if (KeySelectBox.SelectedItem.ToString()== "                         " + aa.A)
                                {
                                    list.Add(new FileTrans { B = aa.B, C = aa.C, D = aa.D });
                                }
                            }
                        }

                        var res = from s in list orderby s.C descending select s;
                        KeyList.ItemsSource = res;
                    }
                }
         */
        /*
                 public void lists(XmlDocument xmlDocument, string xpath)
                {
                    XmlNode xmlNode = xmlDocument.SelectSingleNode(xpath);
                    List<FileTrans> list = new List<FileTrans>();
                    if (xmlNode.ChildNodes.Count<1)
                    {
                        return;
                    }
                    for (int aa = 0; aa < xmlNode.ChildNodes.Count; aa++)
                    {
                        list.Add(new FileTrans
                        {
                            A = xmlNode.ChildNodes[aa].Attributes[3].InnerText,
                            B =  FileTrans.PCCC(xmlNode.ChildNodes[aa].Attributes[2].InnerText),
                            C = xmlNode.ChildNodes[aa].Attributes[1].InnerText,
                            D = xmlNode.ChildNodes[aa].Attributes[0].InnerText
                        }); ;
                    }
                    var res = from s in list orderby s.C descending group s by s.A into ss select ss;
                    var list2 = new List<FileTrans>();
                    foreach (IGrouping<string, FileTrans> fileTrans in res)
                    {
                        list2.Add(new FileTrans { A = fileTrans.Key + ">>>>" });
                        foreach (var result in fileTrans)
                        {
                            list2.Add(new FileTrans { A = "", B = result.B, C = result.C, D = result.D });
                        }
                    }
                    KeyList.ItemsSource = list2;
                }

         */
        public const string KeyFormat = @"(?!^.*N.*N.*$)([BCDFGHJKMPQRTVWXY2-9N]{5}\-){4}[BCDFGHJKMPQRTVWXY2-9N]{4}[BCDFGHJKMPQRTVWXY2-9]";
        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (KeyList.Items.Count<1)
            {
                return;
            }
            if (KeyList.SelectedItem==null)
            {
                return;
            }

            ProductKeyBaseModel.Key selectedKey = KeyList.SelectedItem as ProductKeyBaseModel.Key;
            if (!string.IsNullOrWhiteSpace(selectedKey.ProductKey)&&Regex.IsMatch(selectedKey.ProductKey, KeyFormat))
            {
                Clipboard.SetText(selectedKey.ProductKey);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (KeyList.Items.Count<1)
            {
                return;
            }
            if (KeyList.SelectedItem==null)
            {
                return;
            }

            ProductKeyBaseModel.Key selectedKey = KeyList.SelectedItem as ProductKeyBaseModel.Key;
            if (!string.IsNullOrWhiteSpace(selectedKey.ProductKey)&&Regex.IsMatch(selectedKey.ProductKey, KeyFormat))
            {
                MessageBoxResult result = MessageBox.Show($"确认删除该密钥{selectedKey.ProductKey}？", "提示", MessageBoxButton.YesNo);
                if (result==MessageBoxResult.Yes)
                {
                    try
                    {
                        ProductKeysCollection tProdType = null;

                        switch (selectedKey.LicenseTypeTag)
                        {
                            case "MAK-License":
                                if (keyBase.MAKLicense != null)
                                {
                                    var makLicense = keyBase.MAKLicense;
                                    switch (selectedKey.ProductTypeTag)
                                    {
                                        case "Office-Keys":
                                            tProdType= makLicense.OfficeKeys;
                                            break;
                                        case "Windows-Keys":
                                            tProdType= makLicense.WindowsKeys;
                                            break;
                                        case "Server-Keys":
                                            tProdType= makLicense.ServerKeys;
                                            break;
                                        case "VS-Keys":
                                            tProdType= makLicense.VSKeys;
                                            break;
                                        case "Other-Keys":
                                            tProdType= makLicense.OtherKeys;
                                            break;
                                    }

                                }
                                break;
                            case "Retail-License":
                                if (keyBase.RetailLicense != null)
                                {
                                    var retailLicense = keyBase.RetailLicense;
                                    switch (selectedKey.ProductTypeTag)
                                    {
                                        case "Office-Keys":
                                            tProdType= retailLicense.OfficeKeys;
                                            break;
                                        case "Windows-Keys":
                                            tProdType= retailLicense.WindowsKeys;
                                            break;
                                        case "Server-Keys":
                                            tProdType= retailLicense.ServerKeys;
                                            break;
                                        case "VS-Keys":
                                            tProdType= retailLicense.VSKeys;
                                            break;
                                        case "Other-Keys":
                                            tProdType= retailLicense.OtherKeys;
                                            break;
                                    }

                                }
                                break;
                            case "KMS-License":
                                if (keyBase.KMSLicense != null)
                                {
                                    var kmsLicense = keyBase.KMSLicense;
                                    switch (selectedKey.ProductTypeTag)
                                    {
                                        case "Office-Keys":
                                            tProdType= kmsLicense.OfficeKeys;
                                            break;
                                        case "Windows-Keys":
                                            tProdType= kmsLicense.WindowsKeys;
                                            break;
                                        case "Server-Keys":
                                            tProdType= kmsLicense.ServerKeys;
                                            break;
                                        case "VS-Keys":
                                            tProdType= kmsLicense.VSKeys;
                                            break;
                                        case "Other-Keys":
                                            tProdType= kmsLicense.OtherKeys;
                                            break;
                                    }

                                }
                                break;
                            case "Other-License":
                                if (keyBase.OtherLicense != null)
                                {
                                    tProdType = keyBase.OtherLicense;
                                }
                                break;
                        }

                        if (tProdType!=null)
                        {
                            ProductKeyBaseModel.Key foundKey = tProdType.Keys.FirstOrDefault(p => p.ProductKey == selectedKey.ProductKey);
                            if (foundKey!=null)
                            {
                                tProdType.Keys.Remove(foundKey);
                                GlobalParameters.SaveKeyBaseFile(keyBase);

                                //判断当前版本密钥是否还有，没有则重新加载版本选择框并清空列表，有则刷新列表并更新版本选择框
                                var selectVersion = tProdType.Keys.Where(p => p.ProductName == selectedKey.ProductName);
                                var tselectBox = KeySelectBox.ItemsSource as List<KeySelectModel>;
                                if (selectVersion.Count()>0)
                                {
                                    var updItem = tselectBox.FirstOrDefault(p => p.Name == selectedKey.ProductName);
                                    if (updItem!=null)
                                    {
                                        updItem.ShowName = $"                    [{selectVersion.Count()}]     {selectedKey.ProductName}";

                                        var tselectIndex = KeySelectBox.SelectedIndex;
                                        KeySelectBox.ItemsSource = null;
                                        KeySelectBox.ItemsSource = tselectBox;
                                        KeySelectBox.SelectedIndex = tselectIndex;
                                    }

                                }
                                else
                                {
                                    var updItem = tselectBox.FirstOrDefault(p => p.Name == selectedKey.ProductName);
                                    if (updItem!=null)
                                    {
                                        tselectBox.Remove(updItem);
                                        KeySelectBox.ItemsSource = null;
                                        KeySelectBox.ItemsSource = tselectBox;
                                        KeySelectBox.SelectedIndex = 0;
                                    }

                                }

                                MessageBox.Show("删除密钥成功！");

                            }
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("删除密钥发生异常，请刷新列表稍后再试！");
                    }

                }

            }

        }
        private async void CheckBtn_Click(object sender, RoutedEventArgs e)
        {
            if (KeyList.Items.Count<1)
            {
                return;
            }
            if (KeyList.SelectedItem==null)
            {
                return;
            }

            ProductKeyBaseModel.Key selectedKey = KeyList.SelectedItem as ProductKeyBaseModel.Key;
            if (!string.IsNullOrWhiteSpace(selectedKey.ProductKey)&&Regex.IsMatch(selectedKey.ProductKey, KeyFormat))
            {
                try
                {
                    ProductKeysCollection tProdType = null;

                    switch (selectedKey.LicenseTypeTag)
                    {
                        case "MAK-License":
                            if (keyBase.MAKLicense != null)
                            {
                                var makLicense = keyBase.MAKLicense;
                                switch (selectedKey.ProductTypeTag)
                                {
                                    case "Office-Keys":
                                        tProdType= makLicense.OfficeKeys;
                                        break;
                                    case "Windows-Keys":
                                        tProdType= makLicense.WindowsKeys;
                                        break;
                                    case "Server-Keys":
                                        tProdType= makLicense.ServerKeys;
                                        break;
                                    case "VS-Keys":
                                        tProdType= makLicense.VSKeys;
                                        break;
                                    case "Other-Keys":
                                        tProdType= makLicense.OtherKeys;
                                        break;
                                }

                            }
                            break;
                        case "Retail-License":
                            if (keyBase.RetailLicense != null)
                            {
                                var retailLicense = keyBase.RetailLicense;
                                switch (selectedKey.ProductTypeTag)
                                {
                                    case "Office-Keys":
                                        tProdType= retailLicense.OfficeKeys;
                                        break;
                                    case "Windows-Keys":
                                        tProdType= retailLicense.WindowsKeys;
                                        break;
                                    case "Server-Keys":
                                        tProdType= retailLicense.ServerKeys;
                                        break;
                                    case "VS-Keys":
                                        tProdType= retailLicense.VSKeys;
                                        break;
                                    case "Other-Keys":
                                        tProdType= retailLicense.OtherKeys;
                                        break;
                                }

                            }
                            break;
                        case "KMS-License":
                            if (keyBase.KMSLicense != null)
                            {
                                var kmsLicense = keyBase.KMSLicense;
                                switch (selectedKey.ProductTypeTag)
                                {
                                    case "Office-Keys":
                                        tProdType= kmsLicense.OfficeKeys;
                                        break;
                                    case "Windows-Keys":
                                        tProdType= kmsLicense.WindowsKeys;
                                        break;
                                    case "Server-Keys":
                                        tProdType= kmsLicense.ServerKeys;
                                        break;
                                    case "VS-Keys":
                                        tProdType= kmsLicense.VSKeys;
                                        break;
                                    case "Other-Keys":
                                        tProdType= kmsLicense.OtherKeys;
                                        break;
                                }

                            }
                            break;
                        case "Other-License":
                            if (keyBase.OtherLicense != null)
                            {
                                tProdType = keyBase.OtherLicense;
                            }
                            break;
                    }

                    if (tProdType!=null)
                    {
                        ProductKeyBaseModel.Key foundKey = tProdType.Keys.FirstOrDefault(p => p.ProductKey == selectedKey.ProductKey);
                        if (foundKey!=null)
                        {
                            string checkResult = "";
                            if (!string.IsNullOrWhiteSpace(foundKey.ConfigID))
                            {
                                ErrorCodeCheckResultModel tresult = await GetErrorCodeByPost.ConsumeKeyWithID(foundKey.ProductKey, foundKey.ConfigID);
                                checkResult=tresult.CheckResult;
                            }
                            else if (!string.IsNullOrWhiteSpace(foundKey.EPID))
                            {
                                checkResult=KeyCheckFunModel.GetCount(foundKey.EPID);
                            }
                            else
                            {
                                MainWindow.mainWindow.Test.Text=foundKey.ProductKey;
                                this.Close();
                                return;
                            }

                            if (checkResult.Contains("Blocked")||checkResult.Contains("0xC004C003")||checkResult.Contains("0xC004C060"))
                            {
                                tProdType.Keys.Remove(foundKey);
                                MessageBox.Show("当前密钥 " + foundKey.ProductKey + " 检测为被封密钥，已从本地密钥库中移除！", "提示");
                            }
                            else
                            {
                                foundKey.ActiveCount=checkResult;
                            }

                            GlobalParameters.SaveKeyBaseFile(keyBase);

                            //判断当前版本密钥是否还有，没有则重新加载版本选择框并清空列表，有则刷新列表并更新版本选择框
                            var selectVersion = tProdType.Keys.Where(p => p.ProductName == selectedKey.ProductName);
                            var tselectBox = KeySelectBox.ItemsSource as List<KeySelectModel>;
                            if (selectVersion.Count()>0)
                            {
                                var updItem = tselectBox.FirstOrDefault(p => p.Name == selectedKey.ProductName);
                                if (updItem!=null)
                                {
                                    updItem.ShowName = $"                    [{selectVersion.Count()}]     {selectedKey.ProductName}";

                                    var tselectIndex = KeySelectBox.SelectedIndex;
                                    KeySelectBox.ItemsSource = null;
                                    KeySelectBox.ItemsSource = tselectBox;
                                    KeySelectBox.SelectedIndex = tselectIndex;
                                }

                            }
                            else
                            {
                                var updItem = tselectBox.FirstOrDefault(p => p.Name == selectedKey.ProductName);
                                if (updItem!=null)
                                {
                                    tselectBox.Remove(updItem);
                                    KeySelectBox.ItemsSource = null;
                                    KeySelectBox.ItemsSource = tselectBox;
                                    KeySelectBox.SelectedIndex = 0;
                                }

                            }


                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("重新检测时发生异常，请刷新列表稍后再试！");
                }

            }

        }

        /*
                 public void SetComplete(string count, FileTrans result, MenuItem abcd)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(ABC.keyFile + "Keybase.xml");
            XmlNode xmlNode = null;

            if (count == "Key is Blocked" || count.Contains("060") || count.Contains("003"))
            {
                try
                {
                    xmlNode = xmlDocument.SelectSingleNode("//Keys[@ProductKey='" + FileTrans.PAAA(result.B) +"']");

                    xmlNode.RemoveAll();
                    xmlDocument.Save(ABC.keyFile + "Keybase.xml");
                    string data = File.ReadAllText(ABC.keyFile + "Keybase.xml");
                    data = data.Replace("<Keys />", "");
                    File.WriteAllText(ABC.keyFile + "Keybase.xml", data);
                }
                catch (Exception)
                {

                }
            }
            else
            {
                xmlNode = xmlDocument.SelectSingleNode("//Keys[@EPID='" +result.D +"']");
                xmlNode.Attributes[1].InnerText = count;
                xmlDocument.Save(ABC.keyFile + "Keybase.xml");


            }


            int index = KeySelectBox.SelectedIndex;
            if (index == -1)
            {
                string abc = SearchTb.Text;
                SearchTb.Text = "";
                SearchTb.Text = abc;
                return;
            }
            KeySelectBox.SelectedIndex = 0;
            KeySelectBox.SelectedIndex = index;
        }
                 private async void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
                {
                    await Task.Delay(1000);

                    if (string.IsNullOrWhiteSpace(SearchTb.Text))
                    {
                        KeyList.ItemsSource = null;
                        SearchResult.Text = "";
                        return;
                    }
                    var xmlDocument = new XmlDocument();
                    xmlDocument.Load(ABC.keyFile + "Keybase.xml");
                    XmlNode keyRoot = null;
                    var a = ABC.SelectBig(keyRoot, xmlDocument, "//MAK-License");
                    var b = ABC.SelectBig(keyRoot, xmlDocument, "//Retail-License");
                    var c = ABC.SelectBig(keyRoot, xmlDocument, "//KMS-License");
                    a.AddRange(b);
                    a.AddRange(c);

                    List<FileTrans> list = new List<FileTrans>();
                    foreach (var aa in a)
                    {
                        if (aa.B.Contains(SearchTb.Text))
                        {
                            list.Add(new FileTrans { A = aa.A, B = aa.B, C = aa.C, D = aa.D });
                        }
                    }
                    if (list.Count>0)
                    {
                        SearchResult.Text = "已找到"+list.Count+"项结果.";

                        var res = from s in list orderby s.C descending group s by s.A into ss select ss;
                        var list2 = new List<FileTrans>();
                        foreach (IGrouping<string, FileTrans> fileTrans in res)
                        {
                            list2.Add(new FileTrans { A = fileTrans.Key+">>>>" });
                            foreach (var result in fileTrans)
                            {
                                list2.Add(new FileTrans { A = "", B = result.B, C = result.C, D = result.D });
                            }
                        }
                        KeyList.ItemsSource = list2;
                    }
                    else
                    {
                        KeyList.ItemsSource = null;
                        SearchResult.Text = "没有匹配的结果.";
                    }
                }
         */

        private void KeyListMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (KeyList.SelectedItem==null)
            {
                KeyListMenu.IsOpen=false;
                return;
            }

            ProductKeyBaseModel.Key selectedKey = KeyList.SelectedItem as ProductKeyBaseModel.Key;
            if (string.IsNullOrWhiteSpace(selectedKey.ProductKey)|| !Regex.IsMatch(selectedKey.ProductKey, KeyFormat))
            {
                KeyListMenu.IsOpen=false;
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTb.Text))
            {
                return;
            }

            KeySelectBox.SelectedIndex = 0;
            List<ProductKeyBaseModel.Key> keylist = KeyList.ItemsSource as List<ProductKeyBaseModel.Key>;
            var items = keylist.Where(p => p.ProductKey.Contains(SearchTb.Text));
            KeySelectBox.SelectedIndex = -1;

            if (items!=null&&items.Count()>0)
            {
                KeyList.ItemsSource = items.ToList();
                SearchResult.Text = "已找到" + items.Count() + "项结果.";
            }
            else
            {
                KeyList.ItemsSource = null;
                SearchResult.Text = "没有匹配的结果.";
            }
        }
    }
}
