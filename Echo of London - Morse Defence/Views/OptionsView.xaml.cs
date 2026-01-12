using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Echo_of_London___Morse_Defence.Views
{
    public partial class OptionsView : UserControl
    {
        MainWindow oknoGlowne;

        Brush tloAktywne = (Brush)new BrushConverter().ConvertFrom("#029273");
        Brush tekstAktywny = (Brush)new BrushConverter().ConvertFrom("#151b21");
        Brush tloNormalne = (Brush)new BrushConverter().ConvertFrom("#262c33");
        Brush tekstNormalny = (Brush)new BrushConverter().ConvertFrom("#029273");

        public OptionsView(MainWindow mw)
        {
            InitializeComponent();
            oknoGlowne = mw;
            OdswiezPrzyciski();
        }

        void OdswiezPrzyciski()
        {
            if (GameSettings.TrybJednegoPrzycisku)
            {
                btnOneButton.Background = tloAktywne;
                btnOneButton.Foreground = tekstAktywny;
                btnTwoButton.Background = tloNormalne;
                btnTwoButton.Foreground = tekstNormalny;
            }
            else
            {
                btnOneButton.Background = tloNormalne;
                btnOneButton.Foreground = tekstNormalny;
                btnTwoButton.Background = tloAktywne;
                btnTwoButton.Foreground = tekstAktywny;
            }
        }

        void OneButton_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            GameSettings.TrybJednegoPrzycisku = true;
            OdswiezPrzyciski();
        }

        void TwoButton_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            GameSettings.TrybJednegoPrzycisku = false;
            OdswiezPrzyciski();
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            oknoGlowne.GoBack();
        }
    }
}