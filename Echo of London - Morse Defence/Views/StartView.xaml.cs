using System.Windows;
using System.Windows.Controls;

namespace Echo_of_London___Morse_Defence.Views
{
    public partial class StartView : UserControl
    {
        MainWindow oknoGlowne;

        public StartView()
        {
            InitializeComponent();
            oknoGlowne = Application.Current.MainWindow as MainWindow;
        }

        public StartView(MainWindow mw)
        {
            InitializeComponent();
            oknoGlowne = mw;
        }

        void Start_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.NavigateTo(new DifficultyView(oknoGlowne));
        }

        void Options_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.NavigateTo(new OptionsView(oknoGlowne));
        }

        void Scores_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.NavigateTo(new ScoreboardView(oknoGlowne));
        }

        void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}