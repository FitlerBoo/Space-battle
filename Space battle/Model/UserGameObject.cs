using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Space_battle.Model
{
    abstract class UserGameObject : GameObject
    {

        public int Acceleration { get; set; }
        public double PlayerStartX { get; } = 370;
        public double PlayerStartY { get; } = 500;
        public double HP { get; set; } = 100;
        public void Reset()
        {
            X = PlayerStartX;
            Y = PlayerStartY;
        }

        /// <summary>
        /// true = left / false = right
        /// </summary>
        public void RotateForm(bool rotatingSide)
        {
            int angle = rotatingSide ? 1 : -1;
            Angle += angle;
            Angle %= 36;
            Form.RenderTransform = new RotateTransform(-AngleValue * Angle,
                    Form.ActualWidth / 2,
                    Form.ActualHeight / 2);
        }

        /// <summary>
        /// true - increase / false - reduce
        /// </summary>
        /// <param name="increaseSpeed"></param>
        public void CalculateSpeed(bool increaseSpeed)
        {
            Acceleration = increaseSpeed ? 1 : -1;

            if (Speed <= 10 && Speed >= 0)
                Speed += Acceleration * 0.2;
            else if (Speed > 10)
                Speed = 10;
            else Speed = 0;
        }
    }
}
