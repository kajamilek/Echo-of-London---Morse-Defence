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
            bool showHints = HintsCheckBox.IsChecked == true;
            _main.NavigateTo(new GameView(_main, difficulty: "easy", showHints: showHints));
        }

        private void Mid_Click(object sender, RoutedEventArgs e)
        {
            bool showHints = HintsCheckBox.IsChecked == true;
            _main.NavigateTo(new GameView(_main, difficulty: "mid", showHints: showHints));
        }

        private void Hard_Click(object sender, RoutedEventArgs e)
        {
            bool showHints = HintsCheckBox.IsChecked == true;
            _main.NavigateTo(new GameView(_main, difficulty: "hard", showHints: showHints));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.GoBack();
        }
    }
}