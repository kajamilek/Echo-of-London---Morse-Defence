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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Echo_of_London___Morse_Defence.Views
{
    public partial class OptionsView : UserControl
    {
        public OptionsView(MainWindow mainWindow)
        {
            InitializeComponent();
        }

        private void OneButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TwoButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.GoBack();
        }
    }
}