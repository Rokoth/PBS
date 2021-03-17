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
        public event EventHandler<FormulaSelectedArgs> OnFormulaSelected;

        public FormulaSelectWindow()
        {
            InitializeComponent();
        }
    }

    public class FormulaSelectedArgs
    {
        public Guid Id { get; set; }
    }
}
