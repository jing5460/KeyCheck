using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Xml.Linq;
using System.Xml.Serialization;
using static PKeyTools.PKeyConfigModel;

namespace PKeyTools
{
    /// <summary>
    /// ConfigMP.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigMP : Window
    {
        public static ConfigMP configMP;
        public ConfigMP()
        {
            InitializeComponent();
            configMP = this;
        }
        public PkeyData pkconfigModel;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadList();   
        }
        private void PKList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(PKList.SelectedIndex<0)
            {
                HiddenBtn();
                return;
            }
            RemoveCon.Visibility = 0;
            EditCon.Visibility = 0;
            UpCon.Visibility = 0;
            DownCon.Visibility = 0;
            //ConfigType aaa = e.AddedItems[0] as ConfigType;
        }
        public void HiddenBtn()
        {
            RemoveCon.Visibility = (Visibility)2;
            EditCon.Visibility = (Visibility)2;
            UpCon.Visibility = (Visibility)2;
            DownCon.Visibility = (Visibility)2;
        }
        public void LoadList()
        {
            pkconfigModel =GlobalParameters.GetPkeyData();

            if (pkconfigModel!=null)
            {
                PKList.ItemsSource = pkconfigModel.ConfigTypes;
            }
        }
        public void SaveConfigList()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new XmlSerializer(typeof(PkeyData)).Serialize(ms, pkconfigModel);
                File.WriteAllText(GlobalParameters.GetConfigFilePath(), Encoding.ASCII.GetString(ms.ToArray()));
            }
        }
        private void AddCon_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            addWindow.ShowDialog();
        }

        private void RemoveCon_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result= MessageBox.Show("确认删除？此操作不会删除本地的证书文件","提示",MessageBoxButton.YesNo);

            if (result==MessageBoxResult.Yes)
            {
                pkconfigModel = GlobalParameters.GetPkeyData();

                if (pkconfigModel!=null)
                {
                    List<ConfigType> conList = pkconfigModel.ConfigTypes;
                    var removeItem = conList.FirstOrDefault(p => p.ConfigID==(PKList.SelectedItem as ConfigType).ConfigID);
                    if(removeItem!=null)
                    {
                        conList.Remove(removeItem);
                        SaveConfigList();
                        LoadList();
                    }
                }

            }
        }

        private void EditCon_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            addWindow.Title = "EditWindow";
            addWindow.T1.Text = "编辑证书页面";

            pkconfigModel = GlobalParameters.GetPkeyData();
            if (pkconfigModel!=null)
            {
                List<ConfigType> conList = pkconfigModel.ConfigTypes;
                var editItem = conList.FirstOrDefault(p => p.ConfigID==(PKList.SelectedItem as ConfigType).ConfigID);
                if (editItem!=null)
                {
                    addWindow.ConfigName.Text = editItem.ConfigName;
                    addWindow.FileAddress.Text = "[不支持修改] "+editItem.ConfigPath;
                    addWindow.FileSelect.IsEnabled=false;
                    addWindow.cid= editItem.ConfigID;
                    addWindow.ShowDialog();
                }
            }

        }

        private void UpCon_Click(object sender, RoutedEventArgs e)
        {
            int a = PKList.SelectedIndex;
            if(a-1<0)
            {
                return;
            }

            pkconfigModel = GlobalParameters.GetPkeyData();
            if (pkconfigModel!=null)
            {
                List<ConfigType> conList = pkconfigModel.ConfigTypes;
                var tItem = conList.FirstOrDefault(p => p.ConfigID==(PKList.SelectedItem as ConfigType).ConfigID);
                if (tItem!=null)
                {
                    int tindex= conList.IndexOf(tItem);
                    (conList[tindex-1], conList[tindex])= (conList[tindex], conList[tindex-1]);

                    SaveConfigList();
                    LoadList();
                }
            }
        }

        private void DownCon_Click(object sender, RoutedEventArgs e)
        {
            int a = PKList.SelectedIndex;
            if (a+1>=PKList.Items.Count)
            {
                return;
            }

            pkconfigModel = GlobalParameters.GetPkeyData();
            if (pkconfigModel!=null)
            {
                List<ConfigType> conList = pkconfigModel.ConfigTypes;
                var tItem = conList.FirstOrDefault(p => p.ConfigID==(PKList.SelectedItem as ConfigType).ConfigID);
                if (tItem!=null)
                {
                    int tindex = conList.IndexOf(tItem);
                    (conList[tindex], conList[tindex+1])= (conList[tindex+1], conList[tindex]);

                    SaveConfigList();
                    LoadList();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }
    }
}
