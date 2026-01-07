using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
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
using System.Xml;
using static PKeyTools.PKeyConfigModel;

namespace PKeyTools
{
    /// <summary>
    /// AddWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
        }
        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XrML 数字许可证|*.xrm-ms";

            if (openFileDialog.ShowDialog() == true)
            {
                FileAddress.Text = openFileDialog.FileName;
                name = openFileDialog.SafeFileName;
            }
        }
        public string addr;
        public string name;
        public int cid;
        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(ConfigName.Text)|| string.IsNullOrEmpty(FileAddress.Text))
            {
                return;
            }
            
            if(this.Title=="AddWindow")
            {
                try
                {
                    var targetPath = Path.Combine(AppContext.BaseDirectory.Replace("bin\\Debug\\net8.0-windows", "").Replace("bin\\Release\\net8.0-windows", ""), "PKeyConfig\\Other", name);
                    if (File.Exists(targetPath))
                    {
                        MessageBox.Show("您选择的证书在当前目录下已存在,请手动删除已存在的证书.\n(本程序暂时不支持替换已存在的旧证书)");
                        return;
                    }

                    File.Copy(FileAddress.Text, targetPath);
                }
                catch (Exception)
                {
                    MessageBox.Show("复制文件发生异常，请重试。");
                    return;
                }
                addr = "Other\\" + name;


                ConfigMP.configMP.pkconfigModel = GlobalParameters.GetPkeyData();
                if (ConfigMP.configMP.pkconfigModel!=null)
                {
                    List<ConfigType> conList = ConfigMP.configMP.pkconfigModel.ConfigTypes;
                    ConfigType newConfig = new ConfigType()
                    {
                        ConfigID = ConfigMP.configMP.pkconfigModel.ConfigTypes.Count > 0 ? ConfigMP.configMP.pkconfigModel.ConfigTypes.Max(p => p.ConfigID) + 1 : 1,
                        ConfigName = ConfigName.Text,
                        ConfigPath = addr
                    };
                    conList.Add(newConfig);

                }
            }
            else
            {
                ConfigMP.configMP.pkconfigModel = GlobalParameters.GetPkeyData();
                if (ConfigMP.configMP.pkconfigModel!=null)
                {
                    List<ConfigType> conList = ConfigMP.configMP.pkconfigModel.ConfigTypes;
                    var editItem = conList.FirstOrDefault(p => p.ConfigID==cid);
                    if (editItem!=null)
                    {
                        editItem.ConfigName = ConfigName.Text;
                        //editItem.ConfigPath = FileAddress.Text;
                    }
                }
            }

            ConfigMP.configMP.SaveConfigList();
            ConfigMP.configMP.LoadList();
            this.Close();
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
