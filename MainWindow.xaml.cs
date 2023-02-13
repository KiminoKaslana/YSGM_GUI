using System;
using System.Collections.Generic;
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
        }

        public string ExecuteCommand(string userInput)
        {
            string[] split = userInput!.Split(' ');
            string cmd = split[0];
            // If the user entered a valid command, execute it.
            if (CommandMap.handlers.ContainsKey(cmd))
            {
                var handler = CommandMap.handlers[cmd];
                var arguments = split.Skip(1).ToArray();

                return handler.Execute(arguments);
            }
            else
            {
                return "Invalid command.";//
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            callback.Content = ExecuteCommand(commandBox.Text);
        }
    }
}
