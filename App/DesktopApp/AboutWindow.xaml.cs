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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            textBlock.Inlines.AddRange(
                ParseInlines("Программа представляет собой приложение, выбирающее элемент из дерева, представленного в виде:" +
                "<LineBreak/>- структуры в базе данных," +
                "<LineBreak/>- структуры в файле (в формате json)," +
                "<LineBreak/>- структуры, переданной в API (в формате json), на основе нескольких параметров: " +
                "<LineBreak/>    - вес элемента," +
                "<LineBreak/>    - счетчик предыдущих выборов," +
                "<LineBreak/>    - число, сгенерированное генератором случайных чисел," +
                "<LineBreak/>    - либо любых других параметров,определенных в структуре элемента дерева и в формуле выбора." +
                "<LineBreak/>Приложение состоит из следующих частей: " +
                "<LineBreak/>- сервер, реализующий логику работы с БД, предоставляющий веб-приложение и API. Поставляется в вариантах для Windows и Linux;" +
                "<LineBreak/>- web-приложение ;" +
                "<LineBreak/>- десктопное приложение." +
                "<LineBreak/>Для всех частей приложения прилагаются программы установки (кроме web-приложения)." +
                "<LineBreak/>" +
                "<LineBreak/>Программа распространяется свободно по лицензии FreeBSD"));
        }


        private IEnumerable<Inline> ParseInlines(string text)
        {
            var textBlock = (TextBlock)XamlReader.Parse(
                "<TextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">"
                + text
                + "</TextBlock>");

            return textBlock.Inlines.ToList(); // must be enumerated
        }
    }
}
