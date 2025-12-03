using Echo_of_London___Morse_Defence.Views;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Echo_of_London___Morse_Defence
{
    public partial class MainWindow : Window
    {
        Stack<UserControl> historia = new Stack<UserControl>();

        public MainWindow()
        {
            InitializeComponent();
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
    }
}