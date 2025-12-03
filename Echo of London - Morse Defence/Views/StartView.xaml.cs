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
        private MainWindow _main;

        // Konstruktor bezparametrowy (dla kompatybilności)
        public StartView()
        {
            InitializeComponent();
            _main = Application.Current.MainWindow as MainWindow;
        }

        // Konstruktor z parametrem MainWindow
        public StartView(MainWindow main)
        {
            InitializeComponent();
            _main = main;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            _main.NavigateTo(new DifficultyView(_main));
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            _main.NavigateTo(new OptionsView(_main));
        }

        private void Scores_Click(object sender, RoutedEventArgs e)
        {
            _main.NavigateTo(new ScoreboardView(_main));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}