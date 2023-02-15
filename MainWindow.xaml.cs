﻿using System;
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
        string additionalParameters = "";
        TextBox currentFocusedBox;//当前正在操作的textbox
        List<ListBoxItem> currentList;//当前正在操作的list

        public MainWindow()
        {
            InitializeComponent();
            CommandMap.RegisterAll();

            LoadConfiguration();
        }

        #region 应用级事件

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
            string[] config = new string[5];
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

        private void ReadFileAndCreateList(string fileName, ref ListBox listBox)
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

            currentList = listBoxItems;

            listBox.ItemsSource = listBoxItems;
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

        private void UpdateCMD()
        {
            commandBox.Text = "gm " + uidBox.Text + " " + mainCMD + " " + parameter + " " + additionalParameters;
        }

        private void SearchInList(string key, ref ListBox listBox)
        {
            Regex regex = new Regex(@".*" + key + @".*");

            if (listBox != null)
            {
                if (currentList != null)
                {
                    Trace.WriteLine("search");

                    List<ListBoxItem> list = new List<ListBoxItem>();

                    for (int i = 0; i < currentList.Count; i++)
                    {
                        if (regex.IsMatch((string)currentList[i].Content))
                        {
                            list.Add(currentList[i]);
                        }
                    }

                    listBox.ItemsSource = list;
                }
            }
        }

        private void Selected(object sender, SelectionChangedEventArgs e)//所有listbox共用此事件
        {
            Regex regex = new Regex(@"\d+");
            ListBoxItem list = (ListBoxItem)((ListBox)sender).SelectedItem;
            if (list == null)
            {
                return;
            }

            currentFocusedBox.Text = (string)list.Content;
            parameter = regex.Match((string)list.Content).Value;

            UpdateCMD();
        }

        #endregion

        #region 位置页事件


        private void position_sceneID_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "jump";
            additionalParameters = "";

            //UpdateCMD();

            TextBox textBox = (TextBox)sender;
            currentFocusedBox = textBox;
            //textBox.SelectAll();

            if (textBox.Text == "点击后在此搜索或在右侧选择")
            {
                textBox.Text = "";
            }

            ReadFileAndCreateList("Scene.txt", ref positionList);
        }

        private void position_TPPoint_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "point";
            additionalParameters = "1";

            //UpdateCMD();

            TextBox textBox = (TextBox)sender;
            currentFocusedBox = textBox;
            //textBox.SelectAll();

            if (textBox.Text == "点击后在此搜索或在右侧选择")
            {
                textBox.Text = "";
            }


            ReadFileAndCreateList("Scene.txt", ref positionList);
        }

        private void positionGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (position_sceneID.Text == "")
            {
                position_sceneID.Text = "点击后在此搜索或在右侧选择";
            }

            if (position_TPPoint.Text == "")
            {
                position_TPPoint.Text = "点击后在此搜索或在右侧选择";
            }

            commandBox.Focus();
        }

        private void position_IsOpenAllTPPoint_Click(object sender, RoutedEventArgs e)
        {
            if (position_IsOpenAllTPPoint.IsChecked != null)
            {
                if ((bool)position_IsOpenAllTPPoint.IsChecked)
                {
                    additionalParameters = "all";
                }
                else
                {
                    additionalParameters = position_TPPointNum.Text;
                }

                UpdateCMD();
            }
        }

        private void position_sceneID_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "jump";
            UpdateCMD();

            SearchInList(position_sceneID.Text, ref positionList);

        }

        private void position_TPPoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "point";
            UpdateCMD();

            SearchInList(position_TPPoint.Text, ref positionList);
        }

        private void position_TPPointNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "point";
            additionalParameters = position_TPPointNum.Text;
            UpdateCMD();
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
