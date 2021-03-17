using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.DesktopApp.Service;
using ProjectBranchSelector.Models;
using System;
using System.Windows;

namespace DesktopApp
{
    /// <summary>
    /// Логика взаимодействия для FormulaAddWindow.xaml
    /// </summary>
    public partial class FormulaAddWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public event EventHandler<FormulaAddArgs> OnAddEvent;
        public event EventHandler<FormulaAddArgs> OnUpdateEvent;
        private readonly ILogger<FormulaAddWindow> _logger;
        private FormulaWindowMode _mode;
        private Guid? _id;

        public FormulaAddWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<FormulaAddWindow>>();
        }

        public void ShowDialogAdd()
        {
            _mode = FormulaWindowMode.Add;
            ShowDialog();
        }

        public async void ShowDialogEdit(Guid id)
        {
            try
            {
                _mode = FormulaWindowMode.Edit;
                _id = id;
                var dataService = _serviceProvider.GetRequiredService<IDataService>();
                var formula = await dataService.GetFormula(id);
                if (formula != null)
                {
                    NameTextBox.Text = formula.Name;
                    DefaultCheckBox.IsChecked = formula.IsDefault;
                    FormulaTextBox.Text = formula.Text;
                    ShowDialog();
                }
                else
                {
                    MessageBox.Show($"Формула не найдена");
                    Close();
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна редактирования формулы: {ex.Message}");
                _logger.LogError($"Ошибка при открытии окна редактирования формулы: {ex.Message} {ex.StackTrace}");
                Close();
            }
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_mode)
            {
                case FormulaWindowMode.Add:
                    try
                    {
                        var dataService = _serviceProvider.GetRequiredService<IDataService>();
                        FormulaModel result = await dataService.AddFormula(new FormulaCreator()
                        {
                            IsDefault = DefaultCheckBox.IsChecked == true,
                            Name = NameTextBox.Text,
                            Text = FormulaTextBox.Text
                        });
                        if (result != null)
                        {
                            OnAddEvent?.Invoke(this, new FormulaAddArgs()
                            {
                                Id = result.Id
                            });
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Необработанная ошибка при добавлении формулы");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Ошибка при добавлении формулы: {ex.Message} {ex.StackTrace}");
                        MessageBox.Show($"Ошибка при добавлении формулы: {ex.Message}");
                    }
                    break;
                case FormulaWindowMode.Edit:
                    try
                    {
                        var dataService = _serviceProvider.GetRequiredService<IDataService>();
                        FormulaModel result = await dataService.UpdateFormula(new FormulaUpdater()
                        { 
                            Id = _id.Value,
                            IsDefault = DefaultCheckBox.IsChecked == true,
                            Name = NameTextBox.Text,
                            Text = FormulaTextBox.Text
                        });
                        if (result != null)
                        {
                            OnUpdateEvent?.Invoke(this, new FormulaAddArgs()
                            {
                                Id = result.Id
                            });
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Необработанная ошибка при обновлении формулы");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Ошибка при обновлении формулы: {ex.Message} {ex.StackTrace}");
                        MessageBox.Show($"Ошибка при обновлении формулы: {ex.Message}");
                    }
                    break;
            }            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class FormulaAddArgs
    { 
       public Guid Id { get; set; }
    }

    public enum FormulaWindowMode
    { 
       Add = 1,
       Edit = 2
    }
}
