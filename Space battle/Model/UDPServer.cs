using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;

namespace Space_battle.Model
{
    internal class UDPServer
    {
        private readonly int localPort = 10100;
        private readonly int remotePort = 10101;
        #region Byte Indicators
        private readonly byte playerIndicator = 0b1;
        private readonly byte enemyIndicator = 0b0;
        private readonly byte rocketIndicator = 0b1;
        private readonly byte bulletIndicator = 0b0;
        #endregion
        private Player player;
        private Enemy enemy;
        private List<Bullet> bullets;
        private List<EnemyBullet> eBullets;
        /// <summary>
        /// bytes : 0 - moveForward; 1 - fire; 2 - rotateLeft; 3 - rotateRight
        /// </summary>
        public bool[] Command { get; set; } = { false, false, false, false };

        UdpClient receiver;
        UdpClient sender;
        private IPEndPoint remoteEndPoint;
        FileStream fs;
        StreamWriter sw;


        public UDPServer(Player player, Enemy enemy, List<Bullet> bullets, List<EnemyBullet> eBullets)
        {
            this.player = player;
            this.enemy = enemy;
            this.bullets = bullets;
            this.eBullets = eBullets;
            Initialize();
        }

        private void Initialize()
        {
            receiver = new UdpClient(localPort);
            sender = new UdpClient();
        }

        #region Data Exchanging
        private void Send()
        {
            Task.Run(async () =>
            {
                try
                {
                    sender.Connect(IPAddress.Broadcast, remotePort);
                    throw new NullReferenceException();
                }
                catch (Exception e)
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "err.txt"), e.Message);
                    if (e.InnerException != null)
                        File.WriteAllText("err.txt", e.InnerException.Message);
                }
                while (true)
                {
                    var dgram = PrepareData();
                    var i = await sender.SendAsync(dgram, dgram.Length);
                    await Task.Delay(15);
                }
            });
        }

        private void Receive()
        {
            try
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        var receivedData = receiver.Receive(ref remoteEndPoint);

                        ProcessData(receivedData);
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
        #region Data Preparation
        private byte[] PrepareData()
        {
            List<byte> result = new List<byte>();
            result.AddRange(PrepareGameObjectData(player, isPlayer: true, isRocket: true));
            result.AddRange(AddHpData(player.HP));
            result.AddRange(PrepareGameObjectData(enemy, isPlayer: false, isRocket: true));
            result.AddRange(AddHpData(enemy.HP));
            result.AddRange(PrepareBulletsData(bullets, isPlayer: true));
            result.AddRange(PrepareBulletsData(eBullets, isPlayer: false));

            return result.ToArray();
        }

        private List<byte> PrepareGameObjectData(GameObject gobj, bool isPlayer, bool isRocket)
        {
            List<byte> data = new List<byte>();
            data.Add(isPlayer ? playerIndicator : enemyIndicator);
            data.Add(isRocket ? rocketIndicator : bulletIndicator);
            SerializeAndAddProperty(gobj.X, ref data);
            SerializeAndAddProperty(gobj.Y, ref data);
            SerializeAndAddProperty(gobj.Angle, ref data);

            return data;
        }

        private void SerializeAndAddProperty(double value, ref List<byte> data)
        {
            var valueByte = BitConverter.GetBytes(value);
            data.Add((byte)valueByte.Length);
            data.AddRange(valueByte);
        }

        private List<byte> PrepareBulletsData(IEnumerable<GameObject> gobj, bool isPlayer)
        {
            List<byte> result = new List<byte>();
            foreach (var item in gobj.ToArray())
            {
                result.AddRange(PrepareGameObjectData(item, isPlayer, false));
            }
            return result;
        }

        private byte[] AddHpData(double HP)
        {
            var list = new List<byte>();
            var data = BitConverter.GetBytes(HP);
            list.Add((byte)data.Length);
            list.AddRange(data);
            return list.ToArray();
        }
        #endregion
        #region Data Processing
        /// <summary>
        /// bytes : 0. moveForward, 1. fire, 2. rotateLeft,  3. rotateRight
        /// </summary>
        /// <param name="data"></param>
        private void ProcessData(byte[] data)
        {
            Command = new bool[data.Length];
            for (int i = 0; i < data.Length; i++)
                Command[i] = data[i] > 0;
        }


        #endregion
    }
}
