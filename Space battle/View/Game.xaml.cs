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

        #endregion
        #region Render Objects
        private Player player1;
        private Player player2;
        private DispatcherTimer gameTimer = new DispatcherTimer();

        private List<Rectangle> itemRemover = new List<Rectangle>();

        private Task gameTask;
        #endregion
        #region Commands
        private const byte trueCommand = 0b1;
        private const byte falseCommand = 0b0;
        private bool rotateRight;
        private bool rotateLeft;
        private bool moveForward;
        private bool fire;
        #endregion
        #region Net
        private UDPServer udpServer;
        private UDPClient udpClient;
        private bool isClient;
        private bool changeSpeedPlayer;
        private bool changeSpeedEnemy;
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
            while (player1.GetHealth() > 0 && player2.GetHealth() > 0)
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
            var winner = player1.GetHealth() > player2.GetHealth() ? "Player 1" : "Player 2";
            MessageBox.Show(winner + " wins!");
            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }

        //*************************************************************** Server part ******************************************************************************
        private void GameLoopServer()
        {
            player1.CalculateSpeed(changeSpeedPlayer);
            player2.CalculateSpeed(changeSpeedEnemy);

            MovePlayer();
            MoveEnemy();

            MoveBullets(bullets, isEnemy : false);
            MoveBullets(eBullets, isEnemy : true);

            // TODO : Подумать как сделать неуязвимость на промежуток времени и анимацию.
            var playerImmune = CheckBulletsIntersectionsPlayer(player1, eBullets);
            var enemyImmune = CheckBulletsIntersectionsEnemy(player2, bullets);

            ClearBullets();
        }

        private void ClearBullets()
        {
            foreach (var bullet in itemRemover)
                MyCanvas.Children.Remove(bullet);

            if (itemRemover.Count > 0)
                itemRemover.Clear();
        }

        private bool CheckBulletsIntersectionsPlayer(Player playerObj, List<EnemyBullet> bullets)
        {
            foreach (var bullet in bullets.ToArray())
                if (playerObj.HitBox.IntersectsWith(bullet.HitBox))
                {
                    MyCanvas.Children.Remove(bullet.Form);
                    itemRemover.Add(bullet.Form);
                    bullets.Remove(bullet);
                    playerObj.HP -= 10;
                    PlayerHP.Text = playerObj.StringHP;
                    return true;
                }
            return false;
        }

        private bool CheckBulletsIntersectionsEnemy(Enemy playerObj, List<Bullet> bullets)
        {
            foreach (var bullet in bullets.ToArray())
                if (playerObj.HitBox.IntersectsWith(bullet.HitBox))
                {
                    MyCanvas.Children.Remove(bullet._form);
                    bullets.Remove(bullet);
                    playerObj.HP -= 10;
                    Player2HP.Text = playerObj.StringHP;
                    return true;
                }
            return false;
        }

        private void MoveEnemy()
        {
            /// TODO: вместо 1,2,3 сделать enum {Create = 1, RotateLeft = 2, RotateRight = 3}
            var inBorder = CheckBorderCondition(player2);
            if (inBorder)
            {
                if (udpServer.Command[(int)Commands.MoveForward])
                {
                    changeSpeedEnemy = true;
                    player2.Move();
                    Canvas.SetBottom(player2.Form, player2.Y);
                    Canvas.SetRight(player2.Form, player2.X);
                }
                else changeSpeedEnemy = false;
            }
            else player2.Reset();
            if (udpServer.Command[(int)Commands.Fire]) CreateEnemyBullet();
            if (udpServer.Command[(int)Commands.RotateLeft]) player2.RotateForm(true);
            if (udpServer.Command[(int)Commands.RotateRight]) player2.RotateForm(false);
        }
        private void MovePlayer()
        {
            var inBorder = CheckBorderCondition(player1);
            if (inBorder)
            {
                if (moveForward || player1.Speed > 0)
                {
                    player1.Move();
                    Canvas.SetTop(player1._form, player1.Y);
                    Canvas.SetLeft(player1._form, player1.X);
                }
            }
            else player1.Reset();
        }

        private void MoveBullets(IEnumerable<GameObject> bullets, bool isEnemy)
        {
            foreach (var bullet in bullets)
            {
                if (MyCanvas.Children.Contains(bullet._form))
                {
                    var inBorder = CheckBorderCondition(bullet);
                    if (!inBorder)  itemRemover.Add(bullet._form);
                    else
                    {
                        bullet.Move();
                        if (isEnemy)
                        {
                            Canvas.SetBottom(bullet._form, bullet.Y);
                            Canvas.SetRight(bullet._form, bullet.X);
                        }
                        else
                        {
                            Canvas.SetTop(bullet._form, bullet.Y);
                            Canvas.SetLeft(bullet._form, bullet.X);
                        }
                    }
                }
            }
        }


        /// TODO: Вот так делать не стоит, если хочется отделить кусок кода,
        /// то скорее всего ему тут не место, и нужно выделить его в отдельны класс.
        /// На крайняк есть region <summary> 
        
        //*************************************************************** Client part ******************************************************************************
        private void GameLoopClient()
        {
            udpClient.Command = MakeCommand();
            var objects = udpClient.GetRenderedObjects();
            RenderScene(player : objects.Item1,
                enemy : objects.Item2,
                bullets : objects.Item3,
                eBullets : objects.Item4);
        }

        private byte[] MakeCommand()
        {
            var commands = new bool[] { moveForward, fire, rotateLeft, rotateRight };
            var resultCommand = new byte[4];
            for (int i = 0; i < commands.Length; i++)
                resultCommand[i] = commands[i] ? trueCommand : falseCommand;
            return resultCommand;
        }
         
        private void RenderScene(Player player, Enemy enemy, List<Bullet> bullets, List<EnemyBullet> eBullets)
        {
            foreach (var obj in itemRemover)
                MyCanvas.Children.Remove(obj);
            itemRemover.Clear();

            UpdateScore(player, enemy);
            AddObject(player, false);
            AddObject(enemy, true);

            if(bullets != null)
                foreach (var bullet in bullets) AddObject(bullet, false);
            if (eBullets != null)
                foreach (var eBullet in eBullets) AddObject(eBullet, true);
        }

        //****************************************************************** Common ******************************************************************************

        private void AddObject(GameObject gObj, bool isEnemy)
        {
            if (gObj == null) return;
            if (isEnemy)
            {
                Canvas.SetRight(gObj._form, gObj.X);
                Canvas.SetBottom(gObj._form, gObj.Y);
            }
            else
            {
                Canvas.SetLeft(gObj._form, gObj.X);
                Canvas.SetTop(gObj._form, gObj.Y);
            }
            MyCanvas.Children.Add(gObj._form);
            this.itemRemover.Add(gObj._form);
        }
        private bool CheckBorderCondition(GameObject obj)
        {
            return obj.Y > -40 && (obj.Y - 30) < applicationHeight &&
                (obj.X + 30) < applicationWidth && obj.X > -40;
        }
        private void UpdateScore(Player player, Enemy enemy)
        {
            if (player == null) return;
            PlayerHP.Text = player.StringHP;
            Player2HP.Text = enemy.StringHP;
        }

        #region Game objects creation
        private void CreateBullet()
        {
            Bullet bullet = player1.MakeBullet();
            Canvas.SetLeft(bullet._form, bullet.X);
            Canvas.SetTop(bullet._form, bullet.Y);

            MyCanvas.Children.Add(bullet._form);
            bullets.Add(bullet);
        }
        // TODO : Подумай как обобщить эти методы без костылей
        private void CreateEnemyBullet()
        {
            EnemyBullet bullet = new EnemyBullet(player2);

            Canvas.SetRight(bullet.Form, bullet.X);
            Canvas.SetBottom(bullet.Form, bullet.Y);

            MyCanvas.Children.Add(bullet.Form);
            eBullets.Add(bullet);
        }

        private void AddPlayer(bool isClient)
        {
            player1 = new Player();
            player1.Reset();
            if (isClient) return;
            MyCanvas.Children.Add(player1._form);
            Canvas.SetZIndex(player1._form, 1);
            Canvas.SetTop(player1._form, player1.PlayerStartY);
            Canvas.SetLeft(player1._form, player1.PlayerStartX);
        }

        private void AddEnemy(bool isClient)
        {
            player2 = new Enemy();
            if (isClient) return;
            MyCanvas.Children.Add(player2.Form);
            Canvas.SetZIndex(player2.Form, 1);
            Canvas.SetBottom(player2.Form, player2.PlayerStartY);
            Canvas.SetRight(player2.Form, player2.PlayerStartX);
            player2.Reset();
            player2.Transform();
        }
        #endregion

        #region Keys
        // TODO : Переписать методы со скоростью
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
            {
                moveForward = false;
                changeSpeedPlayer = false;
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
                changeSpeedPlayer = true;
                //moveBack = false;
            }

            #region moveBack
            //if (e.Key == Key.S)
            //{
            //    //moveBack = true;
            //    //moveForward = false;
            //}
            #endregion

            if (e.Key == Key.D && !isClient) player1.RotateForm(false);
            else if (e.Key == Key.D && isClient) rotateRight = true;

            if (e.Key == Key.A && !isClient) player1.RotateForm(true);
            else if (e.Key == Key.A && isClient) rotateLeft = true;

            if (e.Key == Key.Space && !isClient) CreateBullet();
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
