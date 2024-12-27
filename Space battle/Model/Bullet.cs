using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class Bullet : GameObject
    {

         public int OwnerId { get; set; }

        public Bullet(GameObject player)
        {
            SetForm();
            Speed = 40;
            angleStartPosition = -90;
            Angle = player.Angle;
            X = player.X + (player.Form.Width / 2.0);
            Y = player.Y + (player.Form.Height / 2.0);
            Transform();
        }

        public Bullet(double x, double y, double angle)
        {
            SetForm();
            X = x;
            Y = y;
            Angle = angle;
            Transform();
        }

        private void SetForm()
        {
            Form = new Rectangle()
            {
                Height = 6,
                Width = 6,
                Fill = Brushes.White,
                Stroke = Brushes.DarkRed
            };
        }
    }
}
