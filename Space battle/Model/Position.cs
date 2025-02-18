using System;
using System.Net;

namespace Space_battle.Model
{
    internal class Position
    {
        private const int ANGLE_STEP = 10;

        private double _movementRange;

        public double X { get; private set; }
        public double Y { get; private set; }
        public double MovementAngle { get; private set; }

        /// <summary>
        /// Конструктор для создания Bullet
        /// </summary>
        public Position(double x, double y, double angle, double movementRange)
        {
            X = x;
            Y = y;
            MovementAngle = angle;
            _movementRange = movementRange;
        }

        public void Move()
        {
            X -= (_movementRange * Math.Cos((ANGLE_STEP * MovementAngle) * Math.PI / 180));
            Y += (_movementRange * Math.Sin((ANGLE_STEP * MovementAngle) * Math.PI / 180));
        }

        public void CalculatePlayerMovementSpeed(bool increaseSpeed)
        {
            _movementRange = (increaseSpeed ? 1 : -1) * 0.2;
            if (_movementRange <= 9.8 && _movementRange >= 0)
                return;
            else if (_movementRange > 10)
                _movementRange = 10;
            else _movementRange = 0;
        }

        public void ChangeAngle(bool increase)
        {
            int increment = increase ? 1 : -1;
            MovementAngle += increment;
            MovementAngle %= 36;
        }

        public int GetAngleStep()
        {
            return ANGLE_STEP;
        }
    }
}
