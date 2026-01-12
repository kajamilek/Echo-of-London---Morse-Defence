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
            SoundHelper.PlayClick();
            bool podpowiedzi = HintsCheckBox.IsChecked == true;
            bool polski = PolishCheckBox.IsChecked == true;
            oknoGlowne.NavigateTo(new GameView(oknoGlowne, "easy", podpowiedzi, polski));
        }

        void Mid_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            bool podpowiedzi = HintsCheckBox.IsChecked == true;
            bool polski = PolishCheckBox.IsChecked == true;
            oknoGlowne.NavigateTo(new GameView(oknoGlowne, "mid", podpowiedzi, polski));
        }

        void Hard_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            bool podpowiedzi = HintsCheckBox.IsChecked == true;
            bool polski = PolishCheckBox.IsChecked == true;
            oknoGlowne.NavigateTo(new GameView(oknoGlowne, "hard", podpowiedzi, polski));
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            oknoGlowne.GoBack();
        }
    }
}