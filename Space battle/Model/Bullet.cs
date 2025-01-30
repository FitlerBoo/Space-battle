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

            // TODO: Вынести в конструктор базового класса
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

            // TODO: Вынести в конструктор базового класса
            X = x;
            Y = y;
            Angle = angle;
            Transform();
        }

        /// <summary>
        /// TODO: Вынести метод в базовый класс, используется во всех наследниках
        /// или использовать для Form protected, а метод вообще убрать
        ///  public Rectangle Form { get; protected set; }
        /// </summary>
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
