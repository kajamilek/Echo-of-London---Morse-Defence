using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Echo_of_London___Morse_Defence.Views
{
    public partial class GameView : UserControl
    {
        private MainWindow main;
        private Random rng = new Random();
        private DispatcherTimer enemyTimer;
        private double centerX;
        private double centerY;

        private readonly double[] sectorAngles = { 0, 60, 120, 180, 240, 300 };

        private int enemyCount = 0;
        private string currentLetters = "";

        // === Obsługa wpisywania Morse'a ===
        private string currentMorseInput = "";
        private const int MAX_MORSE_LENGTH = 6;
        private TextBlock[] morseSlots;

        // Timer do automatycznego zatwierdzania
        private DispatcherTimer inputTimer;

        // === Śledzenie przeciwników w sektorach ===
        private List<EnemyData> enemies = new List<EnemyData>();

        // === USTAWIENIA TRUDNOŚCI ===
        private string difficulty;
        private double enemySpawnInterval;
        private double enemyMoveDuration;
        private double inputDelaySeconds;

        // === HINTS ===
        private bool showHints;

        // === SYSTEM ŻYCIA I PUNKTÓW ===
        private int lives = 3;
        private int score = 0;
        private int wave = 1;
        private bool isGameOver = false;
        private bool scoreSaved = false;

        // Kolor linii na kole
        private readonly Brush lineColor = (Brush)new BrushConverter().ConvertFrom("#029273");

        // Ścieżka do pliku z wynikami
        private static readonly string ScoresFilePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EchoOfLondon",
            "highscores.txt");

        private class EnemyData
        {
            public UIElement Element { get; set; }
            public double AngleDegrees { get; set; }
            public int Sector { get; set; }
            public DateTime SpawnTime { get; set; }
            public DoubleAnimation AnimX { get; set; }
            public DoubleAnimation AnimY { get; set; }
        }

        // Kody Morse'a
        private Dictionary<char, string> morse = new Dictionary<char, string>()
        {
            {'A',"•–"},      {'B',"–•••"},
            {'C',"–•–•"},    {'D',"–••"},
            {'E',"•"},       {'F',"••–•"},
            {'G',"––•"},     {'H',"••••"},
            {'I',"••"},      {'J',"•–––"},
            {'K',"–•–"},     {'L',"•–••"},
            {'M',"––"},      {'N',"–•"},
            {'O',"–––"},     {'P',"•––•"},
            {'Q',"––•–"},    {'R',"•–•"},
            {'S',"•••"},     {'T',"–"},
            {'U',"••–"},     {'V',"•••–"},
            {'W',"•––"},     {'X',"–••–"},
            {'Y',"–•––"},    {'Z',"––••"}
        };

        public GameView(MainWindow main, string difficulty, bool showHints = false)
        {
            InitializeComponent();
            this.main = main;
            this.difficulty = difficulty;
            this.showHints = showHints;

            SetDifficultyParameters();

            Loaded += GameView_Loaded;
        }

        private void SetDifficultyParameters()
        {
            switch (difficulty.ToLower())
            {
                case "easy":
                    enemySpawnInterval = 3.0;
                    enemyMoveDuration = 5.0;
                    inputDelaySeconds = 1.0;
                    break;

                case "mid":
                case "normal":
                    enemySpawnInterval = 2.0;
                    enemyMoveDuration = 3.0;
                    inputDelaySeconds = 0.8;
                    break;

                case "hard":
                    enemySpawnInterval = 1.2;
                    enemyMoveDuration = 2.0;
                    inputDelaySeconds = 0.6;
                    break;

                default:
                    enemySpawnInterval = 2.0;
                    enemyMoveDuration = 3.0;
                    inputDelaySeconds = 0.8;
                    break;
            }
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            centerX = EnemyCanvas.ActualWidth / 2;
            centerY = EnemyCanvas.ActualHeight / 2;

            morseSlots = new TextBlock[]
            {
                MorseSlot0, MorseSlot1, MorseSlot2,
                MorseSlot3, MorseSlot4, MorseSlot5
            };

            HintsPanel.Visibility = showHints ? Visibility.Visible : Visibility.Collapsed;

            UpdateLivesDisplay();
            UpdateScoreDisplay();
            UpdateWaveDisplay();

            inputTimer = new DispatcherTimer();
            inputTimer.Interval = TimeSpan.FromSeconds(inputDelaySeconds);
            inputTimer.Tick += InputTimer_Tick;

            this.Focusable = true;
            this.Focus();
            this.KeyDown += GameView_KeyDown;
            this.MouseDown += (s, ev) => this.Focus();

            enemyTimer = new DispatcherTimer();
            enemyTimer.Interval = TimeSpan.FromSeconds(enemySpawnInterval);
            enemyTimer.Tick += (s, ev) => SpawnEnemy();
            enemyTimer.Start();

            StartNewTurn();
        }

        // ============================================================
        // SYSTEM ŻYCIA I PUNKTÓW
        // ============================================================
        private void UpdateLivesDisplay()
        {
            string hearts = new string('♥', Math.Max(0, lives));
            string emptyHearts = new string('♡', Math.Max(0, 3 - lives));
            LivesText.Text = hearts + emptyHearts;

            if (lives <= 1)
                LivesText.Foreground = Brushes.Red;
            else if (lives == 2)
                LivesText.Foreground = Brushes.Orange;
            else
                LivesText.Foreground = (Brush)new BrushConverter().ConvertFrom("#ff5555");
        }

        private void UpdateScoreDisplay()
        {
            ScoreText.Text = score.ToString();
        }

        private void UpdateWaveDisplay()
        {
            WaveText.Text = wave.ToString();
        }

        private void LoseLife()
        {
            if (isGameOver) return;

            lives--;
            UpdateLivesDisplay();
            FlashScreen();

            if (lives <= 0)
            {
                GameOver();
            }
        }

        private void AddScore(int points)
        {
            score += points;
            UpdateScoreDisplay();
        }

        private void FlashScreen()
        {
            var originalFill = player.Fill;
            player.Fill = Brushes.Red;

            var resetTimer = new DispatcherTimer();
            resetTimer.Interval = TimeSpan.FromMilliseconds(200);
            resetTimer.Tick += (s, e) =>
            {
                resetTimer.Stop();
                player.Fill = originalFill;
            };
            resetTimer.Start();
        }

        private void GameOver()
        {
            isGameOver = true;
            scoreSaved = false;

            enemyTimer?.Stop();
            inputTimer?.Stop();

            foreach (var enemy in enemies.ToList())
            {
                enemy.Element.BeginAnimation(Canvas.LeftProperty, null);
                enemy.Element.BeginAnimation(Canvas.TopProperty, null);
                EnemyCanvas.Children.Remove(enemy.Element);
            }
            enemies.Clear();

            FinalScoreText.Text = $"SCORE: {score}";
            FinalWaveText.Text = $"WAVE: {wave}";
            SaveConfirmText.Text = "";
            PlayerNameTextBox.Text = "PLAYER";
            GameOverOverlay.Visibility = Visibility.Visible;

            // Focus na pole nazwy
            PlayerNameTextBox.Focus();
            PlayerNameTextBox.SelectAll();
        }

        // ============================================================
        // SYSTEM ZAPISU WYNIKÓW
        // ============================================================
        private void SaveScore_Click(object sender, RoutedEventArgs e)
        {
            if (scoreSaved)
            {
                SaveConfirmText.Text = "ALREADY SAVED!";
                SaveConfirmText.Foreground = Brushes.Orange;
                return;
            }

            string playerName = PlayerNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "PLAYER";
            }

            // Usuń znaki specjalne
            playerName = new string(playerName.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
            if (playerName.Length > 15)
                playerName = playerName.Substring(0, 15);

            try
            {
                SaveScoreToFile(playerName, score, wave, difficulty);
                scoreSaved = true;
                SaveConfirmText.Text = "SCORE SAVED!";
                SaveConfirmText.Foreground = (Brush)new BrushConverter().ConvertFrom("#12b491");
            }
            catch (Exception ex)
            {
                SaveConfirmText.Text = "SAVE FAILED!";
                SaveConfirmText.Foreground = Brushes.Red;
                System.Diagnostics.Debug.WriteLine($"Error saving score: {ex.Message}");
            }
        }

        private void SaveScoreToFile(string playerName, int score, int wave, string difficulty)
        {
            // Utwórz folder jeśli nie istnieje
            string directory = System.IO.Path.GetDirectoryName(ScoresFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Format: NAME|SCORE|WAVE|DIFFICULTY|DATE
            string scoreEntry = $"{playerName}|{score}|{wave}|{difficulty.ToUpper()}|{DateTime.Now:yyyy-MM-dd HH:mm}";

            // Dodaj do pliku
            using (StreamWriter sw = File.AppendText(ScoresFilePath))
            {
                sw.WriteLine(scoreEntry);
            }

            System.Diagnostics.Debug.WriteLine($"Score saved: {scoreEntry}");
        }

        // Statyczna metoda do odczytu wyników (używana przez HighScoresView)
        public static List<ScoreEntry> LoadScores()
        {
            var scores = new List<ScoreEntry>();

            if (!File.Exists(ScoresFilePath))
                return scores;

            try
            {
                string[] lines = File.ReadAllLines(ScoresFilePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length >= 5)
                    {
                        scores.Add(new ScoreEntry
                        {
                            PlayerName = parts[0],
                            Score = int.Parse(parts[1]),
                            Wave = int.Parse(parts[2]),
                            Difficulty = parts[3],
                            Date = parts[4]
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading scores: {ex.Message}");
            }

            // Sortuj od najwyższego wyniku
            return scores.OrderByDescending(s => s.Score).ToList();
        }

        // ============================================================
        // OBSŁUGA KLAWIATURY
        // ============================================================
        private void GameView_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver) return;

            switch (e.Key)
            {
                case Key.Left:
                    AddMorseSymbol("–");
                    e.Handled = true;
                    break;

                case Key.Right:
                    AddMorseSymbol("•");
                    e.Handled = true;
                    break;

                case Key.Space:
                case Key.Enter:
                    SubmitMorseCode();
                    e.Handled = true;
                    break;

                case Key.Back:
                case Key.Delete:
                    RemoveLastSymbol();
                    e.Handled = true;
                    break;

                case Key.Escape:
                    ClearMorseInput();
                    e.Handled = true;
                    break;
            }
        }

        private void AddMorseSymbol(string symbol)
        {
            if (currentMorseInput.Length >= MAX_MORSE_LENGTH)
                return;

            currentMorseInput += symbol;
            UpdateMorseDisplay();

            inputTimer.Stop();
            inputTimer.Start();
        }

        private void RemoveLastSymbol()
        {
            if (currentMorseInput.Length > 0)
            {
                currentMorseInput = currentMorseInput.Substring(0, currentMorseInput.Length - 1);
                UpdateMorseDisplay();
            }

            inputTimer.Stop();
            if (currentMorseInput.Length > 0)
                inputTimer.Start();
        }

        private void ClearMorseInput()
        {
            currentMorseInput = "";
            UpdateMorseDisplay();
            inputTimer.Stop();
        }

        private void UpdateMorseDisplay()
        {
            int startSlot = MAX_MORSE_LENGTH - currentMorseInput.Length;

            for (int i = 0; i < morseSlots.Length; i++)
            {
                if (i >= startSlot)
                {
                    int inputIndex = i - startSlot;
                    morseSlots[i].Text = currentMorseInput[inputIndex].ToString();
                    morseSlots[i].Foreground = lineColor;
                }
                else
                {
                    morseSlots[i].Text = "";
                    morseSlots[i].Foreground = lineColor;
                }
            }
        }

        private void InputTimer_Tick(object sender, EventArgs e)
        {
            inputTimer.Stop();
            SubmitMorseCode();
        }

        private void SubmitMorseCode()
        {
            inputTimer.Stop();

            if (string.IsNullOrEmpty(currentMorseInput))
                return;

            char? matchedLetter = null;
            foreach (var kvp in morse)
            {
                if (kvp.Value == currentMorseInput)
                {
                    matchedLetter = kvp.Key;
                    break;
                }
            }

            if (matchedLetter.HasValue)
            {
                int sectorIndex = currentLetters.IndexOf(matchedLetter.Value);

                if (sectorIndex >= 0)
                {
                    bool destroyed = DestroyEnemyInSector(sectorIndex);

                    if (destroyed)
                    {
                        OnEnemyDestroyed(matchedLetter.Value, sectorIndex);
                    }
                    else
                    {
                        OnNoEnemyInSector(matchedLetter.Value, sectorIndex);
                    }
                }
                else
                {
                    OnWrongLetter(matchedLetter.Value);
                }
            }
            else
            {
                OnInvalidCode();
            }

            ClearMorseInput();
        }

        // ============================================================
        // NISZCZENIE PRZECIWNIKÓW
        // ============================================================
        private int GetSectorFromAngle(double angleDegrees)
        {
            angleDegrees = ((angleDegrees % 360) + 360) % 360;
            double shifted = (angleDegrees + 30) % 360;
            int sector = (int)(shifted / 60);
            return sector;
        }

        private bool DestroyEnemyInSector(int sector)
        {
            var enemiesInSector = enemies
                .Where(e => e.Sector == sector)
                .OrderBy(e => e.SpawnTime)
                .ToList();

            if (enemiesInSector.Count == 0)
                return false;

            var enemyToDestroy = enemiesInSector.First();

            enemyToDestroy.Element.BeginAnimation(Canvas.LeftProperty, null);
            enemyToDestroy.Element.BeginAnimation(Canvas.TopProperty, null);

            PlayDestructionAnimation(enemyToDestroy.Element);
            enemies.Remove(enemyToDestroy);

            return true;
        }

        private void PlayDestructionAnimation(UIElement enemy)
        {
            var scaleTransform = new ScaleTransform(1, 1);
            enemy.RenderTransform = scaleTransform;
            enemy.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnim = new DoubleAnimation(1, 2, TimeSpan.FromMilliseconds(200));
            var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));

            opacityAnim.Completed += (s, e) =>
            {
                EnemyCanvas.Children.Remove(enemy);
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            enemy.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
        }

        // ============================================================
        // EFEKTY WIZUALNE
        // ============================================================
        private void OnEnemyDestroyed(char letter, int sector)
        {
            FlashSlots(Brushes.LimeGreen);
            AddScore(100);
        }

        private void OnNoEnemyInSector(char letter, int sector)
        {
            FlashSlots(Brushes.Cyan);
        }

        private void OnWrongLetter(char letter)
        {
            FlashSlots(Brushes.Orange);
        }

        private void OnInvalidCode()
        {
            FlashSlots(Brushes.Red);
        }

        private void FlashSlots(Brush color)
        {
            foreach (var slot in morseSlots)
            {
                slot.Foreground = color;
            }

            var resetTimer = new DispatcherTimer();
            resetTimer.Interval = TimeSpan.FromMilliseconds(300);
            resetTimer.Tick += (s, e) =>
            {
                resetTimer.Stop();
                UpdateMorseDisplay();
            };
            resetTimer.Start();
        }

        // ============================================================
        // NOWA TURA
        // ============================================================
        private void StartNewTurn()
        {
            currentLetters = GenerateRandomLetters();
            ShowAllSectorLetters(currentLetters);

            if (showHints)
            {
                ShowMorsePanel(currentLetters);
            }
        }

        private void ShowMorsePanel(string letters)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in letters)
            {
                sb.AppendLine($"{c}   {morse[c]}");
            }
            MorseDisplay.Text = sb.ToString();
        }

        private string GenerateRandomLetters()
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            HashSet<char> selected = new HashSet<char>();

            while (selected.Count < 6)
            {
                char c = alphabet[rng.Next(alphabet.Length)];
                selected.Add(c);
            }

            return new string(new List<char>(selected).ToArray());
        }

        private void ShowAllSectorLetters(string letters)
        {
            LetterCanvas.Children.Clear();
            if (letters.Length < 6) return;

            double radius = 220;

            for (int i = 0; i < 6; i++)
            {
                double angleRad = sectorAngles[i] * Math.PI / 180.0;

                double x = centerX + Math.Cos(angleRad) * radius;
                double y = centerY + Math.Sin(angleRad) * radius;

                TextBlock tb = new TextBlock
                {
                    Text = letters[i].ToString(),
                    FontFamily = new FontFamily("OCR A Extended"),
                    FontSize = 48,
                    Foreground = lineColor
                };

                Canvas.SetLeft(tb, x - 20);
                Canvas.SetTop(tb, y - 20);

                LetterCanvas.Children.Add(tb);
            }
        }

        // ============================================================
        // PRZECIWNIK
        // ============================================================
        private void SpawnEnemy()
        {
            if (isGameOver) return;

            enemyCount++;

            if (enemyCount % 10 == 0)
            {
                wave++;
                UpdateWaveDisplay();
                StartNewTurn();
            }

            double enemySize = 30;
            double spawnRadius = 160;

            double angleRad = GetSafeAngle();
            double angleDegrees = angleRad * 180.0 / Math.PI;

            double startX = centerX + Math.Cos(angleRad) * spawnRadius;
            double startY = centerY + Math.Sin(angleRad) * spawnRadius;

            UIElement enemy = CreateEnemy(startX, startY);
            EnemyCanvas.Children.Add(enemy);

            int sector = GetSectorFromAngle(angleDegrees);

            var enemyData = new EnemyData
            {
                Element = enemy,
                AngleDegrees = angleDegrees,
                Sector = sector,
                SpawnTime = DateTime.Now
            };

            enemies.Add(enemyData);

            var animX = new DoubleAnimation(
                startX - enemySize / 2,
                centerX - enemySize / 2,
                TimeSpan.FromSeconds(enemyMoveDuration));

            var animY = new DoubleAnimation(
                startY - enemySize / 2,
                centerY - enemySize / 2,
                TimeSpan.FromSeconds(enemyMoveDuration));

            animX.FillBehavior = FillBehavior.Stop;
            animY.FillBehavior = FillBehavior.Stop;

            enemyData.AnimX = animX;
            enemyData.AnimY = animY;

            animY.Completed += (s, e) =>
            {
                if (enemies.Contains(enemyData))
                {
                    EnemyCanvas.Children.Remove(enemy);
                    enemies.Remove(enemyData);
                    LoseLife();
                }
            };

            enemy.BeginAnimation(Canvas.LeftProperty, animX);
            enemy.BeginAnimation(Canvas.TopProperty, animY);
        }

        private double GetSafeAngle()
        {
            double[] forbiddenAngles = { 90, 270, 30, 150, 210, 330 };
            double margin = 10;

            List<(double start, double end)> forbiddenRanges = new List<(double, double)>();

            foreach (double angle in forbiddenAngles)
            {
                double start = (angle - margin + 360) % 360;
                double end = (angle + margin + 360) % 360;
                forbiddenRanges.Add((start, end));
            }

            while (true)
            {
                double angleDegrees = rng.NextDouble() * 360;
                bool isSafe = true;

                foreach (var range in forbiddenRanges)
                {
                    if (range.start <= range.end)
                    {
                        if (angleDegrees >= range.start && angleDegrees <= range.end)
                        {
                            isSafe = false;
                            break;
                        }
                    }
                    else
                    {
                        if (angleDegrees >= range.start || angleDegrees <= range.end)
                        {
                            isSafe = false;
                            break;
                        }
                    }
                }

                if (isSafe)
                    return angleDegrees * Math.PI / 180.0;
            }
        }

        private UIElement CreateEnemy(double x, double y)
        {
            var container = new Grid
            {
                Width = 30,
                Height = 30,
                IsHitTestVisible = false
            };

            var filled = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = (Brush)new BrushConverter().ConvertFrom("#12b491"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var ring = new Ellipse
            {
                Width = 30,
                Height = 30,
                Stroke = (Brush)new BrushConverter().ConvertFrom("#029273"),
                StrokeThickness = 4,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            container.Children.Add(ring);
            container.Children.Add(filled);

            Canvas.SetLeft(container, x - 15);
            Canvas.SetTop(container, y - 15);

            return container;
        }

        private void Main_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.NavigateTo(new StartView(mainWindow));
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.NavigateTo(new MenuView(mainWindow));
        }

    }

    // ============================================================
    // KLASA DO PRZECHOWYWANIA WYNIKU
    // ============================================================
    public class ScoreEntry
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int Wave { get; set; }
        public string Difficulty { get; set; }
        public string Date { get; set; }
    }
}