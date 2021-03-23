using DesktopApp.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectBranchSelector.BSHttpClient;
using ProjectBranchSelector.DesktopApp.Service;
using ProjectBranchSelector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Mode
        { 
           Tree = 1,
           Formula = 2
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IDataService _dataService;
        private readonly ISyncService syncService;
        private readonly IDbService _dbService;
        private readonly ILogger logger;
        private readonly IBSHttpClient httpClient;
        private Mode _mode = Mode.Tree;
        private bool modeChanged = true;

        private int page = 0;
        private int allPages = 1;

        private bool isLoaded = false;
        private bool needRefresh = false;

        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<ServerConnectEventArgs> OnServerConnected;        

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _dataService = _serviceProvider.GetRequiredService<IDataService>();
            _dbService = _serviceProvider.GetRequiredService<IDbService>();

            logger = _serviceProvider.GetRequiredService<ILogger<MainWindow>>();
            httpClient = _serviceProvider.GetRequiredService<IBSHttpClient>();
            OnError += MainWindow_OnError;
            OnServerConnected += MainWindow_OnServerConnected;

            syncService = _serviceProvider.GetRequiredService<ISyncService>();
            syncService.OnSync += (e, a) => { needRefresh = true; ; };
            syncService.Start();

            Task.Factory.StartNew(RunTimer, TaskCreationOptions.LongRunning);
            DataGridMain.MouseDoubleClick += EditButton_Click;
            
             isLoaded = true;
        }

        

        private async Task RunTimer()
        {
            while (true)
            {
                if (needRefresh)
                {
                    Dispatcher.Invoke(() => FillTable());
                    //FillTable();
                    needRefresh = false;
                }
                await Task.Delay(1000);
            }
        }

        private void MainWindow_OnServerConnected(object sender, ServerConnectEventArgs e)
        {
            MessageBox.Show($"Подключение к серверу {e.ServerAddress} успешно ");
            _dbService.SaveSettings("ServerAddress", e.ServerAddress);
        }

        private void MainWindow_OnError(object sender, ErrorEventArgs e)
        {
            MessageBox.Show("Произошла ошибка: " + e.Error);
        }

        private void ChangeMode(Mode mode)
        {
            if (_mode != mode)
            {
                modeChanged = true;
            }
            _mode = mode;
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
                    if (modeChanged)
                    {
                        page = 0;
                        DataGridMain.Columns.Clear();
                        DataGridMain.Columns.Add(GetTextColumn("Наименование", "Name"));
                        DataGridMain.Columns.Add(GetTextColumn("Описание", "Description"));
                        DataGridMain.Columns.Add(GetTextColumn("Формула", "Formula"));
                        DataGridMain.Columns.Add(GetTextColumn("Id", "Id", true));
                        DataGridMain.Columns.Add(GetTextColumn("FormulaId", "FormulaId", true));
                        modeChanged = false;
                    }

                    var result = await _dataService.GetTrees(FilterTextBox.Text, page, perPage, null);
                    allPages = (result.Item1 % perPage == 0) ? (result.Item1/ perPage) : ((result.Item1 / perPage) + 1);
                    foreach (var item in result.Item2)
                    {
                        DataGridMain.Items.Add(item);
                    }
                }
                else
                {
                    if (modeChanged)
                    {
                        page = 0;
                        DataGridMain.Columns.Clear();
                        DataGridMain.Columns.Add(GetTextColumn("Наименование", "Name"));
                        DataGridMain.Columns.Add(GetTextColumn("Текст", "Text"));
                        DataGridMain.Columns.Add(GetBoolColumn("По умолчанию", "IsDefault"));
                        DataGridMain.Columns.Add(GetTextColumn("Id", "Id", true));                       
                        modeChanged = false;
                    }
                    var result = await _dataService.GetFormulas(FilterTextBox.Text, page, perPage, null);
                    allPages = (result.Item1 % perPage == 0) ? (result.Item1 / perPage) : ((result.Item1 / perPage) + 1);
                    foreach (var item in result.Item2)
                    {
                        DataGridMain.Items.Add(item);
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

        private void ConnectToServer()
        {
            string serverAdress = null;
            var connectWindow = _serviceProvider.GetRequiredService<ServerConnectWindow>();
            connectWindow.OnSetServerAddress += (a,e)=> {
                serverAdress = e.ServerAddress;
            };
            connectWindow.ShowDialog();
            if (serverAdress != null)
            {
                if (!serverAdress.StartsWith("http"))
                {
                    serverAdress = "https://" + serverAdress;
                }

                httpClient.ConnectToServer(serverAdress, (isConnect, isError, error)=> {
                    if (isConnect)
                    {
                        OnServerConnected(this, new ServerConnectEventArgs() { 
                            ServerAddress = serverAdress
                        });
                    }
                    if (isError)
                    {
                        OnError(this, new ErrorEventArgs() { Error = error });
                    }
                });
            }
        }
        

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DataGridMain_Loaded(object sender, RoutedEventArgs e)
        {            
            FillTable();
        }

        private void ServerConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
        }

        private void SyncConflicts_Click(object sender, RoutedEventArgs e)
        {
            var syncConflictWindow = _serviceProvider.GetService<SyncConflictWindow>();
            syncConflictWindow.ShowDialog();
        }

        private void DataGridMain_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void AddTreeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == Mode.Tree)
            {
                var addTreeWindow = _serviceProvider.GetService<TreeAddEditWindow>();
                addTreeWindow.ShowDialog(AddEditTreeMode.Add, null);
            }
            else
            {
                var addFormulaWindow = _serviceProvider.GetService<FormulaAddWindow>();
                addFormulaWindow.ShowDialog();
            }
            FillTable();
        }

        private void ImportTreeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var addTreeWindow = _serviceProvider.GetService<TreeAddEditWindow>();
            addTreeWindow.ShowDialog(AddEditTreeMode.FromFile, null);
            FillTable();
        }

        private void ExportToFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var exportWindow = _serviceProvider.GetService<ExportToFileWindow>();
            exportWindow.ShowDialog();
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            FillTable();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TreeModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeMode(Mode.Tree);
            FillTable();
        }

        private void FormulaModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ChangeMode(Mode.Formula);
            FillTable();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FillTable();
        }

        private void BeginButton_Click(object sender, RoutedEventArgs e)
        {
            IncPage(false, true);
        }

        private void CountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            IncPage(false, false);
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            IncPage(true, false);
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            IncPage(true, true);
        }

        private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoaded)
            {
                page = 0;
                FillTable();
            }
        }

        private void DataGridCell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //EditSelected();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var windowAbout = _serviceProvider.GetRequiredService<AboutWindow>();
            windowAbout.ShowDialog();
        }

        private void ErrorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var windowError = _serviceProvider.GetRequiredService<ErrorReportWindow>();
            windowError.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshButton2_Click(object sender, RoutedEventArgs e)
        {
            FillTable();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditSelected();
        }

        private void EditSelected()
        {
            if (_mode == Mode.Tree)
            {
                var addTreeWindow = _serviceProvider.GetService<TreeAddEditWindow>();
                var row = DataGridMain.SelectedItem;
                if (row != null)
                {
                    addTreeWindow.ShowDialog(AddEditTreeMode.Edit, ((TreeModel)row).Id);
                }
            }
            else
            {
                var addFormulaWindow = _serviceProvider.GetService<FormulaAddWindow>();
                var row = DataGridMain.SelectedItem;
                if (row != null)
                {
                    addFormulaWindow.ShowDialogEdit(((FormulaModel)row).Id);
                }
            }
            FillTable();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelected();
        }
    }

    public class ErrorEventArgs
    {         
        public string Error { get; set; }
    }

    public class FillTableArgs
    { 
       public IEnumerable<TreeModel> Trees { get; set; }
    }
}
