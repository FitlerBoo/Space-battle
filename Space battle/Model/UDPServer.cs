using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Spaceship player1;
        private Spaceship player2;
        /// <summary>
        /// bytes : 0 - moveForward; 1 - fire; 2 - rotateLeft; 3 - rotateRight
        /// </summary>
        public bool[] Command { get; set; } = { false, false, false, false };

        UdpClient receiver;
        UdpClient sender;
        private IPEndPoint remoteEndPoint;


        public UDPServer(Spaceship player1, Spaceship player2)
        {
            this.player1 = player1;
            this.player2 = player2;
            Initialize();
        }

        private void Initialize()
        {
            receiver = new UdpClient(localPort);
            sender = new UdpClient();
        }

        #region Data Exchanging
        protected void Send()
        {
            Task.Run(async () =>
            {
                using (MemoryStream memoryStream = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
                    {
                        try
                        {
                            sender.Connect(IPAddress.Broadcast, remotePort);
                        }
                        catch (Exception e)
                        {
                            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "err.txt"), e.Message);
                            if (e.InnerException != null)
                                File.WriteAllText("err.txt", e.InnerException.Message);
                        }
                        while (true)
                        {
                            byte[] dgram = PrepareData(writer);
                            var i = await sender.SendAsync(dgram, dgram.Length);
                            memoryStream.SetLength(0);
                            await Task.Delay(15);
                        }
                    }
            });
        }

        protected void Receive()
        {
            try
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        var receivedData = receiver.Receive(ref remoteEndPoint);

                        GetCommand(receivedData);
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

        /// <summary>
        /// TODO: Вынести всё связанное с сереализацие/десериализацией в отдельный класс
        /// Здесть должна остаться только логика приёма/перадачи данных, поддержка соединения
        private byte[] PrepareData(BinaryWriter writer)
        {
            player1.Serialize(writer);
            player2.Serialize(writer);
            return ((MemoryStream)writer.BaseStream).ToArray();
        }
        #endregion
        #region Data Processing
        /// <summary>
        /// bytes : 0. moveForward, 1. fire, 2. rotateLeft,  3. rotateRight
        /// </summary>
        /// <param name="data"></param>
        private void GetCommand(byte[] data)
        {
            Command = new bool[data.Length];
            for (int i = 0; i < data.Length; i++)
                Command[i] = data[i] > 0;
        }
        #endregion
    }
}
