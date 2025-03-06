using System.Text.Json;
using Space_battle.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting;
using System.Security.Policy;
using System.Text;
using System.Threading;
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
using System.ComponentModel.Design;
using System.IO;

namespace Space_battle.View
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        #region Application Properties
        private readonly double applicationHeight = Application.Current.MainWindow.Height;
        private readonly double applicationWidth = Application.Current.MainWindow.Width;
        private bool _isClient;
        private bool _offline;
        private bool _increaseP1Speed;
        private bool _increaseP2Speed;
        Position Player1DefaultPos => new Position(370, 500, -9);
        Position Player2DefaultPos => new Position(370, 40, 9);
        #endregion

        #region Render Objects
        private Spaceship player1;
        private Spaceship player2;
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private Queue<UIElement> itemRemover = new Queue<UIElement>();
        private Task gameTask;
        #endregion

        #region Commands
        private const byte TRUE_COMMAND = 0b1;
        private const byte FALSE_COMMAND = 0b0;
        private bool rotateRight;
        private bool rotateLeft;
        private bool moveForward;
        private bool moveForwardP2;
        private bool fire;
        #endregion

        #region Net
        private UDPServer server;
        private UDPClient udpClient;
        #endregion

        /// <summary>
        /// Является ли пользователь клиентом?
        /// </summary>
        /// <param name="isClient"></param>
        public Game(bool isClient, bool offline)
        {
            _isClient = isClient;
            _offline = offline;
            DataContext = this;
            InitializeComponent();

            RenderStartScene();

            if (isClient)
            {
                udpClient = new UDPClient();
                udpClient.StartDataExchange();
            }
            else
            {
                server = new UDPServer(player1, player2);
                if(!_offline) server.StartDataExchange();
            }

            gameTask = new Task(RenderTask); 
            
            gameTimer.Interval = TimeSpan.FromMilliseconds(15);

            MyCanvas.Focus();

        }

        private void RenderStartScene()
        {
            Canvas.SetRight(Player2HP, 0);
            AddPlayers();
        }

        public void StartGame()
        {
            gameTask.Start();
        }

        private async void RenderTask()
        {
            var renderTimer = Stopwatch.StartNew();
            while (player1.Health > 0 && player2.Health > 0)
            {

                if (renderTimer.ElapsedMilliseconds < 15)
                {
                    //await Task.Delay(30);
                    //
                    continue;
                }

                


                renderTimer.Restart();

                try
                {
                    if (_isClient)
                        await this.Dispatcher.InvokeAsync(new Action(() => GameLoopClient()));
                        
                    else
                        await this.Dispatcher.InvokeAsync(new Action(() => GameLoopServer()));
                }
                catch (TaskCanceledException)
                {
                    this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                }
            }
            var winner = player1.Health > player2.Health ? "Player 1" : "Player 2";
            MessageBox.Show(winner + " wins!");
            //MessageBox.Show()
            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }

        #region Server part
        private void GameLoopServer()
        {
            KeysUpdate();
            MovePlayer();
            MoveEnemy();

            MoveBullets(player1.Bullets);
            MoveBullets(player2.Bullets);

            // TODO : Подумать как сделать неуязвимость на промежуток времени и анимацию.
            var playerImmune = CheckBulletsIntersectionsPlayer(player1, player2.Bullets);
            var enemyImmune = CheckBulletsIntersectionsPlayer(player2, player1.Bullets);
        }

        private bool CheckBulletsIntersectionsPlayer(Spaceship playerObj, Queue<Bullet> bullets)
        {
            foreach (var bullet in bullets.ToArray())
                if (playerObj.HitBox.IntersectsWith(bullet.HitBox))
                {
                    MyCanvas.Children.Remove(bullets.Dequeue().Form);
                    playerObj.TakeDamage();
                    UpdateScore(player1, player2);
                    return true;
                }
            return false;
        }

        private void MoveEnemy()
        {
            if (CheckBorderCondition(player2))
            {
                if (server.Command[(int)Commands.MoveForward])
                {
                    moveForwardP2 = true;
                    _increaseP2Speed = true;
                }
                else
                {
                    moveForwardP2 = false;
                    _increaseP2Speed = false;
                }

                if (moveForwardP2 || player2.Speed > 0)
                {
                    player2.CalculateSpeed(_increaseP2Speed);
                    player2.Move();
                }
            }
            else player2.MoveToPosition(Player2DefaultPos);
            if (server.Command[(int)Commands.Fire]) MyCanvas.Children.Add(player2.MakeBullet().Form);
            if (server.Command[(int)Commands.RotateLeft]) player2.RotateObject(true);
            if (server.Command[(int)Commands.RotateRight]) player2.RotateObject(false);
        }
        private void MovePlayer()
        {
            if (CheckBorderCondition(player1))
            {
                if (moveForward || player1.Speed > 0)
                {
                    player1.CalculateSpeed(_increaseP1Speed);
                    player1.Move();
                }
            }
            else
                player1.MoveToPosition(Player1DefaultPos); 
        }

        private void MoveBullets(Queue<Bullet> bullets)
        {
            foreach (var bullet in bullets.ToArray())
            {
                if (MyCanvas.Children.Contains(bullet.Form))
                {
                    if (CheckBorderCondition(bullet))
                        bullet.Move(); 
                    else
                        MyCanvas.Children.Remove(bullets.Dequeue().Form);
                }
            }
        }
        #endregion

        #region  Client part
        private void GameLoopClient()
        {
            KeysUpdate();
            udpClient.Command = MakeCommand();
            var objects = udpClient.GetRenderedObjects();
            RenderScene(player1 : objects.Item1, player2 : objects.Item2);
        }

        private byte[] MakeCommand()
        {
            var commands = new bool[] { moveForward, fire, rotateLeft, rotateRight };
            var resultCommand = new byte[4];
            for (int i = 0; i < commands.Length; i++)
                resultCommand[i] = commands[i] ? TRUE_COMMAND : FALSE_COMMAND;
            return resultCommand;
        }
         
        private void RenderScene(Spaceship player1, Spaceship player2)
        {
            if (this.player1 != null && player2 != null)
            {
                itemRemover.Enqueue(this.player1.Form);
                itemRemover.Enqueue(this.player2.Form);
            }
            if (player1 == null || player2 == null) return;
            ClearObsoleteObjects();

            UpdateScore(player1, player2);
            AddObject(player1);
            AddObject(player2);

            AddBullets(player1.Bullets);
            AddBullets(player2.Bullets);
        }
        #endregion

        #region Common
        private bool CheckBorderCondition(GameObject obj)
        {
            return obj.Y > -40 && (obj.Y - 30) < applicationHeight &&
                (obj.X + 30) < applicationWidth && obj.X > -40;
        }

        private void UpdateScore(Spaceship player1, Spaceship player2)
        {
            if (player1 == null || player2 == null) return;
            Player1HP.Text = this.player1.MessageHealth;
            Player2HP.Text = player2.MessageHealth;
        }
        #endregion

        #region Game objects creation and removal
        private void AddObject(GameObject gObj)
        {
            if (gObj == null) return;

            gObj.MakeVisualMovement();
            MyCanvas.Children.Add(gObj.Form);
            this.itemRemover.Enqueue(gObj.Form);
        }

        private void AddPlayers()
        {
            player1 = new Spaceship(GameObjectStyle.Red, GameObjectType.Spaceship, Player1DefaultPos);
            player2 = new Spaceship(GameObjectStyle.Yellow, GameObjectType.Spaceship, Player2DefaultPos);
            MyCanvas.Children.Add(player1.Form);
            MyCanvas.Children.Add(player2.Form);
            Canvas.SetZIndex(player1.Form, 1);
            Canvas.SetZIndex(player2.Form, 1);
        }

        private void ClearObsoleteObjects()
        {
            foreach (var obj in itemRemover)
                MyCanvas.Children.Remove(obj);
            itemRemover.Clear();
        }

        private void AddBullets(Queue<Bullet> bullets)
        {
            if (bullets.Count > 0)
                foreach (var bullet in bullets) AddObject(bullet);
        }
        #endregion

        #region Keys
        private void KeysUpdate()
        {
            if (Keyboard.IsKeyDown(Key.W))
            {
                moveForward = true;
                _increaseP1Speed = true;
            }
            else
            {
                moveForward = false;
                _increaseP1Speed = false;
            }

            if (Keyboard.IsKeyDown(Key.D)) player1.RotateObject(false);
            if (Keyboard.IsKeyDown(Key.A)) player1.RotateObject(true);
            if (Keyboard.IsKeyDown(Key.Space)) MyCanvas.Children.Add(player1.MakeBullet().Form);

            #region Second player
            if (_offline)
            {
                if (Keyboard.IsKeyDown(Key.Up)) server.Command[(int)Commands.MoveForward] = true;
                else server.Command[(int)Commands.MoveForward] = false;

                if (Keyboard.IsKeyDown(Key.Left)) server.Command[(int)Commands.RotateLeft] = true;
                else server.Command[(int)Commands.RotateLeft] = false;

                if (Keyboard.IsKeyDown(Key.Right)) server.Command[(int)Commands.RotateRight] = true;
                else server.Command[(int)Commands.RotateRight] = false;

                if (Keyboard.IsKeyDown(Key.RightCtrl)) server.Command[(int)Commands.Fire] = true;
                else server.Command[(int)Commands.Fire] = false;
            }
            #endregion
        }
        #endregion
        public enum Commands
        {
            MoveForward,
            Fire,
            RotateLeft,
            RotateRight
        }
    }
}
