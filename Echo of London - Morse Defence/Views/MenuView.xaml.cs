using System.Windows;
using System.Windows.Controls;

namespace Echo_of_London___Morse_Defence.Views
{
    public partial class MenuView : UserControl
    {
        MainWindow oknoGlowne;

        string zapamietanaTrudnosc;
        bool zapamietanePodpowiedzi;

        public MenuView(MainWindow mw, string trudnosc = "normal", bool podpowiedzi = false)
        {
            InitializeComponent();
            oknoGlowne = mw;
            zapamietanaTrudnosc = trudnosc;
            zapamietanePodpowiedzi = podpowiedzi;
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.GoBack();
        }

        void Options_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.NavigateTo(new OptionsView(oknoGlowne));
        }

        void Reset_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.NavigateTo(new GameView(oknoGlowne, zapamietanaTrudnosc, zapamietanePodpowiedzi));
        }

        void Exit_Click(object sender, RoutedEventArgs e)
        {
            // wróć do gry i wywołaj game over
            oknoGlowne.GoBack();

            if (oknoGlowne.MainContent.Content is GameView gra)
            {
                gra.WymusGameOver();
            }
        }
    }
}