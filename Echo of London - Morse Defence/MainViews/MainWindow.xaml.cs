using Echo_of_London___Morse_Defence.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Echo_of_London___Morse_Defence
{
    public partial class MainWindow : Window
    {
        private Stack<UserControl> navigationHistory = new Stack<UserControl>();

        public MainWindow()
        {
            InitializeComponent();
            NavigateTo(new Views.StartView());
        }

        public void NavigateTo(UserControl nextView)
        {
            if (MainContent.Content != null)
            {
                navigationHistory.Push((UserControl)MainContent.Content);
            }
            MainContent.Content = nextView;
        }

        public void GoBack()
        {
            if (navigationHistory.Count > 0)
            {
                var lastView = navigationHistory.Pop();
                MainContent.Content = lastView;
            }
        }
    }
}