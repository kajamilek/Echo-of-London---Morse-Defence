using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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

        public GameView(MainWindow main, string difficulty)
        {
            InitializeComponent();
            this.main = main;

            Loaded += GameView_Loaded;
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            // środek canvasu
            centerX = EnemyCanvas.ActualWidth / 2;
            centerY = EnemyCanvas.ActualHeight / 2;

            // timer — co 2 sekundy pojawia się nowy przeciwnik
            enemyTimer = new DispatcherTimer();
            enemyTimer.Interval = TimeSpan.FromSeconds(2);
            enemyTimer.Tick += (s, ev) => SpawnEnemy();
            enemyTimer.Start();
        }

        private void SpawnEnemy()
        {
            double size = 20;
            double startX, startY;

            // losujemy stronę (góra/dół/lewo/prawo)
            int side = rng.Next(4);
            if (side == 0) { startX = rng.NextDouble() * EnemyCanvas.ActualWidth; startY = 0 - size; }
            else if (side == 1) { startX = EnemyCanvas.ActualWidth + size; startY = rng.NextDouble() * EnemyCanvas.ActualHeight; }
            else if (side == 2) { startX = rng.NextDouble() * EnemyCanvas.ActualWidth; startY = EnemyCanvas.ActualHeight + size; }
            else { startX = 0 - size; startY = rng.NextDouble() * EnemyCanvas.ActualHeight; }

            // utworzenie przeciwnika
            Ellipse enemy = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromRgb(255, 70, 70))
            };

            // ustawienie pozycji startowej
            Canvas.SetLeft(enemy, startX);
            Canvas.SetTop(enemy, startY);
            EnemyCanvas.Children.Add(enemy);

            // animacja ruchu w stronę środka
            double duration = 3; // sekundy
            var animX = new DoubleAnimation(startX, centerX - size / 2, TimeSpan.FromSeconds(duration));
            var animY = new DoubleAnimation(startY, centerY - size / 2, TimeSpan.FromSeconds(duration));

            animX.FillBehavior = FillBehavior.Stop;
            animY.FillBehavior = FillBehavior.Stop;

            animY.Completed += (s, e) =>
            {
                EnemyCanvas.Children.Remove(enemy);
                // TODO: tutaj możesz dodać logikę kolizji / utraty życia
            };

            enemy.BeginAnimation(Canvas.LeftProperty, animX);
            enemy.BeginAnimation(Canvas.TopProperty, animY);
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.NavigateTo(new MenuView(mainWindow));
        }
    }
}