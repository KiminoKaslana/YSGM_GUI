using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Windows.Shapes;

namespace YSGM_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CommandMap.RegisterAll();

            LoadConfiguration();
        }

        private void ChildThreadStart(object? cmd)
        {
            if (cmd == null)
            {
                return;
            }
            string res = Execute.ExecuteCommand((string)cmd);
            App.Current.Dispatcher.Invoke(new Action(() => {
                callback.Content = res;
                ExecuteButton.IsEnabled = true;
                ExecuteButton.Content = "执行";
            }));
            return;
        }

        private void LoadConfiguration()
        {
            string[] config;
            if (File.Exists("configuration"))
            {
                config = File.ReadAllLines("configuration");
            }
            else
            {
                config = new string[5];
                config[0] = "127.0.0.1";//ssh host
                config[1] = "root";//ssh user
                config[2] = "http://127.0.0.1:20011/api";//muip host
                config[3] = "dev_gio";//target region
                config[4] = "0";//uid
                File.WriteAllLines("configuration", config);
            }

            hostIP.Text = config[0];
            sshUser.Text = config[1];
            port.Text = config[2].Split(":")[2].Split("/")[0];
            targetRegion.Text = config[3];
            defaultUID.Text = config[4];

        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            //callback.Content = ExecuteCommand(commandBox.Text);
            Thread thread = new Thread(ChildThreadStart);
            thread.Start(commandBox.Text);

            ExecuteButton.IsEnabled = false;
            ExecuteButton.Content = "等待结果";
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
