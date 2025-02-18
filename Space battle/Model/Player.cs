using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class Player : GameObject
    {
        private double _health;
        private Queue<Bullet> _bullets = new Queue<Bullet>();
        public new Rect HitBox => new Rect((_position.X + 15), (_position.Y + 15), 30, 30); 
        public string MessageHealth => string.Format("{0} {1}", _isFirstPlayerStyle ? "Player 1: " : "Player 2: ", _health);
        public double Health => _health;
        public Queue<Bullet> Bullets => _bullets;
        public bool Style => _isFirstPlayerStyle;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stile">true = red style / false = yellow style</param>
        /// <param name="pos"></param>
        /// <param name="health"></param>
        public Player(bool style, Position pos, double health = 100)
            :base(pos, style)
        {
            _health = health;
        }

        protected override void SetForm()
        {
            var path = _isFirstPlayerStyle ? @"..\..\Images\vehicleU.png" : @"..\..\Images\vehicle2U.png";
            _form = new Rectangle()
            {
                Width = 60,
                Height = 60,
                Fill = new ImageBrush(new BitmapImage(new Uri(path, UriKind.Relative)))
            };
        }

        public Bullet MakeBullet()
        {
            Bullet bullet = new Bullet(_isFirstPlayerStyle, _position.X + Form.Width / 2, _position.Y + Form.Height / 2, _position.MovementAngle);
            _bullets.Enqueue(bullet);
            return bullet;
        }

        public void AddBullet(Bullet bullet)
        {
            _bullets.Enqueue(bullet);
        }

        public void TakeDamage()
        {
            if (_health > 0)
                _health -= 10;
        }

        public void CalculateSpeed(bool increase)
        {
            _position.CalculatePlayerMovementSpeed(increase);
        }

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.Add(_isFirstPlayerStyle ? (byte)0 : (byte)1);
            SerializeAndAddProperty(X, ref data);
            SerializeAndAddProperty(Y, ref data);
            SerializeAndAddProperty(Angle, ref data);
            SerializeAndAddProperty(Health, ref data);
            foreach (Bullet bullet in _bullets)
                data.AddRange(bullet.Serialize());
            return data.ToArray();
        }

        public override void Deserialize(byte[] data, ref int currentIndex)
        {
            var currentIndicator = data[currentIndex++];
            var x = DeserializeProperty(data, ref currentIndex);
            var y = DeserializeProperty(data, ref currentIndex);
            var angle = DeserializeProperty(data, ref currentIndex);
            _health = DeserializeProperty(data, ref currentIndex);
            MoveToPosition(new Position(x, y, angle));
        }
    }
}
