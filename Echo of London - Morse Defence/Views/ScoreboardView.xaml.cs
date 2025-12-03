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
        private MainWindow _main;

        public ScoreboardView(MainWindow main)
        {
            InitializeComponent();
            _main = main;

            Loaded += ScoreboardView_Loaded;
        }

        private void ScoreboardView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAndDisplayScores();
        }

        private void LoadAndDisplayScores()
        {
            var scores = GameView.LoadScores();

            if (scores.Count == 0)
            {
                ScoresList.ItemsSource = null;
                NoScoresText.Visibility = Visibility.Visible;
                return;
            }

            NoScoresText.Visibility = Visibility.Collapsed;

            // Przygotuj dane do wyświetlenia (max 20 wyników)
            var displayScores = scores.Take(20).Select((s, index) => new ScoreDisplayItem
            {
                Rank = (index + 1).ToString(),
                RankColor = GetRankColor(index + 1),
                PlayerName = s.PlayerName,
                Score = s.Score.ToString("N0"),
                Wave = s.Wave.ToString(),
                Difficulty = s.Difficulty,
                DifficultyColor = GetDifficultyColor(s.Difficulty),
                Date = s.Date
            }).ToList();

            ScoresList.ItemsSource = displayScores;
        }

        private Brush GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return Brushes.Gold;
                case 2: return Brushes.Silver;
                case 3: return (Brush)new BrushConverter().ConvertFrom("#CD7F32"); // Bronze
                default: return (Brush)new BrushConverter().ConvertFrom("#029273");
            }
        }

        private Brush GetDifficultyColor(string difficulty)
        {
            switch (difficulty?.ToUpper())
            {
                case "EASY": return Brushes.LimeGreen;
                case "MID":
                case "NORMAL": return Brushes.Orange;
                case "HARD": return Brushes.Red;
                default: return (Brush)new BrushConverter().ConvertFrom("#029273");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _main.GoBack();
        }

        private void ClearScores_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete all scores?",
                "Clear Scoreboard",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string scoresPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "EchoOfLondon",
                        "highscores.txt");

                    if (File.Exists(scoresPath))
                    {
                        File.Delete(scoresPath);
                    }

                    LoadAndDisplayScores();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing scores: {ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Klasa pomocnicza do wyświetlania
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
    }
}