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
        private Player player;
        private Enemy enemy;
        private DispatcherTimer gameTimer = new DispatcherTimer();

        private List<Rectangle> itemRemover = new List<Rectangle>();
        private List<Bullet> bullets = new List<Bullet>();
        private List<EnemyBullet> eBullets = new List<EnemyBullet>();
        private bool fl = false;

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
                udpServer = new UDPServer(player, enemy, bullets, eBullets);
                udpServer.StartDataExchange();
            }
                
            

            gameTask = new Task(RenderTask); 
            
            gameTimer.Interval = TimeSpan.FromMilliseconds(15);

            MyCanvas.Focus();
        }

        private void RenderStartScene(bool isClient)
        {
            Canvas.SetRight(EnemyHP, 0);
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
            while (player.HP > 0 && enemy.HP > 0)
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
            var winner = player.HP > enemy.HP ? "Player" : "Enemy";
            MessageBox.Show(winner + " wins!");
            this.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        }

        //*************************************************************** Server part ******************************************************************************
        private void GameLoopServer()
        {
            player.CalculateSpeed(changeSpeedPlayer);
            enemy.CalculateSpeed(changeSpeedEnemy);

            MovePlayer();
            MoveEnemy();

            MoveBullets(bullets, isEnemy : false);
            MoveBullets(eBullets, isEnemy : true);

            // TODO : Подумать как сделать неуязвимость на промежуток времени и анимацию.
            var playerImmune = CheckBulletsIntersectionsPlayer(player, eBullets);
            var enemyImmune = CheckBulletsIntersectionsEnemy(enemy, bullets);

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
                    MyCanvas.Children.Remove(bullet.Form);
                    bullets.Remove(bullet);
                    playerObj.HP -= 10;
                    EnemyHP.Text = playerObj.StringHP;
                    return true;
                }
            return false;
        }

        private void MoveEnemy()
        {
            var inBorder = CheckBorderCondition(enemy);
            if (inBorder)
            {
                if (udpServer.Command[0])
                {
                    changeSpeedEnemy = true;
                    enemy.Move();
                    Canvas.SetBottom(enemy.Form, enemy.Y);
                    Canvas.SetRight(enemy.Form, enemy.X);
                }
                else changeSpeedEnemy = false;
            }
            else enemy.Reset();
            if (udpServer.Command[1]) CreateEnemyBullet();
            if (udpServer.Command[2]) enemy.RotateForm(true);
            if (udpServer.Command[3]) enemy.RotateForm(false);
        }
        private void MovePlayer()
        {
            var inBorder = CheckBorderCondition(player);
            if (inBorder)
            {
                if (moveForward || player.Speed > 0)
                {
                    player.Move();
                    Canvas.SetTop(player.Form, player.Y);
                    Canvas.SetLeft(player.Form, player.X);
                }
            }
            else player.Reset();
        }

        private void MoveBullets(IEnumerable<GameObject> bullets, bool isEnemy)
        {
            foreach (var bullet in bullets)
            {
                if (MyCanvas.Children.Contains(bullet.Form))
                {
                    var inBorder = CheckBorderCondition(bullet);
                    if (!inBorder)  itemRemover.Add(bullet.Form);
                    else
                    {
                        bullet.Move();
                        if (isEnemy)
                        {
                            Canvas.SetBottom(bullet.Form, bullet.Y);
                            Canvas.SetRight(bullet.Form, bullet.X);
                        }
                        else
                        {
                            Canvas.SetTop(bullet.Form, bullet.Y);
                            Canvas.SetLeft(bullet.Form, bullet.X);
                        }
                    }
                }
            }
        }

        //*************************************************************** Clietn part ******************************************************************************
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
                Canvas.SetRight(gObj.Form, gObj.X);
                Canvas.SetBottom(gObj.Form, gObj.Y);
            }
            else
            {
                Canvas.SetLeft(gObj.Form, gObj.X);
                Canvas.SetTop(gObj.Form, gObj.Y);
            }
            MyCanvas.Children.Add(gObj.Form);
            this.itemRemover.Add(gObj.Form);
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
            EnemyHP.Text = enemy.StringHP;
        }

        #region Game objects creation
        private void CreateBullet()
        {
            Bullet bullet = player.MakeBullet();
            Canvas.SetLeft(bullet.Form, bullet.X);
            Canvas.SetTop(bullet.Form, bullet.Y);

            MyCanvas.Children.Add(bullet.Form);
            bullets.Add(bullet);
        }
        // TODO : Подумай как обобщить эти методы без костылей
        private void CreateEnemyBullet()
        {
            EnemyBullet bullet = new EnemyBullet(enemy);

            Canvas.SetRight(bullet.Form, bullet.X);
            Canvas.SetBottom(bullet.Form, bullet.Y);

            MyCanvas.Children.Add(bullet.Form);
            eBullets.Add(bullet);
        }

        private void AddPlayer(bool isClient)
        {
            player = new Player();
            player.Reset();
            if (isClient) return;
            MyCanvas.Children.Add(player.Form);
            Canvas.SetZIndex(player.Form, 1);
            Canvas.SetTop(player.Form, player.PlayerStartY);
            Canvas.SetLeft(player.Form, player.PlayerStartX);
        }

        private void AddEnemy(bool isClient)
        {
            enemy = new Enemy();
            if (isClient) return;
            MyCanvas.Children.Add(enemy.Form);
            Canvas.SetZIndex(enemy.Form, 1);
            Canvas.SetBottom(enemy.Form, enemy.PlayerStartY);
            Canvas.SetRight(enemy.Form, enemy.PlayerStartX);
            enemy.Reset();
            enemy.Transform();
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

            if (e.Key == Key.D && !isClient) player.RotateForm(false);
            else if (e.Key == Key.D && isClient) rotateRight = true;

            if (e.Key == Key.A && !isClient) player.RotateForm(true);
            else if (e.Key == Key.A && isClient) rotateLeft = true;

            if (e.Key == Key.Space && !isClient) CreateBullet();
            else if (e.Key == Key.Space && isClient) fire = true;
        }
        #endregion
    }
}
