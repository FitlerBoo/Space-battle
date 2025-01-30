using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Space_battle.Model
{
    internal class Position
    {
        private const double PLAYER_START_X = 370;
        private const double PLAYER_START_Y = 500;
        private const int ANGLE_STEP = 10;

        private double _movementRange;
        private readonly double _angleStartPosition;
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Angle { get; private set; }

        public Position(double x, double y, double angle, bool isEnemy, [Optional] int movementSpeed)
        {
            X = x;
            Y = y;
            _angleStartPosition = isEnemy ? 90 : -90;
            Angle = angle;
            _movementRange = movementSpeed;
        }

        public void CalculateNextPosition()
        {
            X -= (_movementRange * Math.Cos((ANGLE_STEP * Angle + _angleStartPosition) * Math.PI / 180));
            Y += (_movementRange * Math.Sin((ANGLE_STEP * Angle + _angleStartPosition) * Math.PI / 180));
        }

        public void MovePlayerToStartPosition()
        {
            X = PLAYER_START_X;
            Y = PLAYER_START_Y;
        }

        public void CalculatePlayerMovementRange(int acceleration)
        {
            if (_movementRange <= 10 && _movementRange >= 0)
                _movementRange += acceleration * 0.2;
            else if (_movementRange > 10)
                _movementRange = 10;
            else _movementRange = 0;
        }
    }
}
