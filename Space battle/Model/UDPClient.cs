using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Space_battle.Model
{
    internal class UDPClient
    {
        private readonly int localPort = 10101;
        private readonly int remotePort = 10100;

        private Spaceship player1;
        private Spaceship player2;

        public byte[] Command { get; set; } = { 0b0, 0b0, 0b0, 0b0 };

        private UdpClient receiver;
        private UdpClient sender;
        private IPEndPoint remoteEndPoint;


        private readonly Dispatcher _UIDispatcher;

        public UDPClient()
        {
            _UIDispatcher  = Dispatcher.CurrentDispatcher;

            receiver = new UdpClient(localPort);
            sender = new UdpClient();
        }

        public (Spaceship, Spaceship) GetRenderedObjects()
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
                    using (var memoryStream = new MemoryStream())
                        using (var reader = new BinaryReader(memoryStream, Encoding.UTF8, true))
                        {
                            while (true)
                            {
                                var receivedData = receiver.Receive(ref remoteEndPoint);
                                memoryStream.Write(receivedData, 0, receivedData.Length);
                                memoryStream.Position = 0;

                                await _UIDispatcher.InvokeAsync(new Action(() =>
                                {
                                    ProcessData(reader);
                                    memoryStream.SetLength(0);
                                }), DispatcherPriority.Render);
                            }
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
                    sender.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), remotePort));
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
        private void ProcessData(BinaryReader br)
        {
            player1 = ProcessPlayer(br);
            player2 = ProcessPlayer(br);
        }

        private Spaceship ProcessPlayer(BinaryReader br)
        {
            var player = new Spaceship(br);
            var bulletsCount = br.ReadBulletsCount();
            for (int i = 0; i < bulletsCount; i++)
                player.AddBullet(new Bullet(br));
            return player;
        }
        #endregion
    }
}
