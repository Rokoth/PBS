using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.BSHttpClient;
using ProjectBranchSelector.DbClient;
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
    /// Логика взаимодействия для ErrorReportWindow.xaml
    /// </summary>
    public partial class ErrorReportWindow : Window
    {
        private IServiceProvider _serviceProvider;
        private ILogger<ErrorReportWindow> _logger;

        public ErrorReportWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<ErrorReportWindow>>();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var httpClient = _serviceProvider.GetRequiredService<IBSHttpClient<ReportHttpClientSettings>>();
            try
            {
                var res = await httpClient.Post<ReportMessage, string>(new ReportMessage() { 
                  Level = ((ComboBoxItem)LevelComboBox.SelectedItem).Name,
                  Message = MessageTextBox.Text,
                  Title = TitleTextBox.Text
                });
                MessageBox.Show($"Сообщение отправлено: {res}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при отправке сообщения: {ex.Message} {ex.StackTrace}");
                MessageBox.Show($"Ошибка при отправке сообщения: {ex.Message}");
            }
        }
    }

    
}
