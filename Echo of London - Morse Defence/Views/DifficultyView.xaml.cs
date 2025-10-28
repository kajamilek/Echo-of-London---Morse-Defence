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
    /// <summary>
    /// Logika interakcji dla klasy DifficultyView.xaml
    /// </summary>
    public partial class DifficultyView : UserControl
    {
        private MainWindow _main;

        public DifficultyView(MainWindow main)
        {
            InitializeComponent();
            _main = main;
        }

        private void Easy_Click(object sender, RoutedEventArgs e)
        {
            _main.NavigateTo(new GameView(_main, difficulty: "easy"));
        }

        private void Mid_Click(object sender, RoutedEventArgs e)
        {
            _main.NavigateTo(new GameView(_main, difficulty: "mid"));
        }

        private void Hard_Click(object sender, RoutedEventArgs e)
        {
            _main.NavigateTo(new GameView(_main, difficulty: "hard"));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.GoBack();
        }

        private void Tip_Checked(object sender, RoutedEventArgs e)
        {
        }
    }
}