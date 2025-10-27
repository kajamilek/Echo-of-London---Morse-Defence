using Echo_of_London___Morse_Defence.Views;
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

namespace Echo_of_London___Morse_Defence
{
    /// <summary>
    /// Logika interakcji dla klasy StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            var menu = new DifficultyWindow();
            Application.Current.MainWindow = menu;
            menu.Show();
            this.Close();
        }

        private void Button_Click_Options(object sender, RoutedEventArgs e)
        {
            var menu = new OptionsWindow();
            Application.Current.MainWindow = menu;
            menu.Show();
            this.Close();
        }


        private void Button_Click_Out(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_Scoreboard(object sender, RoutedEventArgs e)
        {
            var menu = new ScoreboardWindow();
            Application.Current.MainWindow = menu;
            menu.Show();
            this.Close();
        }
    }
}
