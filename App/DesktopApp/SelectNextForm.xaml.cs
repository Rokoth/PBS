using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.DesktopApp.Service;
using ProjectBranchSelector.Models;
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
    /// Логика взаимодействия для SelectNextForm.xaml
    /// </summary>
    public partial class SelectNextForm : Window
    {
        private IServiceProvider _serviceProvider;
        private Guid _treeId;
        private ILogger<SelectNextForm> _logger;

        public SelectNextForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<SelectNextForm>>();
        }

        public void ShowDialog(Guid treeId)
        {
            _treeId = treeId;
            SelectNext();
            ShowDialog();
        }

        private async void SelectNext()
        {
            try
            {
                var dataService = _serviceProvider.GetRequiredService<DataService>();
                var result = (await dataService.SelectItem(new SelectRequest()
                {
                    Count = 1,
                    LeafOnly = true,
                    TreeId = _treeId
                })).Result.FirstOrDefault();
                ResultTextBox.Text = string.Join(" - ", result.FullPath);
                if (WindowsRadioButton.IsChecked == true)
                {
                    FSResultTextBox.Text = string.Join("\\", result.FullPath);
                }
                else
                {
                    FSResultTextBox.Text = string.Join("/", result.FullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении элемента: {ex.Message} StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Ошибка при получении элемента: {ex.Message}");
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            SelectNext();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
