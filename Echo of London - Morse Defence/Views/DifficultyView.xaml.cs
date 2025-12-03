using System.Windows;
using System.Windows.Controls;

namespace Echo_of_London___Morse_Defence.Views
{
    public partial class DifficultyView : UserControl
    {
        MainWindow oknoGlowne;

        public DifficultyView(MainWindow mw)
        {
            InitializeComponent();
            oknoGlowne = mw;
        }

        void Easy_Click(object sender, RoutedEventArgs e)
        {
            bool podpowiedzi = HintsCheckBox.IsChecked == true;
            oknoGlowne.NavigateTo(new GameView(oknoGlowne, "easy", podpowiedzi));
        }

        void Mid_Click(object sender, RoutedEventArgs e)
        {
            bool podpowiedzi = HintsCheckBox.IsChecked == true;
            oknoGlowne.NavigateTo(new GameView(oknoGlowne, "mid", podpowiedzi));
        }

        void Hard_Click(object sender, RoutedEventArgs e)
        {
            bool podpowiedzi = HintsCheckBox.IsChecked == true;
            oknoGlowne.NavigateTo(new GameView(oknoGlowne, "hard", podpowiedzi));
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.GoBack();
        }
    }
}