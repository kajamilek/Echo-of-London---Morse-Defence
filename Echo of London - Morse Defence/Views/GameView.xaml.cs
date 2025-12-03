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
        // główne okno do nawigacji między widokami
        MainWindow oknoGlowne;

        // generator liczb losowych dla spawnu wrogów i liter
        Random losowy = new Random();

        // timery do spawnu wrogów i automatycznego wysyłania kodu morse'a
        DispatcherTimer timerWrogow;
        DispatcherTimer timerWejscia;

        // środek planszy - tu jest gracz
        double srodekX, srodekY;

        // kąty dla 6 sektorów na radarze
        double[] katySektor = { 0, 60, 120, 180, 240, 300 };

        // aktualnie wpisywany kod morse'a
        string aktualnyMorse = "";

        // litery przypisane do sektorów w tej turze
        string aktualneListery = "";

        // max 6 znaków w kodzie morse'a
        int maxDlugoscMorse = 6;

        // pola tekstowe do wyświetlania wpisywanego kodu
        TextBlock[] polaMorse;

        // lista aktywnych wrogów na planszy
        List<DaneWroga> wrogowie = new List<DaneWroga>();

        // ustawienia trudności
        string poziomTrudnosci;
        double interwalSpawnu;
        double czasRuchuWroga;
        double opoznienieWejscia;
        bool pokazPodpowiedzi;

        // stan gry
        int zycia = 3;
        int punkty = 0;
        int fala = 1;
        int licznikWrogow = 0;
        bool koniecGry = false;
        bool wynikZapisany = false;

        // kolor używany w UI
        Brush kolorLinii = (Brush)new BrushConverter().ConvertFrom("#029273");

        // ścieżka do pliku z wynikami w AppData
        static string sciezkaWynikow = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EchoOfLondon", "highscores.txt");

        // słownik kodów morse'a dla każdej litery
        Dictionary<char, string> koderMorse = new Dictionary<char, string>()
        {
            {'A',"•–"}, {'B',"–•••"}, {'C',"–•–•"}, {'D',"–••"},
            {'E',"•"}, {'F',"••–•"}, {'G',"––•"}, {'H',"••••"},
            {'I',"••"}, {'J',"•–––"}, {'K',"–•–"}, {'L',"•–••"},
            {'M',"––"}, {'N',"–•"}, {'O',"–––"}, {'P',"•––•"},
            {'Q',"––•–"}, {'R',"•–•"}, {'S',"•••"}, {'T',"–"},
            {'U',"••–"}, {'V',"•••–"}, {'W',"•––"}, {'X',"–••–"},
            {'Y',"–•––"}, {'Z',"––••"}
        };

        // dane wroga - pozycja, sektor, czas spawnu i animacje
        class DaneWroga
        {
            public UIElement Element { get; set; }
            public double Kat { get; set; }
            public int Sektor { get; set; }
            public DateTime CzasSpawnu { get; set; }
            public DoubleAnimation AnimX { get; set; }
            public DoubleAnimation AnimY { get; set; }
        }

        // konstruktor - ustawia parametry i czeka na załadowanie widoku
        public GameView(MainWindow mw, string trudnosc, bool podpowiedzi = false)
        {
            InitializeComponent();
            oknoGlowne = mw;
            poziomTrudnosci = trudnosc;
            pokazPodpowiedzi = podpowiedzi;
            UstawParametryTrudnosci();
            Loaded += NaZaladowaniu;
        }

        // ustawia szybkość gry w zależności od wybranego poziomu
        void UstawParametryTrudnosci()
        {
            string t = poziomTrudnosci.ToLower();

            if (t == "easy")
            {
                interwalSpawnu = 3.0;      // wróg co 3 sekundy
                czasRuchuWroga = 5.0;      // 5 sekund do środka
                opoznienieWejscia = 1.0;   // sekunda na wpisanie
            }
            else if (t == "hard")
            {
                interwalSpawnu = 1.2;      // wróg co 1.2 sekundy
                czasRuchuWroga = 2.0;      // 2 sekundy do środka
                opoznienieWejscia = 0.6;   // pół sekundy na wpisanie
            }
            else
            {
                interwalSpawnu = 2.0;      // domyślnie normal
                czasRuchuWroga = 3.0;
                opoznienieWejscia = 0.8;
            }
        }

        // inicjalizacja po załadowaniu widoku
        void NaZaladowaniu(object sender, RoutedEventArgs e)
        {
            // oblicz środek radaru
            srodekX = EnemyCanvas.ActualWidth / 2;
            srodekY = EnemyCanvas.ActualHeight / 2;

            // pobierz referencje do pól morse'a
            polaMorse = new TextBlock[] { MorseSlot0, MorseSlot1, MorseSlot2, MorseSlot3, MorseSlot4, MorseSlot5 };

            // pokaż lub ukryj panel podpowiedzi
            HintsPanel.Visibility = pokazPodpowiedzi ? Visibility.Visible : Visibility.Collapsed;

            // ustaw początkowe wartości w UI
            OdswiezZycia();
            OdswiezPunkty();
            OdswiezFale();

            // timer do auto-wysyłania kodu po chwili nieaktywności
            timerWejscia = new DispatcherTimer();
            timerWejscia.Interval = TimeSpan.FromSeconds(opoznienieWejscia);
            timerWejscia.Tick += (s, ev) => { timerWejscia.Stop(); WyslijKodMorse(); };

            // ustaw focus na widok żeby działała klawiatura
            Focusable = true;
            Focus();
            KeyDown += ObslugaKlawiatury;
            MouseDown += (s, ev) => Focus();

            // timer do spawnu wrogów - użyj nazwanej metody
            timerWrogow = new DispatcherTimer();
            timerWrogow.Interval = TimeSpan.FromSeconds(interwalSpawnu);
            timerWrogow.Tick += TimerWrogow_Tick;
            timerWrogow.Start();

            // rozpocznij pierwszą turę
            NowaTura();
        }

        // resetuje grę do stanu początkowego bez zmiany widoku
        public void ResetujGre()
        {
            // zatrzymaj timery
            timerWrogow.Stop();
            timerWejscia.Stop();

            // usuń wszystkich wrogów z planszy
            foreach (var w in wrogowie.ToList())
            {
                w.Element.BeginAnimation(Canvas.LeftProperty, null);
                w.Element.BeginAnimation(Canvas.TopProperty, null);
                EnemyCanvas.Children.Remove(w.Element);
            }
            wrogowie.Clear();

            // wyczyść też canvas na wszelki wypadek
            EnemyCanvas.Children.Clear();

            // zresetuj stan gry
            zycia = 3;
            punkty = 0;
            fala = 1;
            licznikWrogow = 0;
            koniecGry = false;
            wynikZapisany = false;
            aktualnyMorse = "";

            // ukryj overlay game over
            GameOverOverlay.Visibility = Visibility.Collapsed;

            // odśwież UI
            OdswiezZycia();
            OdswiezPunkty();
            OdswiezFale();
            OdswiezWyswietlaczMorse();

            // nowa tura
            NowaTura();

            // wystartuj timer ponownie (ten sam, nie nowy)
            timerWrogow.Start();

            // przywróć focus
            Focus();
        }

        // osobna metoda dla ticka timera żeby można było ją odpiąć
        void TimerWrogow_Tick(object sender, EventArgs e)
        {
            StworzWroga();
        }

        // aktualizuje wyświetlanie żyć jako kółka
        void OdswiezZycia()
        {
            string pelne = new string('●', Math.Max(0, zycia));
            string puste = new string('X', Math.Max(0, 3 - zycia));
            LivesText.Text = pelne + puste;
            LivesText.Foreground = Brushes.White;
        }

        void OdswiezPunkty() { ScoreText.Text = punkty.ToString(); }
        void OdswiezFale() { WaveText.Text = fala.ToString(); }

        // gracz traci życie gdy wróg dotrze do środka
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

        // efekt wizualny - gracz miga na czerwono przy stracie życia
        void MignijGracza()
        {
            var oryginalny = player.Fill;
            player.Fill = Brushes.Red;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += (s, e) => { timer.Stop(); player.Fill = oryginalny; };
            timer.Start();
        }

        // koniec gry - zatrzymuje wszystko i pokazuje ekran końcowy
        void ZakonczGre()
        {
            koniecGry = true;
            wynikZapisany = false;
            timerWrogow?.Stop();
            timerWejscia?.Stop();

            // usuń wszystkich wrogów
            foreach (var w in wrogowie.ToList())
            {
                w.Element.BeginAnimation(Canvas.LeftProperty, null);
                w.Element.BeginAnimation(Canvas.TopProperty, null);
                EnemyCanvas.Children.Remove(w.Element);
            }
            wrogowie.Clear();

            // pokaż wynik końcowy
            FinalScoreText.Text = "SCORE: " + punkty;
            FinalWaveText.Text = "WAVE: " + fala;
            SaveConfirmText.Text = "";
            PlayerNameTextBox.Text = "PLAYER";
            GameOverOverlay.Visibility = Visibility.Visible;
            PlayerNameTextBox.Focus();
            PlayerNameTextBox.SelectAll();
        }

        // zapis wyniku do pliku
        void SaveScore_Click(object sender, RoutedEventArgs e)
        {
            if (wynikZapisany)
            {
                SaveConfirmText.Text = "ALREADY SAVED!";
                SaveConfirmText.Foreground = Brushes.Orange;
                return;
            }

            // oczyść nazwę gracza ze znaków specjalnych
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

        // zapisuje wynik w formacie: nazwa|punkty|fala|trudność|data
        void ZapiszWynikDoPliku(string nazwa, int pkt, int f, string trudnosc)
        {
            string folder = System.IO.Path.GetDirectoryName(sciezkaWynikow);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string wpis = nazwa + "|" + pkt + "|" + f + "|" + trudnosc.ToUpper() + "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            using (StreamWriter sw = File.AppendText(sciezkaWynikow))
                sw.WriteLine(wpis);
        }

        // wczytuje wyniki z pliku - używane przez ScoreboardView
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
                        lista.Add(new ScoreEntry
                        {
                            PlayerName = czesci[0],
                            Score = int.Parse(czesci[1]),
                            Wave = int.Parse(czesci[2]),
                            Difficulty = czesci[3],
                            Date = czesci[4]
                        });
                    }
                }
            }
            catch { }

            return lista.OrderByDescending(x => x.Score).ToList();
        }

        // obsługa klawiszy - strzałki do morse'a, spacja/enter do wysłania
        void ObslugaKlawiatury(object sender, KeyEventArgs e)
        {
            if (koniecGry) return;

            if (e.Key == Key.Left) { DodajSymbolMorse("–"); e.Handled = true; }
            else if (e.Key == Key.Right) { DodajSymbolMorse("•"); e.Handled = true; }
            else if (e.Key == Key.Space || e.Key == Key.Enter) { WyslijKodMorse(); e.Handled = true; }
            else if (e.Key == Key.Back || e.Key == Key.Delete) { UsunOstatniSymbol(); e.Handled = true; }
            else if (e.Key == Key.Escape) { WyczyscMorse(); e.Handled = true; }
        }

        // dodaje kropkę lub kreskę do kodu morse'a
        void DodajSymbolMorse(string symbol)
        {
            if (aktualnyMorse.Length >= maxDlugoscMorse) return;
            aktualnyMorse += symbol;
            OdswiezWyswietlaczMorse();

            // restart timera - po chwili nieaktywności kod zostanie wysłany
            timerWejscia.Stop();
            timerWejscia.Start();
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

        // wyświetla kod morse'a wyrównany do prawej strony
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

        // sprawdza wpisany kod i niszczy wroga jeśli trafiono
        void WyslijKodMorse()
        {
            timerWejscia.Stop();
            if (string.IsNullOrEmpty(aktualnyMorse)) return;

            // szukaj litery pasującej do wpisanego kodu
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
                // sprawdź czy ta litera jest w aktualnej turze
                int indeksSektor = aktualneListery.IndexOf(znalezionaLitera.Value);
                if (indeksSektor >= 0)
                {
                    bool zniszczony = ZniszczWrogaWSektor(indeksSektor);
                    if (zniszczony)
                    {
                        MignijPola(Brushes.LimeGreen);  // zielone - trafiono
                        DodajPunkty(100);
                    }
                    else
                    {
                        MignijPola(Brushes.Cyan);  // cyjan - brak wroga w sektorze
                    }
                }
                else
                {
                    MignijPola(Brushes.Orange);  // pomarańczowe - zła litera
                }
            }
            else
            {
                MignijPola(Brushes.Red);  // czerwone - nieprawidłowy kod
            }

            WyczyscMorse();
        }

        // oblicza sektor na podstawie kąta (0-5)
        int ObliczSektor(double kat)
        {
            kat = ((kat % 360) + 360) % 360;
            double przesuniete = (kat + 30) % 360;
            return (int)(przesuniete / 60);
        }

        // niszczy najstarszego wroga w danym sektorze
        bool ZniszczWrogaWSektor(int sektor)
        {
            var wrogiWSektor = wrogowie.Where(w => w.Sektor == sektor).OrderBy(w => w.CzasSpawnu).ToList();
            if (wrogiWSektor.Count == 0) return false;

            var wrogDoZniszczenia = wrogiWSektor.First();

            // zatrzymaj animacje ruchu
            wrogDoZniszczenia.Element.BeginAnimation(Canvas.LeftProperty, null);
            wrogDoZniszczenia.Element.BeginAnimation(Canvas.TopProperty, null);

            AnimujZniszczenie(wrogDoZniszczenia.Element);
            wrogowie.Remove(wrogDoZniszczenia);
            return true;
        }

        // animacja powiększenia i zniknięcia wroga
        void AnimujZniszczenie(UIElement wrog)
        {
            var skalowanie = new ScaleTransform(1, 1);
            wrog.RenderTransform = skalowanie;
            wrog.RenderTransformOrigin = new Point(0.5, 0.5);

            var animSkala = new DoubleAnimation(1, 2, TimeSpan.FromMilliseconds(200));
            var animPrzezroczystosc = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            animPrzezroczystosc.Completed += (s, e) => EnemyCanvas.Children.Remove(wrog);

            skalowanie.BeginAnimation(ScaleTransform.ScaleXProperty, animSkala);
            skalowanie.BeginAnimation(ScaleTransform.ScaleYProperty, animSkala);
            wrog.BeginAnimation(UIElement.OpacityProperty, animPrzezroczystosc);
        }

        // miga polami morse'a na dany kolor jako feedback
        void MignijPola(Brush kolor)
        {
            foreach (var pole in polaMorse) pole.Foreground = kolor;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            timer.Tick += (s, e) => { timer.Stop(); OdswiezWyswietlaczMorse(); };
            timer.Start();
        }

        // nowa tura - generuje nowe litery dla sektorów
        void NowaTura()
        {
            aktualneListery = GenerujLosoweListery();
            PokazListeryNaSektor(aktualneListery);
            if (pokazPodpowiedzi) PokazPanelPodpowiedzi(aktualneListery);
        }

        // wyświetla podpowiedzi z kodami morse'a
        void PokazPanelPodpowiedzi(string litery)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in litery) sb.AppendLine(c + "   " + koderMorse[c]);
            MorseDisplay.Text = sb.ToString();
        }

        // losuje 6 unikalnych liter
        string GenerujLosoweListery()
        {
            string alfabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            HashSet<char> wybrane = new HashSet<char>();
            while (wybrane.Count < 6)
            {
                char c = alfabet[losowy.Next(alfabet.Length)];
                wybrane.Add(c);
            }
            return new string(wybrane.ToArray());
        }

        // umieszcza litery na radarze przy odpowiednich sektorach - wyśrodkowane
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

                // zmierz rozmiar tekstu żeby wyśrodkować
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double szerokoscLitery = tb.DesiredSize.Width;
                double wysokoscLitery = tb.DesiredSize.Height;

                // ustaw pozycję tak żeby środek litery był na wyliczonej pozycji
                Canvas.SetLeft(tb, x - szerokoscLitery / 2);
                Canvas.SetTop(tb, y - wysokoscLitery / 2);

                LetterCanvas.Children.Add(tb);
            }
        }

        // tworzy nowego wroga i uruchamia jego ruch do środka
        void StworzWroga()
        {
            if (koniecGry) return;
            licznikWrogow++;

            // co 10 wrogów nowa fala z nowymi literami
            if (licznikWrogow % 10 == 0)
            {
                fala++;
                OdswiezFale();
                NowaTura();
            }

            double rozmiar = 30;
            double promienSpawnu = 160;

            // losuj kąt unikając linii podziału sektorów
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

            // animacja ruchu do środka
            var animX = new DoubleAnimation(startX - rozmiar / 2, srodekX - rozmiar / 2, TimeSpan.FromSeconds(czasRuchuWroga));
            var animY = new DoubleAnimation(startY - rozmiar / 2, srodekY - rozmiar / 2, TimeSpan.FromSeconds(czasRuchuWroga));
            animX.FillBehavior = FillBehavior.Stop;
            animY.FillBehavior = FillBehavior.Stop;

            dane.AnimX = animX;
            dane.AnimY = animY;

            // gdy wróg dotrze do środka - gracz traci życie
            animY.Completed += (s, e) =>
            {
                if (wrogowie.Contains(dane))
                {
                    EnemyCanvas.Children.Remove(wrog);
                    wrogowie.Remove(dane);
                    StracZycie();
                }
            };

            wrog.BeginAnimation(Canvas.LeftProperty, animX);
            wrog.BeginAnimation(Canvas.TopProperty, animY);
        }

        // losuje kąt omijając linie podziału sektorów żeby wróg nie był na granicy
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

        // tworzy element graficzny wroga - kółko z obwódką
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

        // powrót do ekranu startowego
        void Main_Click(object sender, RoutedEventArgs e)
        {
            oknoGlowne.NavigateTo(new StartView(oknoGlowne));
        }

        // przejście do menu pauzy
        void Menu_Click(object sender, RoutedEventArgs e)
        {
            // zatrzymaj timery przed przejściem do menu
            timerWrogow.Stop();
            timerWejscia.Stop();

            // przekaż parametry gry do menu żeby reset działał
            oknoGlowne.NavigateTo(new MenuView(oknoGlowne, poziomTrudnosci, pokazPodpowiedzi));
        }

        // wywoływane przez przycisk Give Up w menu
        public void WymusGameOver()
        {
            ZakonczGre();
        }
    }

    // klasa przechowująca dane wyniku gracza
    public class ScoreEntry
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int Wave { get; set; }
        public string Difficulty { get; set; }
        public string Date { get; set; }
    }
}