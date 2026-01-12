using Echo_of_London___Morse_Defence.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Echo_of_London___Morse_Defence
{
    public partial class MainWindow : Window
    {
        private MediaPlayer muzykaTla;
        Stack<UserControl> historia = new Stack<UserControl>();

        public MainWindow()
        {
            InitializeComponent();
            StartMuzyka();
            NavigateTo(new StartView());
        }

        public void NavigateTo(UserControl nowyWidok)
        {
            if (MainContent.Content != null)
                historia.Push((UserControl)MainContent.Content);

            MainContent.Content = nowyWidok;
        }

        public void GoBack()
        {
            if (historia.Count > 0)
                MainContent.Content = historia.Pop();
        }

        public GameView PobierzGameViewZHistorii()
        {
            foreach (var widok in historia)
            {
                if (widok is GameView gv)
                    return gv;
            }
            return null;
        }

        public void WyczyscHistorie()
        {
            historia.Clear();
        }

        private void StartMuzyka()
        {
            muzykaTla = new MediaPlayer();
            muzykaTla.Open(new Uri("Assets/Sounds/music.mp3", UriKind.Relative));
            muzykaTla.Volume = 0.04;

            muzykaTla.MediaEnded += (s, e) =>
            {
                muzykaTla.Position = TimeSpan.Zero;
                muzykaTla.Play();
            };

            muzykaTla.Play();
        }
    }
}