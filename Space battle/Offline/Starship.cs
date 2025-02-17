using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Space_battle.Offline
{
    internal class Starship : SingleGameObject
    {
        private int _health = 100;
        private Queue<Projectile> projectiles = new Queue<Projectile>();
        public string ShowHealth() => string.Format("{0} {1}", _isSecondPlayer ? "Player 2: " : "Player 1: ", _health);
        public void ResetPlayerStartPosition() => _location.SetPlayerToStartPosition(_isSecondPlayer);
        public new Rect HitBox => new Rect((_location.X + 15), (_location.Y + 15), 30, 30);
        public Queue<Projectile> GetProjectiles() => projectiles;
        public int GetHealth() => _health;

        public Starship(bool isSecondPlayer)
            : base(isSecondPlayer)
        {
            var path = isSecondPlayer ? @"..\..\Images\vehicle2U.png" : @"..\..\Images\vehicleU.png";
            _form = new Rectangle()
            {
                Width = 60,
                Height = 60,
                Fill = new ImageBrush(new BitmapImage(new Uri(path, UriKind.Relative)))
            };
        }

        public Projectile MakeProjectile()
        {
            var projectile = new Projectile(_location.X, _location.Y, _isSecondPlayer, _location.MovementAngle);
            projectiles.Enqueue(projectile);
            return projectile;
        }

        public void TakeDamage()
        {
            if (_health > 0)
                _health -= 10;
        }

        // TODO: доделать
        
    }
}
