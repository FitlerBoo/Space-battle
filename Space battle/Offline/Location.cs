using Space_battle.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Space_battle.Offline
{
    internal class Location
    {
        private const double FIRST_PLAYER_START_X = 370;
        private const double FIRST_PLAYER_START_Y = 500;
        private const double SECOND_PLAYER_START_X = 370;
        private const double SECOND_PLAYER_START_Y = 40;
        private const double ANGLE_START_POSITION = -90;
        private const int ANGLE_STEP = 10;

        private double _movementRange;

        public double X { get; private set; }
        public double Y { get; private set; }
        public double MovementAngle { get; private set; }

        public Location(bool isSecondPlayer)
        {
            SetPlayerToStartPosition(isSecondPlayer);
            MovementAngle = isSecondPlayer ? 18 : 0;
        }

        /// <summary>
        /// Конструктор для создания Projectile
        /// </summary>
        public Location(double x, double y, bool isSecondPlayer, double angle)
        {
            X = x;
            Y = y;
            MovementAngle = angle;
            _movementRange = 40;
        }

        public void Move()
        {
            X -= (_movementRange * Math.Cos((ANGLE_STEP * MovementAngle + ANGLE_START_POSITION) * Math.PI / 180));
            Y += (_movementRange * Math.Sin((ANGLE_STEP * MovementAngle + ANGLE_START_POSITION) * Math.PI / 180));
        }

        public void SetPlayerToStartPosition(bool isSecondPlayer)
        {
            X = isSecondPlayer ? SECOND_PLAYER_START_X : FIRST_PLAYER_START_X;
            Y = isSecondPlayer ? SECOND_PLAYER_START_Y : FIRST_PLAYER_START_Y;
        }

        public void CalculatePlayerMovementSpeed(bool increaseSpeed)
        {
            var acceleration = increaseSpeed ? 1 : -1;
            if (_movementRange <= 10 && _movementRange >= 0)
                _movementRange += acceleration * 0.2;
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
