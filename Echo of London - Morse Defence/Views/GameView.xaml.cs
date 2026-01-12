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
        MainWindow oknoGlowne;
        Random losowy = new Random();
        DispatcherTimer timerWrogow;
        DispatcherTimer timerWejscia;

        double srodekX, srodekY;
        double[] katySektor = { 0, 60, 120, 180, 240, 300 };

        string aktualnyMorse = "";
        string aktualneListery = "";
        int maxDlugoscMorse = 6;
        TextBlock[] polaMorse;

        List<DaneWroga> wrogowie = new List<DaneWroga>();

        string poziomTrudnosci;
        double interwalSpawnu;
        double czasRuchuWroga;
        double opoznienieWejscia;
        bool pokazPodpowiedzi;
        bool pokazPolskie;

        //PARAMETRY GRACZA
        int zycia = 3;
        int punkty = 0;
        int fala = 1;


        //PARAMETRY FALI
        int wrogowieNaFale = 2;
        int zniknieciWrogowie = 0;
        bool spawnZablokowany = false;

        bool zablokowanieUlepszeniem = false;
        double mnoznikPunktow = 1;
        double modyfikatorPredkosci = 1;
        double modyfikatorSpawnu = 1;


        int licznikWrogow = 0;
        bool koniecGry = false;
        bool wynikZapisany = false;


        //DZWIĘKI
        private MediaPlayer dzwiekZabicia;
        private MediaPlayer dzwiekObrazen;

        //DO ONE BUTTON MODE
        bool trwaNadawanie = false;
        DateTime czasStartuNadawania;
        DispatcherTimer timerNadawania;
        int progKrotkieMs = 150; 
        



        Brush kolorLinii = (Brush)new BrushConverter().ConvertFrom("#029273");

        static string sciezkaWynikow = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EchoOfLondon", "highscores.txt");

        Dictionary<char, string> koderMorse = new Dictionary<char, string>()
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

        class DaneWroga
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

        void UstawParametryTrudnosci()
        {
            string t = poziomTrudnosci.ToLower();

            if (t == "easy")
            {
                interwalSpawnu = 3.0;
                czasRuchuWroga = 10.0;
                opoznienieWejscia = 1.0;
            }
            else if (t == "hard")
            {
                interwalSpawnu = 1.2;
                czasRuchuWroga = 2.0;
                opoznienieWejscia = 0.6;
            }
            else
            {
                interwalSpawnu = 2.0;
                czasRuchuWroga = 3.0;
                opoznienieWejscia = 0.8;
            }
        }

        void NaZaladowaniu(object sender, RoutedEventArgs e)
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

            timerNadawania = new DispatcherTimer();
            timerNadawania.Interval = TimeSpan.FromMilliseconds(50);
            timerNadawania.Tick += TimerNadawania_Tick;

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

        void TimerWrogow_Tick(object sender, EventArgs e)
        {
            StworzWroga();
        }

        void TimerNadawania_Tick(object sender, EventArgs e)
        {
        }

        public void WymusGameOver()
        {
            ZakonczGre();
        }

        void OdswiezZycia()
        {
            string pelne = new string('●', Math.Max(0, zycia));
            LivesText.Text = pelne ;
            LivesText.Foreground = Brushes.White;
        }

        void OdswiezPunkty() { ScoreText.Text = punkty.ToString(); }
        void OdswiezFale() { WaveText.Text = fala.ToString(); }

        void StracZycie()
        {
            if (koniecGry) return;
            zycia--;
            OdswiezZycia();
            MignijGracza();
            if (zycia <= 0) ZakonczGre();
        }

        void DodajPunkty(int ile)
        {
            punkty += ile;
            OdswiezPunkty();
        }

        void MignijGracza()
        {
            StartDzwiekObrazen();
            var oryginalny = player.Fill;
            player.Fill = Brushes.White;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            timer.Tick += (s, e) => { timer.Stop(); player.Fill = oryginalny; };
            timer.Start();
        }

        void ZakonczGre()
        {
            koniecGry = true;
            wynikZapisany = false;
            timerWrogow?.Stop();
            timerWejscia?.Stop();
            timerNadawania?.Stop();

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

        void SaveScore_Click(object sender, RoutedEventArgs e)
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

        void ZapiszWynikDoPliku(string nazwa, int pkt, int f, string trudnosc)
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

        void ObslugaKlawiatury(object sender, KeyEventArgs e)
        {
            if (koniecGry) return;

            if (GameSettings.TrybJednegoPrzycisku)
            {
                if (e.Key == Key.Space && !trwaNadawanie)
                {
                    trwaNadawanie = true;
                    czasStartuNadawania = DateTime.Now;
                    timerNadawania.Start();
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

        void ObslugaKlawiaturaUp(object sender, KeyEventArgs e)
        {
            if (koniecGry) return;

            if (GameSettings.TrybJednegoPrzycisku && e.Key == Key.Space && trwaNadawanie)
            {
                trwaNadawanie = false;
                timerNadawania.Stop();

                double czasTrzymania = (DateTime.Now - czasStartuNadawania).TotalMilliseconds;

                if (czasTrzymania < progKrotkieMs)
                    DodajSymbolMorse("•");
                else
                    DodajSymbolMorse("–");

                e.Handled = true;
            }
        }

        void DodajSymbolMorse(string symbol)
        {
            if (aktualnyMorse.Length >= maxDlugoscMorse) return;
            aktualnyMorse += symbol;
            OdswiezWyswietlaczMorse();
            timerWejscia.Stop();
            timerWejscia.Start();
            SoundHelper.PlayClick();
        }

        void UsunOstatniSymbol()
        {
            if (aktualnyMorse.Length > 0)
            {
                aktualnyMorse = aktualnyMorse.Substring(0, aktualnyMorse.Length - 1);
                OdswiezWyswietlaczMorse();
            }
            timerWejscia.Stop();
            if (aktualnyMorse.Length > 0) timerWejscia.Start();
        }

        void WyczyscMorse()
        {
            aktualnyMorse = "";
            OdswiezWyswietlaczMorse();
            timerWejscia.Stop();
        }

        void OdswiezWyswietlaczMorse()
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

        void WyslijKodMorse()
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

        int ObliczSektor(double kat)
        {
            kat = ((kat % 360) + 360) % 360;
            double przesuniete = (kat + 30) % 360;
            return (int)(przesuniete / 60);
        }

        bool ZniszczWrogaWSektor(int sektor)
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

        void NowaTura()
        {
            aktualneListery = GenerujLosoweListery();
            PokazListeryNaSektor(aktualneListery);
            if (pokazPodpowiedzi) PokazPanelPodpowiedzi(aktualneListery);
        }

        void PokazPanelPodpowiedzi(string litery)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in litery) sb.AppendLine(c + "   " + koderMorse[c]);
            MorseDisplay.Text = sb.ToString();
        }

        string GenerujLosoweListery()
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

        void PokazListeryNaSektor(string litery)
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

        void StworzWroga()
        {
            if (koniecGry) return;
            if (spawnZablokowany) return;
            if (zablokowanieUlepszeniem) return;
            licznikWrogow++;

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

        void WrogZniknal()
        {
            zniknieciWrogowie++;

            if (zniknieciWrogowie >= wrogowieNaFale)
                spawnZablokowany = true;

            if (spawnZablokowany && wrogowie.Count == 0)
            {
                ZakonczFale();
            }
        }

        void ZakonczFale()
        {
            zablokowanieUlepszeniem = true;
            PokazUpgradeOverlay();
        }
        void PokazUpgradeOverlay()
        {
            LosujUlepszenia();
            AktualizujStatystykiNaOverlay();

            UstawPrzyciskUlepszenia(UpgradeBtn1, wylosowaneUlepszenia[0]);
            UstawPrzyciskUlepszenia(UpgradeBtn2, wylosowaneUlepszenia[1]);
            UstawPrzyciskUlepszenia(UpgradeBtn3, wylosowaneUlepszenia[2]);

            UpgradeOverlay.Visibility = Visibility.Visible;
        }


        enum TypUlepszenia
        {
            SlowerEnemies,
            SlowerSpawn,
            MorePoints,
            MoreLife
        }

        List<TypUlepszenia> dostepneUlepszenia = new List<TypUlepszenia>
        {
            TypUlepszenia.SlowerEnemies,
            TypUlepszenia.SlowerSpawn,
            TypUlepszenia.MorePoints,
            TypUlepszenia.MoreLife
        };

        List<TypUlepszenia> wylosowaneUlepszenia;

        void LosujUlepszenia()
        {
            wylosowaneUlepszenia = dostepneUlepszenia
                .OrderBy(x => losowy.Next())
                .Take(3)
                .ToList();
        }

        void UstawPrzyciskUlepszenia(Button btn, TypUlepszenia typ)
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



        void AktualizujStatystykiNaOverlay()
        {
            SpeedStat.Text = $"ENEMY SPEED: {(modyfikatorPredkosci * 100):0}%";
            SpawnStat.Text = $"SPAWN RATE: {(modyfikatorSpawnu * 100):0}%";
            PointsStat.Text = $"POINTS MULTI: x{mnoznikPunktow:0.00}";
        }


        void Upgrade_SlowerEnemies(object sender, RoutedEventArgs e)
        {
            modyfikatorPredkosci *= 0.9;
            ZastosujUlepszenieIZamknij();
        }

        void Upgrade_SlowerSpawn(object sender, RoutedEventArgs e)
        {
            modyfikatorSpawnu *= 0.9;
            ZastosujUlepszenieIZamknij();
        }

        void Upgrade_MorePoints(object sender, RoutedEventArgs e)
        {
            mnoznikPunktow += 0.25;
            ZastosujUlepszenieIZamknij();
        }

        void Upgrade_MoreLife(object sender, RoutedEventArgs e)
        {
            zycia += 1;
            OdswiezZycia();
            ZastosujUlepszenieIZamknij();
        }

        void ZastosujUlepszenieIZamknij()
        {
            UpgradeOverlay.Visibility = Visibility.Collapsed;

            zablokowanieUlepszeniem = false;
            spawnZablokowany = false;

            fala++;
            OdswiezFale();

            modyfikatorPredkosci *= 1.1;
            modyfikatorSpawnu *= 1.1;

            czasRuchuWroga /= modyfikatorPredkosci;
            interwalSpawnu /= modyfikatorSpawnu;

            zniknieciWrogowie = 0;

            AktualizujStatystykiNaOverlay();

            timerWrogow.Stop();
            timerWrogow.Interval = TimeSpan.FromSeconds(interwalSpawnu);
            timerWrogow.Start();

            NowaTura();
        }


        double PobierzBezpiecznyKat()
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

        UIElement UtworzWrogaUI(double x, double y)
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

        void Main_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            oknoGlowne.NavigateTo(new StartView(oknoGlowne));
        }

        void Menu_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlayClick();
            timerWrogow.Stop();
            timerWejscia.Stop();
            timerNadawania.Stop();
            oknoGlowne.NavigateTo(new MenuView(oknoGlowne, poziomTrudnosci, pokazPodpowiedzi));
        }

        private void StartDzwiekZabicia()
        {
            dzwiekZabicia = new MediaPlayer();
            dzwiekZabicia.Open(new Uri("Assets/Sounds/LifeDroplet.mp3", UriKind.Relative));
            dzwiekZabicia.Volume = 0.7;
            dzwiekZabicia.Stop();
            dzwiekZabicia.Position = TimeSpan.Zero;
            dzwiekZabicia.Play();
        }

        private void StartDzwiekObrazen()
        {
            dzwiekObrazen = new MediaPlayer();
            dzwiekObrazen.Open(new Uri("Assets/Sounds/Dead.mp3", UriKind.Relative));
            dzwiekObrazen.Volume = 0.05;
            dzwiekObrazen.Stop();
            dzwiekObrazen.Position = TimeSpan.Zero;
            dzwiekObrazen.Play();
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