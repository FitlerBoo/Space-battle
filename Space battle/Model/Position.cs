using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Space_battle.Model
{
    internal class Position
    {
        private const double PLAYER_START_X = 370;
        private const double PLAYER_START_Y = 500;
        private const int ANGLE_STEP = 10;

        private double _movementRange;
        private readonly double _angleStartPosition;
        private readonly bool _isEnemy;
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Angle { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="angle">Для вражеской ракеты угол нужно указывать 18 для корректного отображения. </param>
        /// <param name="movementSpeed">Стартовый показатель скорости перемещения объекта. Для игрока 10, для снаряда 40. /n
        /// По умолчанию скорость равна 0</param>
        public Position(double x, double y, bool isEnemy, double angle = 0, double movementSpeed = 0)
        {
            X = x;
            Y = y;
            _angleStartPosition = isEnemy ? 90 : -90;
            Angle = angle;
            _movementRange = movementSpeed;
        }

        public void CalculateNextPosition(UIElement form)
        {
            X -= (_movementRange * Math.Cos((ANGLE_STEP * Angle + _angleStartPosition) * Math.PI / 180));
            Y += (_movementRange * Math.Sin((ANGLE_STEP * Angle + _angleStartPosition) * Math.PI / 180));
            if (_isEnemy)
            {
                Canvas.SetBottom(form, Y);
                Canvas.SetRight(form, X);
            }
            else
            {
                Canvas.SetTop(form, Y);
                Canvas.SetLeft(form, X);
            }
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
