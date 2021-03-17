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
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Логика взаимодействия для ServerConnectWindow.xaml
    /// </summary>
    public partial class ServerConnectWindow : Window
    {
        public event EventHandler<ServerConnectEventArgs> OnSetServerAddress;
        public ServerConnectWindow()
        {
            InitializeComponent();
        }

        private void OnServerAdressSet()
        {
            OnSetServerAddress?.Invoke(this, new ServerConnectEventArgs()
            {
                ServerAddress = ServerAdressTextBox.Text
            });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnServerAdressSet();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ServerConnectEventArgs
    { 
       public string ServerAddress { get; set; }
    }
}
