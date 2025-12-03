using System;
using System.Collections.Generic;
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
        private const double INPUT_DELAY_SECONDS = 0.8;

        // === Śledzenie przeciwników w sektorach ===
        private List<EnemyData> enemies = new List<EnemyData>();

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

        public GameView(MainWindow main, string difficulty)
        {
            InitializeComponent();
            this.main = main;

            Loaded += GameView_Loaded;
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            centerX = EnemyCanvas.ActualWidth / 2;
            centerY = EnemyCanvas.ActualHeight / 2;

            // Inicjalizacja slotów Morse'a
            morseSlots = new TextBlock[]
            {
                MorseSlot0, MorseSlot1, MorseSlot2,
                MorseSlot3, MorseSlot4, MorseSlot5
            };

            // Timer do automatycznego zatwierdzania
            inputTimer = new DispatcherTimer();
            inputTimer.Interval = TimeSpan.FromSeconds(INPUT_DELAY_SECONDS);
            inputTimer.Tick += InputTimer_Tick;

            // Obsługa klawiatury
            this.Focusable = true;
            this.Focus();
            this.KeyDown += GameView_KeyDown;
            this.MouseDown += (s, ev) => this.Focus();

            enemyTimer = new DispatcherTimer();
            enemyTimer.Interval = TimeSpan.FromSeconds(2);
            enemyTimer.Tick += (s, ev) => SpawnEnemy();
            enemyTimer.Start();

            StartNewTurn();
        }

        // ============================================================
        // OBSŁUGA KLAWIATURY
        // ============================================================
        private void GameView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    AddMorseSymbol("–");  // Kreska
                    e.Handled = true;
                    break;

                case Key.Right:
                    AddMorseSymbol("•");  // Kropka
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

        // ============================================================
        // WYŚWIETLANIE OD PRAWEJ - nowe znaki pojawiają się po prawej
        // ============================================================
        private void UpdateMorseDisplay()
        {
            // Oblicz od którego slotu zacząć wyświetlanie
            int startSlot = MAX_MORSE_LENGTH - currentMorseInput.Length;

            for (int i = 0; i < morseSlots.Length; i++)
            {
                if (i >= startSlot)
                {
                    // Ten slot ma znak
                    int inputIndex = i - startSlot;
                    morseSlots[i].Text = currentMorseInput[inputIndex].ToString();
                    morseSlots[i].Foreground = Brushes.Yellow;
                }
                else
                {
                    // Pusty slot
                    morseSlots[i].Text = "";
                    morseSlots[i].Foreground = (Brush)new BrushConverter().ConvertFrom("#029273");
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

            // Znajdź literę odpowiadającą wpisanemu kodowi
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
                // Znajdź indeks litery w aktualnych 6 literach (to jest numer sektora)
                int sectorIndex = currentLetters.IndexOf(matchedLetter.Value);

                if (sectorIndex >= 0)
                {
                    // TRAFIENIE! Zniszcz pierwszego przeciwnika w tym sektorze
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
        // NISZCZENIE PRZECIWNIKÓW W SEKTORZE
        // ============================================================
        private int GetSectorFromAngle(double angleDegrees)
        {
            // Normalizuj kąt do 0-360
            angleDegrees = ((angleDegrees % 360) + 360) % 360;

            // Sektory są wyśrodkowane na: 0°, 60°, 120°, 180°, 240°, 300°
            // Każdy sektor ma zakres 60° (-30° do +30° od środka)
            // Przesuwamy o 30°, żeby granice były na 30°, 90°, 150°...
            double shifted = (angleDegrees + 30) % 360;
            int sector = (int)(shifted / 60);

            return sector;
        }

        private bool DestroyEnemyInSector(int sector)
        {
            // Znajdź wszystkich przeciwników w danym sektorze
            var enemiesInSector = enemies
                .Where(e => e.Sector == sector)
                .OrderBy(e => e.SpawnTime)  // Najstarszy pierwszy (najbliżej środka)
                .ToList();

            if (enemiesInSector.Count == 0)
                return false;

            // Zniszcz pierwszego (najstarszego/najbliższego)
            var enemyToDestroy = enemiesInSector.First();

            // Zatrzymaj animacje
            enemyToDestroy.Element.BeginAnimation(Canvas.LeftProperty, null);
            enemyToDestroy.Element.BeginAnimation(Canvas.TopProperty, null);

            // Animacja zniszczenia
            PlayDestructionAnimation(enemyToDestroy.Element);

            // Usuń z listy
            enemies.Remove(enemyToDestroy);

            return true;
        }

        private void PlayDestructionAnimation(UIElement enemy)
        {
            // Animacja skalowania i zanikania
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
            System.Diagnostics.Debug.WriteLine($"ZNISZCZONO! Litera: {letter}, Sektor: {sector}");
            // TODO: Dodaj punkty
        }

        private void OnNoEnemyInSector(char letter, int sector)
        {
            FlashSlots(Brushes.Cyan);
            System.Diagnostics.Debug.WriteLine($"Brak przeciwnika w sektorze {sector} (litera {letter})");
        }

        private void OnWrongLetter(char letter)
        {
            FlashSlots(Brushes.Orange);
            System.Diagnostics.Debug.WriteLine($"Zła litera: {letter} (nie ma jej w tej turze)");
        }

        private void OnInvalidCode()
        {
            FlashSlots(Brushes.Red);
            System.Diagnostics.Debug.WriteLine("Nieprawidłowy kod Morse'a!");
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
            ShowMorsePanel(currentLetters);
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
                    Foreground = Brushes.Yellow
                };

                Canvas.SetLeft(tb, x - 20);
                Canvas.SetTop(tb, y - 20);

                LetterCanvas.Children.Add(tb);
            }
        }

        // ============================================================
        // PRZECIWNIK - ze śledzeniem sektora
        // ============================================================
        private void SpawnEnemy()
        {
            enemyCount++;

            if (enemyCount % 10 == 0)
                StartNewTurn();

            double enemySize = 30;
            double spawnRadius = 160;

            double angleRad = GetSafeAngle();
            double angleDegrees = angleRad * 180.0 / Math.PI;

            double startX = centerX + Math.Cos(angleRad) * spawnRadius;
            double startY = centerY + Math.Sin(angleRad) * spawnRadius;

            UIElement enemy = CreateEnemy(startX, startY);
            EnemyCanvas.Children.Add(enemy);

            // Oblicz sektor
            int sector = GetSectorFromAngle(angleDegrees);

            // Utwórz dane przeciwnika
            var enemyData = new EnemyData
            {
                Element = enemy,
                AngleDegrees = angleDegrees,
                Sector = sector,
                SpawnTime = DateTime.Now
            };

            enemies.Add(enemyData);

            double duration = 3;

            var animX = new DoubleAnimation(startX - enemySize / 2, centerX - enemySize / 2, TimeSpan.FromSeconds(duration));
            var animY = new DoubleAnimation(startY - enemySize / 2, centerY - enemySize / 2, TimeSpan.FromSeconds(duration));

            animX.FillBehavior = FillBehavior.Stop;
            animY.FillBehavior = FillBehavior.Stop;

            enemyData.AnimX = animX;
            enemyData.AnimY = animY;

            animY.Completed += (s, e) =>
            {
                // Przeciwnik dotarł do środka - usuń go
                EnemyCanvas.Children.Remove(enemy);
                enemies.Remove(enemyData);

                // TODO: Odejmij życie graczowi
                System.Diagnostics.Debug.WriteLine($"Przeciwnik dotarł do środka! Sektor: {sector}");
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

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            enemyTimer?.Stop();
            inputTimer?.Stop();
            main.NavigateTo(new MenuView(main));
        }
    }
}