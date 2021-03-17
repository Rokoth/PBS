using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.DesktopApp.Service;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DesktopApp
{
    /// <summary>
    /// Логика взаимодействия для TreeItemsWindow.xaml
    /// </summary>
    public partial class TreeItemsWindow : Window
    {
        private Guid? _treeId = null;
        private IServiceProvider _serviceProvider;
        private IDataService dataService;
        private ILogger<TreeItemsWindow> _logger;
        private TreeItemsWindowMode _mode;

        public event EventHandler<TreeItemsWindowOnSelectArgs> OnSelect;

        public TreeItemsWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            dataService = serviceProvider.GetRequiredService<IDataService>();
            _logger = serviceProvider.GetRequiredService<ILogger<TreeItemsWindow>>();
        }

        public async void ShowDialog(Guid treeId, TreeItemsWindowMode mode)
        {
            _treeId = treeId;
            _mode = mode;
            var tree = await dataService.GetTree(treeId);
            if (tree == null)
            {
                MessageBox.Show("Дерево не найдено в базе данных");
                Close();
            }
            else
            {
                TreeLabel.Content = $"Элементы дерева {tree.Name}";
            }
            FillTable();
            ShowDialog();
        }

        private async void FillTable()
        {
            try
            {
                if (_treeId != null)
                {
                    TreeItemsView.Items.Clear();
                    var result = await dataService.GetTreeItems(_treeId.Value);
                    var items = result.Item2;
                    foreach (var item in await CreateItems(items, null))
                    {
                        TreeItemsView.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при получении списка элементов дерева: {ex.Message} {ex.StackTrace}");
                MessageBox.Show($"Ошибка при получении списка элементов дерева: {ex.Message}");
            }
        }

        private async Task<IEnumerable<TreeViewItem>> CreateItems(IEnumerable<TreeItemModel> items, Guid? parentId)
        {
            List<TreeViewItem> result = new List<TreeViewItem>();
            var toAdd = items.Where(s => s.ParentId == parentId);
            foreach (var item in toAdd)
            {
                var itemView = new TreeViewItem();
                var panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                panel.Children.Add(new Label { Content = item.Name });
                var addButton = new Button
                {
                    Name = $"AddChild{item.Id}",
                    Content = "Добавить дочерний элемент"
                };
                addButton.Click += (e, a) => {
                    var addWindow = _serviceProvider.GetRequiredService<TreeItemAddEditWindow>();                    
                    addWindow.OnAdd += (s, t) => {
                        FillTable();
                    };
                    addWindow.ShowDialog(TreeItemWindowMode.Add, null, item.Id, _treeId.Value);
                };
                panel.Children.Add(addButton);

                var editButton = new Button
                {
                    Name = $"Edit{item.Id}",
                    Content = "Редактировать"
                };
                editButton.Click += (e, a) => {
                    var editWindow = _serviceProvider.GetRequiredService<TreeItemAddEditWindow>();
                    editWindow.OnAdd += (s, t) => {
                        FillTable();
                    };
                    editWindow.ShowDialog(TreeItemWindowMode.Edit, item.Id, null, _treeId.Value);
                };
                panel.Children.Add(editButton);

                var deleteButton = new Button
                {
                    Name = $"Delete{item.Id}",
                    Content = "Удалить"
                };
                deleteButton.Click += async (e, a) => {
                    var delResult = MessageBox.Show("Вы действительно хотите удалить данный элемент?", "Удаление", MessageBoxButton.OKCancel);
                    if (delResult == MessageBoxResult.OK)
                    {
                        try
                        {
                            TreeItemModel res = await dataService.DeleteTreeItem(item.Id);
                            if (res != null)
                            {
                                FillTable();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Ошибка при удалении элемента дерева: {ex.Message} {ex.StackTrace}");
                            MessageBox.Show($"Ошибка при удалении элемента дерева: {ex.Message}");
                        }
                    }
                };
                panel.Children.Add(deleteButton);

                itemView.Header = panel;
                foreach (var child in await CreateItems(items, item.Id))
                {
                    itemView.Items.Add(child);
                }
                result.Add(itemView);                
            }
            return result;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = _serviceProvider.GetRequiredService<TreeItemAddEditWindow>();
            addWindow.OnAdd += (s, t) => {
                FillTable();
            };
            addWindow.ShowDialog(TreeItemWindowMode.Add, null, null, _treeId.Value);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class TreeItemsWindowOnSelectArgs
    { 
       public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public enum TreeItemsWindowMode
    { 
       List = 1,
       Select = 2
    }
}
