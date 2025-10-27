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
    public partial class StartView : UserControl
    {
        public StartView(MainWindow main)
        {
            InitializeComponent();
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // Przejście do widoku wyboru trudności
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.NavigateTo(new DifficultyView(mainWindow));
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            // Przejście do widoku opcji
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.NavigateTo(new OptionsView(mainWindow));
        }

        private void Scores_Click(object sender, RoutedEventArgs e)
        {
            // Przejście do widoku wyników 
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.NavigateTo(new ScoreboardView(mainWindow));
        }        
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
