using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Space_battle.Model
{
    abstract class GameObject
    {
        public Rectangle Form { get; set; }
        public Rect HitBox
        {
            get
            {
                return new Rect(X, Y, Form.Width, Form.Height);
            }
        }
        public double Speed { get; set; }
        public double Angle { get; set; }
        public readonly int AngleValue  = 10;
        public double X { get; set; }
        public double Y { get; set; }
        public int angleStartPosition;
        public void Transform()
        {
            Form.RenderTransform = new RotateTransform(-AngleValue * (Angle % 36),
                    Form.Width / 2,
                    Form.Height / 2);
        }
        public void Move()
        {
            X -= (Speed * Math.Cos((AngleValue * Angle + angleStartPosition) * Math.PI / 180));
            Y += (Speed * Math.Sin((AngleValue * Angle + angleStartPosition) * Math.PI / 180));
        }
    }
}
