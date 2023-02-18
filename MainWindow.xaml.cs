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
using System.Reflection.Metadata;

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

            if (!Directory.Exists("./Resources/zh-cn"))
            {
                Directory.CreateDirectory("./Resources/zh-cn");
            }

            if (!File.Exists("./Resources/zh-cn/" + fileName))
            {
                File.WriteAllText("./Resources/zh-cn/" + fileName, "无数据，请检查资源有效性");
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

        private void RunCMDInChildThread()
        {
            Thread thread = new Thread(ChildThreadStart);
            thread.Start(commandBox.Text);

            ExecuteButton.IsEnabled = false;
            ExecuteButton.Content = "等待结果";
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            //callback.Content = ExecuteCommand(commandBox.Text);
            RunCMDInChildThread();
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
            commandBox.Text = ("gm " + uidBox.Text + " " + mainCMD + " " + parameter + " " + additionalParameters).Trim().Replace("  ", " ");
        }

        private void SearchInList(string key, ref ListBox listBox, string artifactRank = "0")
        {
            Regex regex;

            if (artifactRank != "0")
            {
                regex = new Regex(@"^\d\d" + artifactRank + ".*" + key + @".*");
            }
            else
            {
                regex = new Regex(@".*" + key + @".*");
            }


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

        //位置

        private void position_X_GotFocus(object sender, RoutedEventArgs e)
        {
            if (position_X.Text == "X")
            {
                position_X.Text = "";
            }
        }

        private void position_Y_GotFocus(object sender, RoutedEventArgs e)
        {
            if (position_Y.Text == "Y")
            {
                position_Y.Text = "";
            }
        }

        private void position_Z_GotFocus(object sender, RoutedEventArgs e)
        {
            if (position_Z.Text == "Z")
            {
                position_Z.Text = "";
            }
        }

        private void position_X_TextChanged(object sender, TextChangedEventArgs e)
        {
            //此处if的意义是，在调试的过程中，
            //发现通过xaml设置初始值时textchanged会执行一次，这时控件可能未完成初始化而出现空引用。
            if (position_X == null || position_Y == null || position_Z == null)
            {
                return;
            }

            //Trace.WriteLine("X change");
            mainCMD = "goto";
            parameter = position_X.Text + " " + position_Y.Text + " " + position_Z.Text;
            additionalParameters = "";
            UpdateCMD();
        }

        private void position_Y_TextChanged(object sender, TextChangedEventArgs e)
        {
            //此处if的意义是，在调试的过程中，
            //发现通过xaml设置初始值时textchanged会执行一次，这时控件可能未完成初始化而出现空引用。
            if (position_X == null || position_Y == null || position_Z == null)
            {
                return;
            }

            mainCMD = "goto";
            parameter = position_X.Text + " " + position_Y.Text + " " + position_Z.Text;
            additionalParameters = "";
            UpdateCMD();
        }

        private void position_Z_TextChanged(object sender, TextChangedEventArgs e)
        {
            //此处if的意义是，在调试的过程中，
            //发现通过xaml设置初始值时textchanged会执行一次，这时控件可能未完成初始化而出现空引用。
            if (position_X == null || position_Y == null || position_Z == null)
            {
                return;
            }

            mainCMD = "goto";
            parameter = position_X.Text + " " + position_Y.Text + " " + position_Z.Text;
            additionalParameters = "";
            UpdateCMD();
        }

        private void position_sceneID_GotFocus(object sender, RoutedEventArgs e) //场景ID
        {
            mainCMD = "jump";
            additionalParameters = "";

            //UpdateCMD();

            TextBox textBox = (TextBox)sender;
            //TextBox textbox = new TextBox();
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
        }//开传送点

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

            if (position_X.Text == "")
            {
                position_X.Text = "X";
            }

            if (position_Y.Text == "")
            {
                position_Y.Text = "Y";
            }

            if (position_Z.Text == "")
            {
                position_Z.Text = "Z";
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

        #region 角色页事件
        //角色
        private void role_adventure_level_GotFocus(object sender, RoutedEventArgs e) //冒险等级
        {
            mainCMD = "player level";
            parameter = "";
            additionalParameters = "";
            UpdateCMD();
        }

        private void role_adventure_level_TextChanged(object sender, TextChangedEventArgs e) //冒险等级
        {
            mainCMD = "player";
            parameter = role_adventure_level.Text;
            UpdateCMD();
        }

        private void role_role_level_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "level";
            parameter = "";
            additionalParameters = "";
            UpdateCMD();
        }

        private void role_role_level_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "level";
            parameter = role_role_level.Text;
            additionalParameters = "";
            UpdateCMD();
        }

        private void role_break_level_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "break";
            parameter = role_break_level.Text;
            additionalParameters = "";
            UpdateCMD();
        }

        private void role_break_level_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "break";
            parameter = role_break_level.Text;
            additionalParameters = "";
            UpdateCMD();
        }

        private void role_skillchoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainCMD = "skill";
            parameter = (string)((ComboBoxItem)role_skillchoice.SelectedItem).Content;
            additionalParameters = role_skill_level.Text;
            UpdateCMD();
        }

        private void role_skill_level_GotFocus(object sender, RoutedEventArgs e)
        {
            if (role_skillchoice.SelectedItem == null)
            {
                return;
            }
            mainCMD = "skill";
            parameter = (string)((ComboBoxItem)role_skillchoice.SelectedItem).Content;
            additionalParameters = role_skill_level.Text;
            UpdateCMD();
        }

        private void role_skill_level_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (role_skillchoice.SelectedItem == null)
            {
                return;
            }
            mainCMD = "skill";
            parameter = (string)((ComboBoxItem)role_skillchoice.SelectedItem).Content;
            additionalParameters = role_skill_level.Text;
            UpdateCMD();
        }

        private void role_addrole_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "item add";
            parameter = "";
            additionalParameters = "1";
            UpdateCMD();

            TextBox textBox = (TextBox)sender;
            currentFocusedBox = textBox;
            //textBox.SelectAll();

            if (textBox.Text == "点击后在此搜索或在右侧选择")
            {
                textBox.Text = "";
            }
            ReadFileAndCreateList("Avatar.txt", ref roleList);
        }

        private void role_addrole_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "item add";
            parameter = "";
            additionalParameters = "1";
            UpdateCMD();
            SearchInList(role_addrole.Text, ref roleList);
        }

        private void role_invicible_Click(object sender, RoutedEventArgs e)
        {
            CheckBox ch = (CheckBox)sender;
            if (ch.IsChecked == true)
            {
                commandBox.Text = "gm " + uidBox.Text + " wudi global avatar on";
            }
            else
            {
                commandBox.Text = "gm " + uidBox.Text + " wudi global avatar off";
            }

            RunCMDInChildThread();
        }

        private void role_infinite_physical_Click(object sender, RoutedEventArgs e)
        {
            CheckBox ch = (CheckBox)sender;
            if (ch.IsChecked == true)
            {
                commandBox.Text = "gm " + uidBox.Text + " stamina infinite on";
            }
            else
            {
                commandBox.Text = "gm " + uidBox.Text + " stamina infinite off";
            }

            RunCMDInChildThread();
        }

        private void role_infinite_elemental_burst_Click(object sender, RoutedEventArgs e)
        {
            CheckBox ch = (CheckBox)sender;
            if (ch.IsChecked == true)
            {
                commandBox.Text = "gm " + uidBox.Text + " wudi global avatar on";
            }
            else
            {
                commandBox.Text = "gm " + uidBox.Text + " wudi global avatar off";
            }

            RunCMDInChildThread();
        }

        private void role_unlock_Click(object sender, RoutedEventArgs e) //gm 1000 talent unlock all
        {
            mainCMD = "talent unlock all";
            parameter = "";
            additionalParameters = "";
            UpdateCMD();

            RunCMDInChildThread();
        }

        private void role_kill_Click(object sender, RoutedEventArgs e) //gm 1000 kill self
        {
            mainCMD = "kill self";
            parameter = "";
            additionalParameters = "";
            UpdateCMD();

            RunCMDInChildThread();
        }
        #endregion

        #region 武器页事件
        //武器
        private void coupon_id_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "equip add";
            parameter = "";
            additionalParameters = coupon_breaklevel.Text + " " + coupon_refinelevel.Text;
            UpdateCMD();

            TextBox textBox = (TextBox)sender;
            currentFocusedBox = textBox;
            //textBox.SelectAll();

            if (textBox.Text == "点击后在此搜索或在右侧选择")
            {
                textBox.Text = "";
            }
            ReadFileAndCreateList("Weapon.txt", ref weaponList);
        }

        private void coupon_id_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "equip add";
            //parameter = ;
            additionalParameters = coupon_breaklevel.Text + " " + coupon_refinelevel.Text;
            UpdateCMD();
            SearchInList(coupon_id.Text, ref weaponList);
        }

        private void coupon_breaklevel_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "equip add";
            UpdateCMD();
        }

        private void coupon_breaklevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "equip add";
            additionalParameters = coupon_breaklevel.Text + " " + coupon_refinelevel.Text;
            UpdateCMD();
        }

        private void coupon_refinelevel_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "equip add";
            UpdateCMD();
        }

        private void coupon_refinelevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "equip add";
            additionalParameters = coupon_breaklevel.Text + " " + coupon_refinelevel.Text;
            UpdateCMD();
        }
        #endregion

        #region 圣遗物页事件
        private void artifactID_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "item add";
            parameter = "";
            additionalParameters = coupon_breaklevel.Text + " " + coupon_refinelevel.Text;
            UpdateCMD();

            TextBox textBox = (TextBox)sender;
            currentFocusedBox = textBox;
            //textBox.SelectAll();

            if (textBox.Text == "点击后在此搜索或在右侧选择")
            {
                textBox.Text = "";
            }
            ReadFileAndCreateList("Artifact.txt", ref artifactList);

            if (artifactRankFilter.SelectedItem != null)
            {
                SearchInList(artifactID.Text, ref artifactList, (string)((ListBoxItem)artifactRankFilter.SelectedItem).Content);
            }
            else
            {
                SearchInList(artifactID.Text, ref artifactList);
            }
        }

        private void artifactID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (artifactAmount == null || artifactRankFilter == null)
            {
                return;
            }

            mainCMD = "item add";
            additionalParameters = artifactAmount.Text;
            UpdateCMD();

            if (artifactRankFilter.SelectedItem != null)
            {
                SearchInList(artifactID.Text, ref artifactList, (string)((ListBoxItem)artifactRankFilter.SelectedItem).Content);
            }
            else
            {
                SearchInList(artifactID.Text, ref artifactList);
            }

        }

        private void artifactAmount_GotFocus(object sender, RoutedEventArgs e)
        {
            mainCMD = "item add";
            additionalParameters = artifactAmount.Text;
            UpdateCMD();
        }

        private void artifactAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainCMD = "item add";
            additionalParameters = artifactAmount.Text;
            UpdateCMD();
        }

        private void artifactRankFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainCMD = "item add";
            SearchInList(artifactID.Text, ref artifactList, (string)((ListBoxItem)artifactRankFilter.SelectedItem).Content);
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
