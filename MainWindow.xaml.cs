using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
using System.Text.RegularExpressions;

namespace YSGM_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string mainCMD = "";
        string parameter = "";

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

            string res;

            if (((string)cmd).ToLower() == "getplayersnum")
            {
                res = MUIPManager.Instance.FetchPlayerNum();
                Trace.WriteLine(res);

                Regex regex = new Regex("\"online_player_num_except_sub_account\":\\d+");

                if (regex.Match(res).Success)
                {
                    res = regex.Match(res).Value.Replace("\"online_player_num_except_sub_account\"", "在线玩家总数");
                }
            }
            else
            {
                res = Execute.ExecuteCommand((string)cmd);

                Trace.WriteLine(res);

                Regex regex = new Regex("\"msg\":\".+\"");

                if (regex.Match(res).Success)
                {
                    res = regex.Match(res).Value.Replace("\"msg\"", "消息");
                }
            }

            App.Current.Dispatcher.Invoke(new Action(() =>
            {

                callback.Content = res;
                ExecuteButton.IsEnabled = true;
                ExecuteButton.Content = "执行";

                TestButton.IsEnabled = true;
            }));
            return;
        }

        private void LoadConfiguration()
        {
            string[] config = new string[5]; ;
            if (File.Exists("App.config"))
            {
                config[0] = ConfigurationManager.AppSettings.Get("SSH_HOST");
                config[1] = ConfigurationManager.AppSettings.Get("SSH_USER");
                config[2] = ConfigurationManager.AppSettings.Get("MUIP_HOST");
                config[3] = ConfigurationManager.AppSettings.Get("MUIP_TARGET_REGION");
                config[4] = ConfigurationManager.AppSettings.Get("UID");
            }
            else
            {
                config[0] = "127.0.0.1";//ssh host
                config[1] = "root";//ssh user
                config[2] = "http://127.0.0.1:20011/api";//muip host
                config[3] = "dev_gio";//target region
                config[4] = "0";//uid
                File.WriteAllText("App.config", "<configuration><appSettings><addkey=\"SSH_HOST\"value=\"127.0.0.1\"/><addkey=\"SSH_USER\"value=\"root\"/><addkey=\"MUIP_HOST\"value=\"http://127.0.0.1:20011/api\"/><addkey=\"MUIP_TARGET_REGION\"value=\"dev_gio\"/><addkey=\"UID\"value=\"0\"/></appSettings></configuration>");
            }

            hostIP.Text = config[0];
            sshUser.Text = config[1];
            port.Text = config[2].Split(":")[2].Split("/")[0];
            targetRegion.Text = config[3];
            defaultUID.Text = config[4];
            uidBox.Text = config[4];
        }

        private void ReadFileAndCreateList(string fileName)
        {
            List<ListBoxItem> listBoxItems = new List<ListBoxItem>();

            string[] sceneFile;

            if (!File.Exists("./Resources/zh-cn/" + fileName))
            {
                File.Create("./Resources/zh-cn/" + fileName);
            }

            sceneFile = File.ReadAllLines("./Resources/zh-cn/" + fileName);

            ListBoxItem[] boxItems = new ListBoxItem[sceneFile.Length];

            for (int i = 0; i < sceneFile.Length; i++)
            {
                boxItems[i] = new ListBoxItem();
                boxItems[i].Content = sceneFile[i];
                listBoxItems.Add(boxItems[i]);
            }

            positionList.ItemsSource = listBoxItems;
        }

        #region 窗口级事件
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
            Thread thread = new Thread(ChildThreadStart);
            thread.Start("getplayersnum");

            ExecuteButton.IsEnabled = false;
            ExecuteButton.Content = "等待结果";

            TestButton.IsEnabled = false;
        }

        private void callback_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(callback.Content.ToString());
        }
        #endregion

        #region 位置页事件
        private void position_sceneID_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "jump";

            TextBox textBox = (TextBox)sender;
            textBox.Text = "";

            ReadFileAndCreateList("Scene.txt");
        }

        private void position_TPPoint_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "point";

            TextBox textBox = (TextBox)sender;
            textBox.Text = "";

            ReadFileAndCreateList("Scene.txt");
        }

        private void positionGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (position_sceneID.Text == "")
            {
                position_sceneID.Text = "点击后在此搜索或在右侧选择";
            }

            if (position_TPPoint.Text == "")
            {
                position_sceneID.Text = "点击后在此搜索或在右侧选择";
            }

            positionGrid.Focus();
        }

        private void position_IsOpenAllTPPoint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void positionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Trace.WriteLine("change");

            UpdateCMDByList();
        }

        private void UpdateCMDByList()
        {
            Regex regex = new Regex(@"\d+");

            ListBoxItem list = (ListBoxItem)positionList.SelectedItem;
            commandBox.Text = "gm " + uidBox.Text + " " + mainCMD + " " + regex.Match((string)list.Content).Value + parameter;
        }

        #endregion

        #region GM设置页事件
        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            string[] config = new string[5];
            config[0] = hostIP.Text;
            config[1] = sshUser.Text;
            config[2] = "http://" + hostIP.Text + ":" + port.Text + "/api";
            config[3] = targetRegion.Text;
            config[4] = defaultUID.Text;

            /*ConfigurationManager.AppSettings.Set("SSH_HOST", config[0]);
            ConfigurationManager.AppSettings.Set("SSH_USER", config[1]);
            ConfigurationManager.AppSettings.Set("MUIP_HOST", config[2]);
            ConfigurationManager.AppSettings.Set("MUIP_TARGET_REGION", config[3]);
            ConfigurationManager.AppSettings.Set("UID", config[4]);*/

            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            settings["SSH_HOST"].Value = config[0];
            settings["SSH_USER"].Value = config[1];
            settings["MUIP_HOST"].Value = config[2];
            settings["MUIP_TARGET_REGION"].Value = config[3];
            settings["UID"].Value = config[4];

            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

            Trace.WriteLine(ConfigurationManager.AppSettings.Get("SSH_HOST"));
        }



        #endregion


    }
}
