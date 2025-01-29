using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class EnemyBullet : GameObject
    {
        private double PointX => 800 - X - Form.Width; 
        private double PointY => 600 - Y - Form.Height; 
        public new Rect HitBox => new Rect(PointX, PointY, Form.Width, Form.Height);
        public EnemyBullet(Enemy enemy)
        {
            SetForm();
            angleStartPosition = 90;
            Speed = 40;
            Angle = enemy.Angle;
            X = enemy.X + (enemy.Form.Width / 2.0);
            Y = enemy.Y + (enemy.Form.Height / 2.0);
            Transform();
        }

        public EnemyBullet(double x, double y, double angle)
        {
            SetForm();
            Angle = angle;
            X = x;
            Y = y;
            Transform();
        }
        private void SetForm()
        {
            Form = new Rectangle()
            {
                Height = 6,
                Width = 6,
                Fill = Brushes.White,
                Stroke = Brushes.Yellow
            };
        }
    }
}
