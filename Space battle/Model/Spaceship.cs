using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

namespace Space_battle.Model
{
    internal class Spaceship : GameObject
    {
        private double _health;
        private Queue<Bullet> _bullets = new Queue<Bullet>();
        public new Rect HitBox => new Rect((_position.X + 15), (_position.Y + 15), 30, 30);
        public string MessageHealth => string.Format("Player {0} HP : {1}", (int)_style, _health);
        public double Health => _health;
        public Queue<Bullet> Bullets => _bullets;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stile">true = red style / false = yellow style</param>
        /// <param name="pos"></param>
        /// <param name="health"></param>
        public Spaceship(GameObjectStyle style, GameObjectType type, Position pos, double health = 100)
            :base(pos, type, style)
        {
            _health = health;
        }

        public Spaceship(BinaryReader br)
        { 
            Deserialize(br);
        }
        protected override void SetForm()
        {
            var path = SelectPath(_style);
            _form = new Rectangle()
            {
                Width = 60,
                Height = 60,
                Fill = new ImageBrush(new BitmapImage(new Uri(path, UriKind.Relative)))
            };
        }

        public Bullet MakeBullet()
        {
            Bullet bullet = new Bullet(GameObjectType.Bullet, _style,
                _position.X + Form.Width / 2,
                _position.Y + Form.Height / 2,
                _position.MovementAngle);
            AddBullet(bullet);
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

        private string SelectPath(GameObjectStyle style)
        {
            switch (style)
            {
                case GameObjectStyle.Red:
                    return @"..\..\Images\vehicleU.png";
                case GameObjectStyle.Yellow:
                    return @"..\..\Images\vehicle2U.png";
                default:
                    return "";
            }
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.WriteGameObjectType(_type);
            bw.WriteGameObjectStyle(_style);
            bw.WritePosition(_position);
            bw.WriteHealth(_health);
            bw.WriteBulletsCount(_bullets.Count);
            foreach (Bullet bullet in _bullets.ToArray())
                bullet.Serialize(bw);
        }

        public override void Deserialize(BinaryReader br)
        {
            _type = br.ReadGameObjectType();
            _style = br.ReadGameObjectStyle();
            SetForm();
            MoveToPosition(br.ReadPosition());
            _health = br.ReadHealth();
        }
    }
}
