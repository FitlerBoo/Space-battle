using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Space_battle.Model
{
    internal class UDPClient
    {
        private readonly int localPort = 10101;
        private readonly int remotePort = 10100;

        private Player player1;
        private Player player2;

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

        public (Player, Player) GetRenderedObjects()
        {
            return (player1, player2);
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
                    //(IPAddress.Parse("192.168.43.139")
                    sender.Connect(IPAddress.Broadcast, remotePort);
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
        /// <summary>
        /// TODO: Вынести всё связанное с сериализацией/десериализацией в отдельный класс
        /// Здесь должна остаться только логика приёма/передачи данных, поддержка соединения
        /// </summary>
        /// <param name="data"></param>
        private void ProcessData(byte[] data)
        {
            int currentIndex = 0;
            DeserializePlayer(player1, data, ref currentIndex);
            DeserializePlayer(player2, data, ref currentIndex);
        }

        private void DeserializePlayer(Player player, byte[] data, ref int currentIndex)
        {
            var indicator = data[currentIndex];
            player.Bullets.Clear();
            player.Deserialize(data, ref currentIndex);
            while (data[currentIndex] == indicator)
            {
                var bullet = new Bullet(player.Style, 0, 0, 0);
                bullet.Deserialize(data, ref currentIndex);
                player1.Bullets.Enqueue(bullet);
            }
        }
        #endregion
    }
}
