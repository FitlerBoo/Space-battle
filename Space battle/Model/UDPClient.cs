using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Space_battle.Model
{
    internal class UDPClient
    {
        private readonly int localPort = 10101;
        private readonly int remotePort = 10100;

        private Player player;
        private Enemy enemy;
        private List<Bullet> bullets = new List<Bullet>();
        private List<EnemyBullet> eBullets = new List<EnemyBullet>();

        public byte[] Command { get; set; } = { 0b0, 0b0, 0b0, 0b0 };

        private UdpClient receiver;
        private UdpClient sender;
        private IPEndPoint remoteEndPoint;
        private int playersCounter = 0;


        private readonly Dispatcher _UIDispatcher;

        public UDPClient()
        {
            _UIDispatcher  = Dispatcher.CurrentDispatcher;

            receiver = new UdpClient(localPort);
            sender = new UdpClient();
        }

        public (Player, Enemy, List<Bullet>, List<EnemyBullet>) GetRenderedObjects()
        {
            return (player, enemy, bullets, eBullets);
        }
        #region Data Exchanging
        public void Receive()
        {
            try
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        var receivedData = receiver.Receive(ref remoteEndPoint);

                        await _UIDispatcher.InvokeAsync(new Action(()=>
                        {
                            ProcessData(receivedData);
                        }), DispatcherPriority.Render);
                    }
                });
            }
            catch (Exception) { }
        }

        public void Send()
        {
            try
            {
                Task.Run(async () =>
                {
                    sender.Connect(new IPEndPoint(IPAddress.Parse("192.168.43.139"), remotePort));
                    while (true)
                    {
                        var i = await sender.SendAsync(Command, Command.Length);
                        await Task.Delay(50);
                    }
                });
            }
            catch (Exception) { }
        }

        public void StartDataExchange()
        {
            Send();
            Receive();
        }
        #endregion
        #region Processing Data
        public void ProcessData(byte[] data)
        {
            bullets.Clear();
            eBullets.Clear();
            bool isEnemy, isRocket,
                inProcess = true;
            int currentIndex = 0;

            while (inProcess)
            {
                isEnemy = data[currentIndex++] > 0;
                isRocket = data[currentIndex++] > 0;
                inProcess = ProcessGameObject(data, ref currentIndex, isEnemy, isRocket);
            }
            playersCounter = 0;
        }

        private bool ProcessGameObject(byte[] data, ref int currentIndex, bool isEnemy, bool isRocket)
        {
            var x = DeserializeProperty(data, ref currentIndex);
            var y = DeserializeProperty(data, ref currentIndex);
            var angle = DeserializeProperty(data, ref currentIndex);
            double HP = -1;
            if (playersCounter < 2)
            {
                HP = DeserializeProperty(data, ref currentIndex);
                playersCounter++;
            }
            if (HP != -1)
                DistributeProperties(isEnemy, isRocket, x, y, angle, HP);
            else
                DistributeProperties(isEnemy, isRocket, x, y, angle);
            return currentIndex != data.Length;
        }

        private double DeserializeProperty(byte[] data, ref int currentIndex)
        {
            var length = data[currentIndex++];
            byte[] byteData = new byte[length];
            for (int i = 0; i < length; i++)
                byteData[i] = data[currentIndex++];
            var deserValue = BitConverter.ToDouble(byteData,0);
            return deserValue;
        }

        private void DistributeProperties(bool isEnemy, bool isRocket, double x, double y, double angle, [Optional] double HP)
        {
            if (isEnemy)
                
                DistributeEnemyProperties(isRocket, x, y, angle, HP);
            else
                DisrtibutePlayerProperties(isRocket, x, y, angle, HP);
        }
        
        private void DistributeEnemyProperties(bool isRocket, double x, double y, double angle, [Optional] double HP)
        {
            if (isRocket)
                enemy = new Enemy(x,y,angle,HP);
            else
                eBullets.Add(new EnemyBullet(x, y, angle));
        }

        private void DisrtibutePlayerProperties(bool isRocket, double x, double y, double angle, [Optional] double HP)
        {
            if (isRocket)
                player = new Player(x,y,angle,HP);
            else
                bullets.Add(new Bullet(x, y, angle));
        }
        #endregion
    }
}
