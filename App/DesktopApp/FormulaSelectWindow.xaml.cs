using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.DesktopApp.Service;
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
    /// Логика взаимодействия для FormulaSelectWindow.xaml
    /// </summary>
    public partial class FormulaSelectWindow : Window
    {
        public event EventHandler<ElementSelectedArgs> OnElementSelected;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataService _dataService;
        private readonly ILogger logger;

        private int page = 0;
        private int allPages = 1;

        private Mode _mode = Mode.Tree;
        public enum Mode
        {
            Tree = 1,
            Formula = 2
        }

        public FormulaSelectWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _dataService = _serviceProvider.GetRequiredService<IDataService>();
            logger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();
        }

        public void ShowDialog(Mode mode)
        {
            _mode = mode;
            FillTable();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void IncPage(bool inc, bool end)
        {
            bool changed = false;
            if (inc)
            {
                if (page + 1 < allPages)
                {
                    if (end)
                    {
                        page = allPages - 1;
                    }
                    else
                    {
                        page++;
                    }
                    changed = true;
                }
            }
            else
            {
                if (page > 0)
                {
                    if (end)
                    {
                        page = 0;
                    }
                    else
                    {
                        page--;
                    }
                    changed = true;
                }
            }
            if (changed)
            {
                FillTable();
            }
        }

        private async void FillTable()
        {
            try
            {
                var perPage = int.Parse(CountTextBox.Text);
                DataGridMain.Items.Clear();
                if (_mode == Mode.Tree)
                {
                    page = 0;
                    DataGridMain.Columns.Clear();
                    DataGridMain.Columns.Add(GetTextColumn("Наименование", "Name"));
                    DataGridMain.Columns.Add(GetTextColumn("Описание", "Description"));
                    DataGridMain.Columns.Add(GetTextColumn("Формула", "Formula"));
                    DataGridMain.Columns.Add(GetTextColumn("Id", "Id", true));
                    DataGridMain.Columns.Add(GetTextColumn("FormulaId", "FormulaId", true));
                    DataGridMain.Columns.Add(new DataGridTextColumn());
                    
                    var result = await _dataService.GetTrees(FilterTextBox.Text, page, perPage, null);
                    allPages = (result.Item1 % perPage == 0) ? (result.Item1 / perPage) : ((result.Item1 / perPage) + 1);
                    foreach (var item in result.Item2)
                    {
                        var selectButton = new Button()
                        { 
                          Content = "Выбрать",
                          Height = 30,
                          Width = 75
                        };
                        selectButton.Click += (e, a) => {
                            if (OnElementSelected != null) OnElementSelected(this, new ElementSelectedArgs()
                            { 
                              Id = item.Id
                            });
                        };
                        DataGridMain.Items.Add(new { item.Name, item.Description, item.Formula, item.Id, item.FormulaId, Select = selectButton });
                    }
                }
                else
                {
                    page = 0;
                    DataGridMain.Columns.Clear();
                    DataGridMain.Columns.Add(GetTextColumn("Наименование", "Name"));
                    DataGridMain.Columns.Add(GetTextColumn("Текст", "Text"));
                    DataGridMain.Columns.Add(GetBoolColumn("По умолчанию", "IsDefault"));
                    DataGridMain.Columns.Add(GetTextColumn("Id", "Id", true));
                    DataGridMain.Columns.Add(new DataGridTextColumn());

                    var result = await _dataService.GetFormulas(FilterTextBox.Text, page, perPage, null);
                    allPages = (result.Item1 % perPage == 0) ? (result.Item1 / perPage) : ((result.Item1 / perPage) + 1);
                    foreach (var item in result.Item2)
                    {
                        var selectButton = new Button()
                        {
                            Content = "Выбрать",
                            Height = 30,
                            Width = 75
                        };
                        selectButton.Click += (e, a) => {
                            if (OnElementSelected != null) OnElementSelected(this, new ElementSelectedArgs()
                            {
                                Id = item.Id
                            });
                        };
                        DataGridMain.Items.Add(new { item.Name, item.Text, item.IsDefault, item.Id, Select = selectButton });
                    }
                }

                CountLabel.Content = $"Страница {page + 1} из {allPages}";
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aex)
                {
                    var message = "";
                    var stack = "";
                    foreach (var exs in aex.InnerExceptions)
                    {
                        message += exs.Message + "\r\n";
                        stack += exs.StackTrace + "\r\n";
                    }
                    logger.LogError($"Ошибка при получении списка деревьев: {message} {stack}");
                    MessageBox.Show($"Ошибка при получении списка деревьев: {message}");
                }
                else
                {
                    logger.LogError($"Ошибка при получении списка деревьев: {ex.Message} {ex.StackTrace}");
                    MessageBox.Show($"Ошибка при получении списка деревьев: {ex.Message}");
                }
            }
        }

        private DataGridTextColumn GetTextColumn(string header, string binding, bool hidden = false)
        {
            DataGridTextColumn column = new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(binding)
            };
            if (hidden) column.Visibility = Visibility.Hidden;
            return column;
        }

        private DataGridCheckBoxColumn GetBoolColumn(string header, string binding, bool hidden = false)
        {
            DataGridCheckBoxColumn column = new DataGridCheckBoxColumn
            {
                Header = header,
                Binding = new Binding(binding)
            };
            if (hidden) column.Visibility = Visibility.Hidden;
            return column;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DataGridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGridMain_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BeginButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CountTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void CountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }

    public class ElementSelectedArgs
    {
        public Guid Id { get; set; }
    }
}
