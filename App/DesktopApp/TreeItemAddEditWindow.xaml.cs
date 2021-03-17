using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.DesktopApp.Service;
using ProjectBranchSelector.Models;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp
{
    /// <summary>
    /// Логика взаимодействия для AddTreeItemWindow.xaml
    /// </summary>
    public partial class TreeItemAddEditWindow : Window
    {
        public event EventHandler<AddTreeItemArgs> OnAdd;
        private TreeItemWindowMode _mode;
        private Guid? _id;
        private Guid? _parentId;
        private Guid? _treeId;
        private IServiceProvider _serviceProvider;
        private ILogger<TreeItemAddEditWindow> _logger;

        public TreeItemAddEditWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<TreeItemAddEditWindow>>();
        }

        public async void ShowDialog(TreeItemWindowMode mode, Guid? id, Guid? parentId, Guid treeId)
        {
            _mode = mode;
            _treeId = treeId;
            if (_mode == TreeItemWindowMode.Add)
            {
                Title = "Добавление элемента дерева";
                _parentId = parentId;
                try
                {
                    if (_parentId.HasValue)
                    {
                        var dataService = _serviceProvider.GetRequiredService<DataService>();
                        var parent = await dataService.GetTreeItem(_parentId.Value);
                        if (parent != null) ParentTextBox.Text = parent.Name;
                    }
                    ShowDialog();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка при получении родительского элемента: {ex.Message} {ex.StackTrace}");
                    MessageBox.Show($"Ошибка при получении родительского элемента: {ex.Message}");
                    Close();
                }                
            }
            else
            {
                Title = "Редактирование элемента дерева";
                _id = id;
                try
                {
                    if (_id.HasValue)
                    {
                        var dataService = _serviceProvider.GetRequiredService<DataService>();
                        var item = await dataService.GetTreeItem(_id.Value);
                        if (item != null)
                        {
                            NameTextBox.Text = item.Name;
                            DescriptionTextBox.Text = item.Description;
                            WeightTextBox.Text = item.Weight.ToString();
                            AddFieldsTextBox.Text = item.AddFields;
                            CountTextBox.Text = item.SelectCount.ToString();
                            IsLeafCheckBox.IsChecked = item.IsLeaf;                            
                            _parentId = item.ParentId;
                            if (item.Parent != null) ParentTextBox.Text = item.Parent.Name;
                            if (item.Tree != null) TreeTextBox.Text = item.Tree.Name;
                            ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show($"Элемент не найден в базе данных");
                            Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Не задан обязательный параметр: id");
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка при получении элемента: {ex.Message} {ex.StackTrace}");
                    MessageBox.Show($"Ошибка при получении элемента: {ex.Message}");
                    Close();
                }
            }           
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataService = _serviceProvider.GetRequiredService<DataService>();
                if (_mode == TreeItemWindowMode.Add)
                {
                    var res = await dataService.AddTreeItem(new TreeItemCreator() { 
                       AddFields = AddFieldsTextBox.Text,
                       Description = DescriptionTextBox.Text,
                       Name = NameTextBox.Text,
                       ParentId = _parentId,
                       TreeId = _treeId.Value,
                       Weight = int.Parse(WeightTextBox.Text)
                    });
                    if (res != null)
                    {
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Неизвестная ошибка при сохранении элемента");
                    }
                }
                else
                {
                    var res = await dataService.UpdateTreeItem(new TreeItemUpdater()
                    {
                        AddFields = AddFieldsTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        Name = NameTextBox.Text,
                        ParentId = _parentId,
                        TreeId = _treeId.Value,
                        Weight = int.Parse(WeightTextBox.Text),
                        Id = _id
                    });
                    if (res != null)
                    {
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Неизвестная ошибка при сохранении элемента");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при сохранении элемента: {ex.Message} {ex.StackTrace}");
                MessageBox.Show($"Ошибка при сохранении элемента: {ex.Message}");
            }        
        }

        private void ParentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window =  _serviceProvider.GetRequiredService<TreeItemsWindow>();
                window.OnSelect += (s, a) => {
                    _parentId = a.Id;
                    ParentTextBox.Text = a.Name;
                };
                window.ShowDialog(_treeId.Value, TreeItemsWindowMode.Select);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при выборе родительского элемента: {ex.Message} {ex.StackTrace}");
                MessageBox.Show($"Ошибка при выборе родительского элемента: {ex.Message}");
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

    public enum TreeItemWindowMode
    { 
       Add = 1,
       Edit = 2
    }

    public class AddTreeItemArgs
    { 
       public Guid Id { get; set; }
    }
}
