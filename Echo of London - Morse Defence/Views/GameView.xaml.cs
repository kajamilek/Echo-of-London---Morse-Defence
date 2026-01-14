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
        private MainWindow oknoGlowne;
        private Random losowy = new Random();
        private DispatcherTimer timerWrogow;
        private DispatcherTimer timerWejscia;

        private double srodekX, srodekY;
        private double[] katySektor = { 0, 60, 120, 180, 240, 300 };

        private string aktualnyMorse = "";
        private string aktualneListery = "";
        private int maxDlugoscMorse = 6;
        private TextBlock[] polaMorse;

        private List<DaneWroga> wrogowie = new List<DaneWroga>();

        private string poziomTrudnosci;
        private double interwalSpawnu;
        private double czasRuchuWroga;
        private double opoznienieWejscia;
        private bool pokazPodpowiedzi;
        private bool pokazPolskie;

        //PARAMETRY GRACZA
        private int zycia = 5;
        private int odblokowaneZycia = 5;
        private int maxZycia = 10;

        private int punkty = 0;
        private int fala = 1;

        //PARAMETRY FALI
        private int wrogowieNaFale = 5;
        private int faleDoUlepszenia = 1;

        private bool zablokowanySpawn = false;

        private double mnoznikPunktow = 1;
        private double modyfikatorPredkosci = 1;
        private double modyfikatorSpawnu = 1;

        private bool koniecGry = false;
        private bool wynikZapisany = false;

        private int stworzeniWrogowie = 0;

        //DZWIĘKI
        private MediaPlayer dzwiekZabicia;
        private MediaPlayer dzwiekObrazen;

        //DO ONE BUTTON MODE
        private bool trwaNadawanie = false;

        private DateTime czasStartuNadawania;
        private int progKrotkieMs = 150;

        private Brush kolorLinii = (Brush)new BrushConverter().ConvertFrom("#029273");

        private static string sciezkaWynikow = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EchoOfLondon", "highscores.txt");

        private Dictionary<char, string> koderMorse = new Dictionary<char, string>()
        {
            {'A',"•–"}, {'B',"–•••"}, {'C',"–•–•"}, {'D',"–••"},
            {'E',"•"}, {'F',"••–•"}, {'G',"––•"}, {'H',"••••"},
            {'I',"••"}, {'J',"•–––"}, {'K',"–•–"}, {'L',"•–••"},
            {'M',"––"}, {'N',"–•"}, {'O',"–––"}, {'P',"•––•"},
            {'Q',"––•–"}, {'R',"•–•"}, {'S',"•••"}, {'T',"–"},
            {'U',"••–"}, {'V',"•••–"}, {'W',"•––"}, {'X',"–••–"},
            {'Y',"–•––"}, {'Z',"––••"},

            // Polskie litery
            {'Ą',"•–––"}, {'Ć',"–•–••"}, {'Ę',"••–••"}, {'Ł',"•–••–"},
            {'Ń',"––•––"}, {'Ó',"–––•"}, {'Ś',"•••–•••"}, {'Ź',"––••–"},
            {'Ż',"––••–•"}
        };

        private class DaneWroga
        {
            public UIElement Element { get; set; }
            public double Kat { get; set; }
            public int Sektor { get; set; }
            public DateTime CzasSpawnu { get; set; }
            public DoubleAnimation AnimX { get; set; }
            public DoubleAnimation AnimY { get; set; }
        }

        public GameView(MainWindow mw, string trudnosc, bool podpowiedzi = false, bool polski = false)
        {
            InitializeComponent();
            oknoGlowne = mw;
            poziomTrudnosci = trudnosc;
            pokazPodpowiedzi = podpowiedzi;
            pokazPolskie = polski;
            UstawParametryTrudnosci();
            Loaded += NaZaladowaniu;
        }

        private void UstawParametryTrudnosci()
        {
            string t = poziomTrudnosci.ToLower();

            if (t == "easy")
            {
                interwalSpawnu = 0.50;
                czasRuchuWroga = 20.0;
                opoznienieWejscia = 3.0;
            }
            else if (t == "hard")
            {
                interwalSpawnu = 1.2;
                czasRuchuWroga = 2.0;
                opoznienieWejscia = 3.0;
            }
            else
            {
                interwalSpawnu = 2.0;
                czasRuchuWroga = 3.0;
                opoznienieWejscia = 3.0;
            }
        }

        private void NaZaladowaniu(object sender, RoutedEventArgs e)
        {
            srodekX = EnemyCanvas.ActualWidth / 2;
            srodekY = EnemyCanvas.ActualHeight / 2;

            polaMorse = new TextBlock[] { MorseSlot0, MorseSlot1, MorseSlot2, MorseSlot3, MorseSlot4, MorseSlot5 };

            HintsPanel.Visibility = pokazPodpowiedzi ? Visibility.Visible : Visibility.Collapsed;

            OdswiezZycia();
            OdswiezPunkty();
            OdswiezFale();
            AktualizujStatystykiNaOverlay();

            timerWejscia = new DispatcherTimer();
            timerWejscia.Interval = TimeSpan.FromSeconds(opoznienieWejscia);
            timerWejscia.Tick += (s, ev) => { timerWejscia.Stop(); WyslijKodMorse(); };

            Focusable = true;
            Focus();
            KeyDown += ObslugaKlawiatury;
            KeyUp += ObslugaKlawiaturaUp;
            MouseDown += (s, ev) => Focus();

            timerWrogow = new DispatcherTimer();
            timerWrogow.Interval = TimeSpan.FromSeconds(interwalSpawnu);
            timerWrogow.Tick += TimerWrogow_Tick;
            timerWrogow.Start();

            NowaTura();
        }

        private void TimerWrogow_Tick(object sender, EventArgs e)
        {
            StworzWroga();
        }

        private void OdswiezZycia()
        {
            LivesText.Text = zycia.ToString();

            if (zycia == 1)
            {
                LivesText.Foreground = Brushes.Red;
            }
            else if (zycia == 2)
            {
                LivesText.Foreground = Brushes.Orange;
            }
            else if (zycia == 3)
            {
                LivesText.Foreground = Brushes.Yellow;
            }
            else
            {
                LivesText.Foreground = (Brush)new BrushConverter().ConvertFrom("#12b491");
            }
        }

        private void OdswiezPunkty()
        { ScoreText.Text = punkty.ToString(); }

        private void OdswiezFale()
        { WaveText.Text = fala.ToString(); }

        private void StracZycie()
        {
            if (koniecGry) return;
            zycia--;
            OdswiezZycia();
            MignijGracza();
            if (zycia <= 0) ZakonczGre();
        }

        private void DodajPunkty(int ile)
        {
            punkty += ile;
            OdswiezPunkty();
        }

        private void MignijGracza()
        {
            StartDzwiekObrazen();
            var oryginalny = player.Fill;
            player.Fill = Brushes.White;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            timer.Tick += (s, e) => { timer.Stop(); player.Fill = oryginalny; };
            timer.Start();
        }

        public void ZakonczGre()
        {
            koniecGry = true;
            wynikZapisany = false;
            timerWrogow?.Stop();
            timerWejscia?.Stop();

            foreach (var w in wrogowie.ToList())
            {
                w.Element.BeginAnimation(Canvas.LeftProperty, null);
                w.Element.BeginAnimation(Canvas.TopProperty, null);
                EnemyCanvas.Children.Remove(w.Element);
            }
            wrogowie.Clear();

            FinalScoreText.Text = "SCORE: " + punkty;
            FinalWaveText.Text = "WAVE: " + fala;
            SaveConfirmText.Text = "";
            PlayerNameTextBox.Text = "PLAYER";
            GameOverOverlay.Visibility = Visibility.Visible;
            PlayerNameTextBox.Focus();
            PlayerNameTextBox.SelectAll();
        }

        private void SaveScore_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            if (wynikZapisany)
            {
                SaveConfirmText.Text = "ALREADY SAVED!";
                SaveConfirmText.Foreground = Brushes.Orange;
                return;
            }

            string nazwa = PlayerNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(nazwa)) nazwa = "PLAYER";
            nazwa = new string(nazwa.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray());
            if (nazwa.Length > 15) nazwa = nazwa.Substring(0, 15);

            try
            {
                ZapiszWynikDoPliku(nazwa, punkty, fala, poziomTrudnosci);
                wynikZapisany = true;
                SaveConfirmText.Text = "SCORE SAVED!";
                SaveConfirmText.Foreground = (Brush)new BrushConverter().ConvertFrom("#12b491");
            }
            catch
            {
                SaveConfirmText.Text = "SAVE FAILED!";
                SaveConfirmText.Foreground = Brushes.Red;
            }
        }

        private void ZapiszWynikDoPliku(string nazwa, int pkt, int f, string trudnosc)
        {
            string folder = System.IO.Path.GetDirectoryName(sciezkaWynikow);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string hintsStr = pokazPodpowiedzi ? "1" : "0";
            string wpis = nazwa + "|" + pkt + "|" + f + "|" + trudnosc.ToUpper() + "|" +
                          DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "|" + hintsStr;

            using (StreamWriter sw = File.AppendText(sciezkaWynikow))
                sw.WriteLine(wpis);
        }

        public static List<ScoreEntry> LoadScores()
        {
            var lista = new List<ScoreEntry>();
            if (!File.Exists(sciezkaWynikow)) return lista;

            try
            {
                foreach (string linia in File.ReadAllLines(sciezkaWynikow))
                {
                    string[] czesci = linia.Split('|');
                    if (czesci.Length >= 5)
                    {
                        var wpis = new ScoreEntry
                        {
                            PlayerName = czesci[0],
                            Score = int.Parse(czesci[1]),
                            Wave = int.Parse(czesci[2]),
                            Difficulty = czesci[3],
                            Date = czesci[4],
                            Hints = false
                        };

                        if (czesci.Length >= 6)
                            wpis.Hints = czesci[5] == "1";

                        lista.Add(wpis);
                    }
                }
            }
            catch { }

            return lista.OrderByDescending(x => x.Score).ToList();
        }

        private void ObslugaKlawiatury(object sender, KeyEventArgs e)
        {
            if (koniecGry) return;

            if (GameSettings.TrybJednegoPrzycisku)
            {
                if (e.Key == Key.Space && !trwaNadawanie)
                {
                    trwaNadawanie = true;
                    czasStartuNadawania = DateTime.Now;
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                if (e.Key == Key.Left) { DodajSymbolMorse("–"); e.Handled = true; return; }
                if (e.Key == Key.Right) { DodajSymbolMorse("•"); e.Handled = true; return; }
            }

            if (e.Key == Key.Enter) { WyslijKodMorse(); e.Handled = true; }
            else if (e.Key == Key.Back || e.Key == Key.Delete) { UsunOstatniSymbol(); e.Handled = true; }
            else if (e.Key == Key.Escape) { WyczyscMorse(); e.Handled = true; }
        }

        private void ObslugaKlawiaturaUp(object sender, KeyEventArgs e)
        {
            if (koniecGry) return;

            if (GameSettings.TrybJednegoPrzycisku && e.Key == Key.Space && trwaNadawanie)
            {
                trwaNadawanie = false;

                double czasTrzymania = (DateTime.Now - czasStartuNadawania).TotalMilliseconds;

                if (czasTrzymania < progKrotkieMs)
                    DodajSymbolMorse("•");
                else
                    DodajSymbolMorse("–");

                e.Handled = true;
            }
        }

        private void DodajSymbolMorse(string symbol)
        {
            if (aktualnyMorse.Length >= maxDlugoscMorse) return;
            aktualnyMorse += symbol;
            OdswiezWyswietlaczMorse();
            timerWejscia.Stop();
            timerWejscia.Start();
            SoundHelper.PlayClick();
        }

        private void UsunOstatniSymbol()
        {
            if (aktualnyMorse.Length > 0)
            {
                aktualnyMorse = aktualnyMorse.Substring(0, aktualnyMorse.Length - 1);
                OdswiezWyswietlaczMorse();
            }
            timerWejscia.Stop();
            if (aktualnyMorse.Length > 0) timerWejscia.Start();
        }

        private void WyczyscMorse()
        {
            aktualnyMorse = "";
            OdswiezWyswietlaczMorse();
            timerWejscia.Stop();
        }

        private void OdswiezWyswietlaczMorse()
        {
            int start = maxDlugoscMorse - aktualnyMorse.Length;
            for (int i = 0; i < polaMorse.Length; i++)
            {
                if (i >= start)
                {
                    polaMorse[i].Text = aktualnyMorse[i - start].ToString();
                    polaMorse[i].Foreground = kolorLinii;
                }
                else
                {
                    polaMorse[i].Text = "";
                    polaMorse[i].Foreground = kolorLinii;
                }
            }
        }

        private void WyslijKodMorse()
        {
            timerWejscia.Stop();
            if (string.IsNullOrEmpty(aktualnyMorse)) return;

            char? znalezionaLitera = null;
            foreach (var para in koderMorse)
            {
                if (para.Value == aktualnyMorse)
                {
                    znalezionaLitera = para.Key;
                    break;
                }
            }

            if (znalezionaLitera.HasValue)
            {
                int indeksSektor = aktualneListery.IndexOf(znalezionaLitera.Value);
                if (indeksSektor >= 0)
                {
                    bool zniszczony = ZniszczWrogaWSektor(indeksSektor);
                    if (zniszczony)
                    {
                        DodajPunkty((int)(100 * mnoznikPunktow));
                    }
                }
            }

            WyczyscMorse();
        }

        private int ObliczSektor(double kat)
        {
            kat = ((kat % 360) + 360) % 360;
            double przesuniete = (kat + 30) % 360;
            return (int)(przesuniete / 60);
        }

        private bool ZniszczWrogaWSektor(int sektor)
        {
            var wrogiWSektor = wrogowie.Where(w => w.Sektor == sektor).OrderBy(w => w.CzasSpawnu).ToList();
            if (wrogiWSektor.Count == 0) return false;

            var wrogDoZniszczenia = wrogiWSektor.First();

            EnemyCanvas.Children.Remove(wrogDoZniszczenia.Element);
            wrogowie.Remove(wrogDoZniszczenia);
            StartDzwiekZabicia();
            WrogZniknal();
            return true;
        }

        private void NowaTura()
        {
            aktualneListery = GenerujLosoweListery();
            PokazListeryNaSektor(aktualneListery);
            if (pokazPodpowiedzi) PokazPanelPodpowiedzi(aktualneListery);

            BreakOverlay.Visibility = Visibility.Visible;
            zablokowanySpawn = true;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.0) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                BreakOverlay.Visibility = Visibility.Collapsed;
                zablokowanySpawn = false;
            };
            timer.Start();
        }

        private void PokazPanelPodpowiedzi(string litery)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in litery) sb.AppendLine(c + "   " + koderMorse[c]);
            MorseDisplay.Text = sb.ToString();
        }

        private string GenerujLosoweListery()
        {
            string alfabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string polskieLitery = "ĄĆĘŁŃÓŚŹŻ";
            if (pokazPolskie) alfabet += polskieLitery;
            HashSet<char> wybrane = new HashSet<char>();
            while (wybrane.Count < 6)
            {
                char c = alfabet[losowy.Next(alfabet.Length)];
                wybrane.Add(c);
            }
            return new string(wybrane.ToArray());
        }

        private void PokazListeryNaSektor(string litery)
        {
            LetterCanvas.Children.Clear();
            if (litery.Length < 6) return;

            double promien = 220;

            for (int i = 0; i < 6; i++)
            {
                double katRad = katySektor[i] * Math.PI / 180.0;
                double x = srodekX + Math.Cos(katRad) * promien;
                double y = srodekY + Math.Sin(katRad) * promien;

                TextBlock tb = new TextBlock
                {
                    Text = litery[i].ToString(),
                    FontFamily = new FontFamily("OCR A Extended"),
                    FontSize = 48,
                    Foreground = kolorLinii
                };

                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double szerokoscLitery = tb.DesiredSize.Width;
                double wysokoscLitery = tb.DesiredSize.Height;

                Canvas.SetLeft(tb, x - szerokoscLitery / 2);
                Canvas.SetTop(tb, y - wysokoscLitery / 2);

                LetterCanvas.Children.Add(tb);
            }
        }

        private void StworzWroga()
        {
            if (koniecGry) return;
            if (zablokowanySpawn) return;
            if (stworzeniWrogowie >= wrogowieNaFale)
            {
                zablokowanySpawn = true;
                return;
            }
            stworzeniWrogowie++;

            double rozmiar = 30;
            double promienSpawnu = 160;

            double katRad = PobierzBezpiecznyKat();
            double katStopnie = katRad * 180.0 / Math.PI;

            double startX = srodekX + Math.Cos(katRad) * promienSpawnu;
            double startY = srodekY + Math.Sin(katRad) * promienSpawnu;

            UIElement wrog = UtworzWrogaUI(startX, startY);
            EnemyCanvas.Children.Add(wrog);

            int sektor = ObliczSektor(katStopnie);

            var dane = new DaneWroga
            {
                Element = wrog,
                Kat = katStopnie,
                Sektor = sektor,
                CzasSpawnu = DateTime.Now
            };
            wrogowie.Add(dane);

            var animX = new DoubleAnimation(startX - rozmiar / 2, srodekX - rozmiar / 2, TimeSpan.FromSeconds(czasRuchuWroga));
            var animY = new DoubleAnimation(startY - rozmiar / 2, srodekY - rozmiar / 2, TimeSpan.FromSeconds(czasRuchuWroga));
            animX.FillBehavior = FillBehavior.Stop;
            animY.FillBehavior = FillBehavior.Stop;

            dane.AnimX = animX;
            dane.AnimY = animY;

            animY.Completed += (s, e) =>
            {
                if (wrogowie.Contains(dane))
                {
                    EnemyCanvas.Children.Remove(wrog);
                    wrogowie.Remove(dane);
                    StracZycie();
                    WrogZniknal();
                }
            };

            wrog.BeginAnimation(Canvas.LeftProperty, animX);
            wrog.BeginAnimation(Canvas.TopProperty, animY);
        }

        private void WrogZniknal()
        {
            if (zablokowanySpawn && wrogowie.Count == 0)
            {
                ZakonczFale();
            }
        }

        private void ZakonczFale()
        {
            zablokowanySpawn = true;
            if (fala % faleDoUlepszenia == 0)
            {
                PokazUpgradeOverlay();
            }
            else
            {
                PrzejdzDoNastepnejFali();
            }
        }

        private void AktualizujStatystykiNaOverlay()
        {
            SpeedStat.Text = $"ENEMY SPEED: {(modyfikatorPredkosci * 100):0}%";
            SpawnStat.Text = $"SPAWN RATE: {(modyfikatorSpawnu * 100):0}%";
            PointsStat.Text = $"POINTS MULTI: x{mnoznikPunktow:0.00}";
        }

        private void PrzejdzDoNastepnejFali()
        {
            fala++;
            OdswiezFale();

            modyfikatorPredkosci *= 1.05;
            modyfikatorSpawnu *= 1.05;

            stworzeniWrogowie = 0;
            zablokowanySpawn = false;

            AktualizujStatystykiNaOverlay();

            timerWrogow.Stop();
            timerWrogow.Interval = TimeSpan.FromSeconds(interwalSpawnu * modyfikatorSpawnu);
            timerWrogow.Start();

            NowaTura();
        }

        private double PobierzBezpiecznyKat()
        {
            double[] zakazane = { 90, 270, 30, 150, 210, 330 };
            double margines = 10;

            var zakresy = new List<(double start, double end)>();
            foreach (double kat in zakazane)
            {
                double start = (kat - margines + 360) % 360;
                double end = (kat + margines + 360) % 360;
                zakresy.Add((start, end));
            }

            while (true)
            {
                double kat = losowy.NextDouble() * 360;
                bool bezpieczny = true;

                foreach (var zakres in zakresy)
                {
                    if (zakres.start <= zakres.end)
                    {
                        if (kat >= zakres.start && kat <= zakres.end) { bezpieczny = false; break; }
                    }
                    else
                    {
                        if (kat >= zakres.start || kat <= zakres.end) { bezpieczny = false; break; }
                    }
                }

                if (bezpieczny) return kat * Math.PI / 180.0;
            }
        }

        private UIElement UtworzWrogaUI(double x, double y)
        {
            var kontener = new Grid { Width = 30, Height = 30, IsHitTestVisible = false };

            var wypelnienie = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = (Brush)new BrushConverter().ConvertFrom("#12b491"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var obwodka = new Ellipse
            {
                Width = 30,
                Height = 30,
                Stroke = (Brush)new BrushConverter().ConvertFrom("#029273"),
                StrokeThickness = 4,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            kontener.Children.Add(obwodka);
            kontener.Children.Add(wypelnienie);
            Canvas.SetLeft(kontener, x - 15);
            Canvas.SetTop(kontener, y - 15);
            return kontener;
        }

        // ULEPSZENIA

        private void PokazUpgradeOverlay()
        {
            LosujUlepszenia();
            AktualizujStatystykiNaOverlay();

            UstawPrzyciskUlepszenia(UpgradeBtn1, wylosowaneUlepszenia[0]);
            UstawPrzyciskUlepszenia(UpgradeBtn2, wylosowaneUlepszenia[1]);
            UstawPrzyciskUlepszenia(UpgradeBtn3, wylosowaneUlepszenia[2]);

            UpgradeOverlay.Visibility = Visibility.Visible;
        }

        private enum TypUlepszenia
        {
            SlowerEnemies,
            SlowerSpawn,
            MorePoints,
            MoreLife
        }

        private List<TypUlepszenia> dostepneUlepszenia = new List<TypUlepszenia>
        {
            TypUlepszenia.SlowerEnemies,
            TypUlepszenia.SlowerSpawn,
            TypUlepszenia.MorePoints,
            TypUlepszenia.MoreLife
        };

        private List<TypUlepszenia> wylosowaneUlepszenia;

        private void LosujUlepszenia()
        {
            var dozwolone = dostepneUlepszenia.ToList();

            if (zycia >= maxZycia)
            {
                dozwolone.Remove(TypUlepszenia.MoreLife);
            }

            wylosowaneUlepszenia = dozwolone
                .OrderBy(x => losowy.Next())
                .Take(3)
                .ToList();
        }

        private void UstawPrzyciskUlepszenia(Button btn, TypUlepszenia typ)
        {
            btn.Click -= Upgrade_SlowerEnemies;
            btn.Click -= Upgrade_SlowerSpawn;
            btn.Click -= Upgrade_MorePoints;
            btn.Click -= Upgrade_MoreLife;

            TextBlock tekst = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };

            switch (typ)
            {
                case TypUlepszenia.SlowerEnemies:
                    tekst.Text = "SLOWER ENEMIES";
                    btn.Click += Upgrade_SlowerEnemies;
                    break;

                case TypUlepszenia.SlowerSpawn:
                    tekst.Text = "SLOWER SPAWN";
                    btn.Click += Upgrade_SlowerSpawn;
                    break;

                case TypUlepszenia.MorePoints:
                    tekst.Text = "MORE POINTS";
                    btn.Click += Upgrade_MorePoints;
                    break;

                case TypUlepszenia.MoreLife:
                    tekst.Text = "MORE LIFE";
                    btn.Click += Upgrade_MoreLife;
                    break;
            }

            btn.Content = tekst;
        }

        private void ZastosujUlepszenieIZamknij()
        {
            UpgradeOverlay.Visibility = Visibility.Collapsed;
            zablokowanySpawn = false;
            wrogowieNaFale += 1;
            zycia = odblokowaneZycia;
            OdswiezZycia();
            PrzejdzDoNastepnejFali();
        }

        // DEFINICJE ULEPSZEŃ
        private void Upgrade_SlowerEnemies(object sender, RoutedEventArgs e)
        {
            modyfikatorPredkosci *= 0.9;
            ZastosujUlepszenieIZamknij();
        }

        private void Upgrade_SlowerSpawn(object sender, RoutedEventArgs e)
        {
            modyfikatorSpawnu *= 0.9;
            ZastosujUlepszenieIZamknij();
        }

        private void Upgrade_MorePoints(object sender, RoutedEventArgs e)
        {
            mnoznikPunktow += 0.25;
            ZastosujUlepszenieIZamknij();
        }

        private void Upgrade_MoreLife(object sender, RoutedEventArgs e)
        {
            zycia += 1;
            OdswiezZycia();
            odblokowaneZycia++;
            ZastosujUlepszenieIZamknij();
        }


        // DŹWIĘKI
        private void GrajDzwiek(string sciezka, double glosnosc)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(sciezka, UriKind.Relative));
            player.Volume = glosnosc;
            player.Play();
        }

        private void StartDzwiekZabicia() => GrajDzwiek("Assets/Sounds/LifeDroplet.mp3", 0.7);

        private void StartDzwiekObrazen() => GrajDzwiek("Assets/Sounds/Dead.mp3", 0.05);        
        
        private void Main_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            oknoGlowne.NavigateTo(new StartView(oknoGlowne));
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            timerWrogow.Stop();
            timerWejscia.Stop();
            oknoGlowne.NavigateTo(new MenuView(oknoGlowne, poziomTrudnosci, pokazPodpowiedzi));
        }
    }

    public class ScoreEntry
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int Wave { get; set; }
        public string Difficulty { get; set; }
        public string Date { get; set; }
        public bool Hints { get; set; }
    }
}