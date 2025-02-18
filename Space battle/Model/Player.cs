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

        protected bool _isFirstPlayerStyle;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stile">true = red style / false = yellow style</param>
        /// <param name="pos"></param>
        /// <param name="health"></param>
        public Player(bool stile, Position pos, double health = 100)
            :base(pos)
        {
            _isFirstPlayerStyle = stile;
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
            Bullet bullet = new Bullet(_isFirstPlayerStyle, _position.X + _form.Width / 2.0, _position.Y + _form.Height / 2.0, _position.MovementAngle);
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
    }
}
