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
    /// Логика взаимодействия для AddTreeWindow.xaml
    /// </summary>
    public partial class TreeAddEditWindow : Window
    {
        private AddEditTreeMode _mode = AddEditTreeMode.Add;
        private IServiceProvider _serviceProvider;
        private IDataService _dataService;
        private Guid? formulaId = null;
        private Guid? _id = null;
        private ILogger<TreeAddEditWindow> _logger;

        public event EventHandler<ChangeTreeArgs> OnTreeAdded;

        public TreeAddEditWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _dataService = _serviceProvider.GetRequiredService<IDataService>();
            _logger = _serviceProvider.GetRequiredService<ILogger<TreeAddEditWindow>>();
           
        }

        private async void GetDefaultFormula()
        {
            var defFormula = await _dataService.GetDefaultFormula();
            if (defFormula.HasValue)
            {
                SetFormula(defFormula.Value);
            }
            else
            {
                MessageBox.Show("Не удалось получить формулу по умолчанию");
            }
        }

        private async void SetFormula(Guid id)
        {
            formulaId = id;
            var formula = await _dataService.GetFormula(id);
            FormulaTextBox.Text = formula.Name;
        }

        public async void ShowDialog(AddEditTreeMode mode, Guid? id)
        {
            _mode = mode;
            switch (mode)
            {
                case AddEditTreeMode.Add:
                    GetDefaultFormula();
                    break;
                case AddEditTreeMode.Edit:
                    _id = id;
                    var tree = await _dataService.GetTree(_id.Value);
                    NameTextBox.Text = tree.Name;
                    DescriptionTextBox.Text = tree.Description;                    
                    SetFormula(tree.FormulaId);
                    break;
                case AddEditTreeMode.FromFile:
                    throw new NotImplementedException("Импорт из файла не реализован");
                    break;
            }
            ShowDialog();
        }

        private async void SaveTree()
        {
            switch (_mode)
            {
                case AddEditTreeMode.Add:
                    if (string.IsNullOrEmpty(NameTextBox.Text))
                    {
                        MessageBox.Show("Не задано наименование");
                    }
                    else if (formulaId == null)
                    {
                        MessageBox.Show("Не задана формула");
                    }
                    else
                    {
                        try
                        {
                            var result = await _dataService.AddTree(new TreeCreator()
                            {
                                Description = DescriptionTextBox.Text,
                                FormulaId = formulaId.Value,
                                Name = NameTextBox.Text
                            });
                            if (result == null)
                                MessageBox.Show("Неизвестная ошибка при сохранении дерева");
                            else
                            {
                                if (OnTreeAdded != null)
                                    OnTreeAdded(this, new ChangeTreeArgs()
                                    {
                                        Id = result.Id
                                    });
                                Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Ошибка при сохранении дерева: {ex.Message} {ex.StackTrace}");
                            MessageBox.Show($"Ошибка при сохранении дерева: {ex.Message}");
                        }
                    }
                    break;
                case AddEditTreeMode.Edit:
                    if (string.IsNullOrEmpty(NameTextBox.Text))
                    {
                        MessageBox.Show("Не задано наименование");
                    }
                    else if (formulaId == null)
                    {
                        MessageBox.Show("Не задана формула");
                    }
                    else
                    {
                        try
                        {
                            var result = await _dataService.UpdateTree(new TreeUpdater()
                            {
                                Description = DescriptionTextBox.Text,
                                FormulaId = formulaId.Value,
                                Name = NameTextBox.Text,
                                Id = _id.Value
                            });
                            if (result == null)
                                MessageBox.Show("Неизвестная ошибка при сохранении дерева");
                            else
                            {
                                if (OnTreeAdded != null)
                                    OnTreeAdded(this, new ChangeTreeArgs()
                                    {
                                        Id = result.Id
                                    });
                                Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Ошибка при сохранении дерева: {ex.Message} {ex.StackTrace}");
                            MessageBox.Show($"Ошибка при сохранении дерева: {ex.Message}");
                        }
                    }
                    break;
                case AddEditTreeMode.FromFile:
                    throw new NotImplementedException("Импорт из файла не реализован");
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SaveTree();
        }

        private void FormulaButton_Click(object sender, RoutedEventArgs e)
        {
            var formulaWindow = _serviceProvider.GetRequiredService<FormulaSelectWindow>();
            formulaWindow.OnElementSelected += FormulaWindow_OnFormulaSelected;
            formulaWindow.ShowDialog();
        }

        private void FormulaWindow_OnFormulaSelected(object sender, ElementSelectedArgs e)
        {
            SetFormula(e.Id);
        }
    }

    public class ChangeTreeArgs
    { 
       public Guid Id { get; set; }
    }

    public enum AddEditTreeMode
    { 
       Add = 0,
       Edit = 1,
       FromFile = 2
    }
}
