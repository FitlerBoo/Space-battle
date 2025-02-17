using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class Player : GameObject
    {
        private double _health = 100;
        private Queue<Bullet> _bullets = new Queue<Bullet>();
        public new Rect HitBox => new Rect((_position.X + 15), (_position.Y + 15), 30, 30); 
        public string ShowHealth() => string.Format("{0} {1}", _isFirstPlayer ? "Player 1: " : "Player 2: ", _health);
        public double GetHealth() => _health;
        public Queue<Bullet> GetBullets() => _bullets;


        public Player(bool isFirstPlayer)
        : base(isFirstPlayer) { }

        public Player(bool isFirstPlayer, double x, double y, double angle, double health)
            :base(isFirstPlayer,x,y,angle)
        {
            _health = health;
        }


        protected override void SetForm()
        {
            var path = _isFirstPlayer ? @"..\..\Images\vehicleU.png" : @"..\..\Images\vehicle2U.png";
            _form = new Rectangle()
            {
                Width = 60,
                Height = 60,
                Fill = new ImageBrush(new BitmapImage(new Uri(path, UriKind.Relative)))
            };
        }

        public Bullet MakeBullet()
        {
            Bullet bullet = new Bullet(_isFirstPlayer, _position.X + _form.Width / 2.0, _position.Y + _form.Height / 2.0, _position.MovementAngle);
            _bullets.Enqueue(bullet);
            return bullet;
        }

        public void TakeDamage()
        {
            if (_health > 0)
                _health -= 10;
        }
    }
}
