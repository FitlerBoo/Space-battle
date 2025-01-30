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
        /// <summary>
        /// TODO: почему доступно на запись извне класса?
        /// Или   public Rectangle Form { get;  }
        /// Или   public Rectangle Form { get; protected set; }
        /// ***
        /// Форма требуется для добавления ее на Canvas
        /// </summary>
        public Rectangle Form { get; set; }
        public Rect HitBox => new Rect(X, Y, Form.Width, Form.Height); 
        public double Speed { get; set; }
        public double Angle { get; set; }
        public readonly int AngleValue  = 10;

        /// <summary>
        /// TODO: Логичнее ввести отдельную структуру Position(x,y)
        /// и далее все что связанно с изменение координат выполнять там
        /// TODO: Координаты не должны меняться извне класса
        /// </summary>
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
