using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Runtime.InteropServices;

namespace Space_battle.Model
{
    internal class Enemy : UserGameObject
    {
        private readonly double hitBoxSize = 30;
        private double PointX => 800 - X - Form.Width + 15; 
        private double PointY =>  600 - Y - Form.Height + 15; 
        public new Rect HitBox =>  new Rect(PointX, PointY, hitBoxSize, hitBoxSize); 
        public string StringHP => "Enemy HP : " + HP; 
        private double StartAngle { get; } = 18;

        public Enemy(double x, double y, double angle, double HP)
        {
            SetForm();
            X = x;
            Y = y;
            Angle = angle + StartAngle;
            this.HP = HP;
            Transform();
        }

        public Enemy()
        {
            SetForm();
            angleStartPosition = 90;
            Angle = StartAngle;
        }

        private void SetForm()
        {
            Form = new Rectangle()
            {
                Width = 60,
                Height = 60,
                Fill = new ImageBrush(new BitmapImage(new Uri(@"..\..\Images\vehicle2U.png", UriKind.Relative)))
            };
        }

        
    }
}
