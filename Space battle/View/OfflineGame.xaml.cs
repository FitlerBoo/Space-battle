using Google.Protobuf.WellKnownTypes;
using Space_battle.Model;
using Space_battle.Offline;
using Space_battle.Offline.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
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
using System.Windows.Threading;

namespace Space_battle.View
{
    /// <summary>
    /// Логика взаимодействия для OfflineGame.xaml
    /// </summary>
    public partial class OfflineGame : Page
    {
        private readonly double _applicationHeight = Application.Current.MainWindow.Height;
        private readonly double _applicationWidth = Application.Current.MainWindow.Width;
        private Starship _player1;
        private Starship _player2;
        private DispatcherTimer _gameTimer = new DispatcherTimer();
        private Task _renderTask;
        private bool _increaseSpeedP1;
        private bool _increaseSpeedP2;
        private bool _rotationSideP1;
        private bool _rotationSideP2;

        public OfflineGame()
        {
            this.DataContext = this;
            InitializeComponent();

            RenderStartScene();

            _renderTask = new Task(RenderTask);
            StartGame();

            _gameTimer.Interval = TimeSpan.FromMilliseconds(15);

            MyCanvas.Focus();
        }

        private void RenderStartScene()
        {
            _player1 = new Starship(false);
            _player2 = new Starship(true);
            MyCanvas.Children.Add(_player1.GetForm());
            MyCanvas.Children.Add(_player2.GetForm());
        }
        private void StartGame()
        {
            _renderTask.Start();
        }

        private void RenderTask()
        {
            var renderTimer = Stopwatch.StartNew();
            while (_player1.GetHealth() > 0 && _player2.GetHealth() > 0)
            {
                if (renderTimer.ElapsedMilliseconds < 15)
                    continue;

                renderTimer.Restart();

                try
                {
                    this.Dispatcher.Invoke(new Action(() => GameLoop()));
                }
                catch (TaskCanceledException)
                {
                    this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                }
            }

            var winner = _player1.GetHealth() > _player2.GetHealth() ? "Player 1" : "Player 2";
            // TODO: логика добавления в БД результатов
            MessageBox.Show(winner + " wins!");
            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);

            //using (ApplicationContext ac = new ApplicationContext())
            //{
            //    ac.Results.Add();
            //}
        }

        private void GameLoop()
        {
            RenderPlayerObjects(_player1, _increaseSpeedP1, _rotationSideP1);
            RenderPlayerObjects(_player2, _increaseSpeedP2, _rotationSideP2);
        }
        private void RenderPlayerObjects(Starship player, bool increaseSpeed, bool rotationSide)
        {
            RenderProjectiles(player);
        }

        private void RenderProjectiles(Starship player)
        {
            foreach(var projectile in player.GetProjectiles())
            {
                projectile.Move();
                CheckBorderConditions(projectile);
            }
        }

        private void CheckBorderConditions(SingleGameObject gObject)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A) _player1.RotateObject(true);
            if (e.Key == Key.Left) _player2.RotateObject(true);

            if (e.Key == Key.D) _player1.RotateObject(false);
            if (e.Key == Key.Right) _player2.RotateObject(false);

            if (e.Key == Key.W) _player1.Move(true);
            if (e.Key == Key.Up) _player2.Move(true);

            if (e.Key == Key.Space) MyCanvas.Children.Add(_player1.MakeProjectile().GetForm());
            if (e.Key == Key.NumPad0) MyCanvas.Children.Add(_player2.MakeProjectile().GetForm());
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W) _increaseSpeedP1 = false;
            if (e.Key == Key.Up) _increaseSpeedP2 = false;
        }
    }
}
