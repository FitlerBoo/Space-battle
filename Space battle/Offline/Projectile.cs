using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Space_battle.Offline
{
    internal class Projectile : SingleGameObject
    {
        public Projectile(double x, double y, bool isEnemy, double angle)
            : base(x, y, isEnemy, angle)
        {
            _form = new Rectangle()
            {
                Height = 6,
                Width = 6,
                Fill = Brushes.White,
                Stroke = isEnemy ? Brushes.Yellow : Brushes.DarkRed
            };
        }
    }
}
