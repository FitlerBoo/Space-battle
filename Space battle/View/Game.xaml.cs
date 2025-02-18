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
        private bool isClient;
        private bool increaseP1Speed;
        private bool increaseP2Speed;
        Position Player1DefaultPos = new Position(370, 500, -9, 0);
        Position Player2DefaultPos = new Position(370, 40, 9, 0);
        #endregion

        #region Render Objects
        private Player player1;
        private Player player2;
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
        private bool fire;
        #endregion

        #region Net
        private UDPServer udpServer;
        private UDPClient udpClient;
        #endregion

        /// <summary>
        /// Является ли пользователь клиентом?
        /// </summary>
        /// <param name="isClient"></param>
        public Game(bool isClient)
        {
            this.isClient = isClient;
            this.DataContext = this;
            InitializeComponent();

            RenderStartScene(isClient);

            if (isClient)
            {
                udpClient = new UDPClient();
                udpClient.StartDataExchange();
            }
            else
            {
                udpServer = new UDPServer(player1, player2);
                udpServer.StartDataExchange();
            }

            gameTask = new Task(RenderTask); 
            
            gameTimer.Interval = TimeSpan.FromMilliseconds(15);

            MyCanvas.Focus();
        }

        private void RenderStartScene(bool isClient)
        {
            Canvas.SetRight(Player2HP, 0);
            AddPlayer(isClient);
            AddEnemy(isClient);
        }

        public void StartGame()
        {
            gameTask.Start();
        }

        private void RenderTask()
        {
            var renderTimer = Stopwatch.StartNew();
            while (player1.Health > 0 && player2.Health > 0)
            {

                if (renderTimer.ElapsedMilliseconds < 15)
                    continue;

                renderTimer.Restart();

                try
                {
                    if (isClient)
                        this.Dispatcher.Invoke(new Action(() => GameLoopClient()));
                        
                    else
                        this.Dispatcher.Invoke(new Action(() => GameLoopServer()));
                }
                catch (TaskCanceledException)
                {
                    this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                }
            }
            var winner = player1.Health > player2.Health ? "Player 1" : "Player 2";
            MessageBox.Show(winner + " wins!");
            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }

        #region Server part
        private void GameLoopServer()
        {
            MovePlayer();
            MoveEnemy();

            MoveBullets(player1.Bullets);
            MoveBullets(player2.Bullets);

            // TODO : Подумать как сделать неуязвимость на промежуток времени и анимацию.
            var playerImmune = CheckBulletsIntersectionsPlayer(player1, player2.Bullets);
            var enemyImmune = CheckBulletsIntersectionsPlayer(player2, player1.Bullets);
        }

        private bool CheckBulletsIntersectionsPlayer(Player playerObj, Queue<Bullet> bullets)
        {
            foreach (var bullet in bullets.ToArray())
                if (playerObj.HitBox.IntersectsWith(bullet.HitBox))
                {
                    var b = bullets.Dequeue();
                    MyCanvas.Children.Remove(b.Form);
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
                if (udpServer.Command[(int)Commands.MoveForward]) increaseP2Speed = true;
                else increaseP2Speed = false;

                player2.CalculateSpeed(increaseP2Speed);
                player2.Move();
            }
            else player2.MoveToPosition(Player2DefaultPos);
            if (udpServer.Command[(int)Commands.Fire]) MyCanvas.Children.Add(player2.MakeBullet().Form);
            if (udpServer.Command[(int)Commands.RotateLeft]) player2.RotateObject(true);
            if (udpServer.Command[(int)Commands.RotateRight]) player2.RotateObject(false);
        }
        private void MovePlayer()
        {
            if (CheckBorderCondition(player1))
            {
                player1.CalculateSpeed(increaseP1Speed);
                player1.Move();
            }
            else player1.MoveToPosition(Player1DefaultPos);
        }

        private void MoveBullets(Queue<Bullet> bullets)
        {
            foreach (var bullet in bullets)
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
         
        private void RenderScene(Player player1, Player player2)
        {
            foreach (var obj in itemRemover)
                MyCanvas.Children.Remove(obj);
            itemRemover.Clear();

            UpdateScore(player1, player2);
            AddObject(player1);
            AddObject(player2);

            if(player1.Bullets.Count > 0)
                foreach (var bullet in player1.Bullets) AddObject(bullet);
            if (player2.Bullets.Count > 0)
                foreach (var bullet in player2.Bullets) AddObject(bullet);
        }
        #endregion

        #region Common

        private void AddObject(GameObject gObj)
        {
            if (gObj == null) return;

            gObj.MakeVisualMovement();
            MyCanvas.Children.Add(gObj.Form);
            this.itemRemover.Enqueue(gObj.Form);
        }
        private bool CheckBorderCondition(GameObject obj)
        {
            return obj.Y > -40 && (obj.Y - 30) < applicationHeight &&
                (obj.X + 30) < applicationWidth && obj.X > -40;
        }
        private void UpdateScore(Player player1, Player player2)
        {
            if (player1 == null || player2 == null) return;
            Player1HP.Text = this.player1.MessageHealth;
            Player2HP.Text = player2.MessageHealth;
        }
        #endregion

        #region Game objects creation

        private void AddPlayer(bool isClient)
        {
            player1 = new Player(true,Player1DefaultPos);
            //if (isClient) return;
            MyCanvas.Children.Add(player1.Form);
            Canvas.SetZIndex(player1.Form, 1);
        }

        private void AddEnemy(bool isClient)
        {
            player2 = new Player(false,Player2DefaultPos);
            if (isClient) return;
            MyCanvas.Children.Add(player2.Form);
            Canvas.SetZIndex(player2.Form, 1);
        }
        #endregion

        #region Keys
        // TODO : Переписать методы со скоростью
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                moveForward = false;
                increaseP1Speed = false;
            }
            if (e.Key == Key.A) rotateLeft = false;
            if (e.Key == Key.D)
                rotateRight = false;
            if (e.Key == Key.Space) fire = false;
            #region moveBack
            //if (e.Key == Key.D)
            //{
            //    moveBack = false;
            //}
            #endregion
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                moveForward = true;
                increaseP1Speed = true;
                //moveBack = false;
            }

            #region moveBack
            //if (e.Key == Key.S)
            //{
            //    //moveBack = true;
            //    //moveForward = false;
            //}
            #endregion

            if (e.Key == Key.D && !isClient) player1.RotateObject(false);
            else if (e.Key == Key.D && isClient) rotateRight = true;

            if (e.Key == Key.A && !isClient) player1.RotateObject(true);
            else if (e.Key == Key.A && isClient) rotateLeft = true;

            if (e.Key == Key.Space && !isClient) MyCanvas.Children.Add(player1.MakeBullet().Form);
            else if (e.Key == Key.Space && isClient) fire = true;
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
