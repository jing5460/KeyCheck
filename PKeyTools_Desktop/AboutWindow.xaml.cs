using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
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

namespace PKeyTools
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void CodeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://github.com/FHWWC/KeyCheck",
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {

            }
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://pkeytool.com/",
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
