using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Echo_of_London___Morse_Defence.Views
{
    public partial class ScoreboardView : UserControl
    {
        MainWindow oknoGlowne;

        public ScoreboardView(MainWindow mw)
        {
            InitializeComponent();
            oknoGlowne = mw;
            Loaded += (s, e) => WczytajWyniki();
        }

        void WczytajWyniki()
        {
            var wyniki = GameView.LoadScores();

            if (wyniki.Count == 0)
            {
                ScoresList.ItemsSource = null;
                NoScoresText.Visibility = Visibility.Visible;
                return;
            }

            NoScoresText.Visibility = Visibility.Collapsed;

            var listaDoWyswietlenia = wyniki.Take(20).Select((w, i) => new ScoreDisplayItem
            {
                Rank = (i + 1).ToString(),
                RankColor = PobierzKolorRangi(i + 1),
                PlayerName = w.PlayerName,
                Score = w.Score.ToString("N0"),
                Wave = w.Wave.ToString(),
                Difficulty = w.Difficulty,
                DifficultyColor = PobierzKolorTrudnosci(w.Difficulty),
                Date = w.Date,
                Hints = w.Hints ? "ON" : "OFF",
                HintsColor = w.Hints ? Brushes.Orange : (Brush)new BrushConverter().ConvertFrom("#029273")
            }).ToList();

            ScoresList.ItemsSource = listaDoWyswietlenia;
        }

        Brush PobierzKolorRangi(int miejsce)
        {
            if (miejsce == 1) return Brushes.Gold;
            if (miejsce == 2) return Brushes.Silver;
            if (miejsce == 3) return (Brush)new BrushConverter().ConvertFrom("#CD7F32");
            return (Brush)new BrushConverter().ConvertFrom("#029273");
        }

        Brush PobierzKolorTrudnosci(string trudnosc)
        {
            string t = trudnosc?.ToUpper();
            if (t == "EASY") return Brushes.LimeGreen;
            if (t == "MID" || t == "NORMAL") return Brushes.Orange;
            if (t == "HARD") return Brushes.Red;
            return (Brush)new BrushConverter().ConvertFrom("#029273");
        }

        void Main_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.NavigateTo(new StartView(oknoGlowne));
        }

        void ClearScores_Click(object sender, RoutedEventArgs e)
        {
            var odpowiedz = MessageBox.Show(
                "Are you sure you want to delete all scores?",
                "Clear Scoreboard",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (odpowiedz == MessageBoxResult.Yes)
            {
                string sciezka = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EchoOfLondon", "highscores.txt");

                if (File.Exists(sciezka))
                    File.Delete(sciezka);
                WczytajWyniki();
            }
        }
    }

    public class ScoreDisplayItem
    {
        public string Rank { get; set; }
        public Brush RankColor { get; set; }
        public string PlayerName { get; set; }
        public string Score { get; set; }
        public string Wave { get; set; }
        public string Difficulty { get; set; }
        public Brush DifficultyColor { get; set; }
        public string Date { get; set; }
        public string Hints { get; set; }
        public Brush HintsColor { get; set; }
    }
}