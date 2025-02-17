﻿using System;

namespace Space_battle.Model
{
    internal class Position
    {
        private const double FIRST_PLAYER_START_X = 370;
        private const double FIRST_PLAYER_START_Y = 500;
        private const double SECOND_PLAYER_START_X = 370;
        private const double SECOND_PLAYER_START_Y = 40;
        private const int ANGLE_STEP = 10;

        private double _movementRange;

        public double X { get; private set; }
        public double Y { get; private set; }
        public double MovementAngle { get; private set; }

        public Position(bool isFirstPlayer)
        {
            SetPlayerToStartPosition(isFirstPlayer);
            MovementAngle = isFirstPlayer ? -9 : 9;
        }

        /// <summary>
        /// Конструктор для создания Bullet
        /// </summary>
        public Position(double x, double y, double angle)
        {
            X = x;
            Y = y;
            MovementAngle = angle;
            _movementRange = 40;
        }

        public void Move()
        {
            X -= (_movementRange * Math.Cos((ANGLE_STEP * MovementAngle) * Math.PI / 180));
            Y += (_movementRange * Math.Sin((ANGLE_STEP * MovementAngle) * Math.PI / 180));
        }

        public void SetPlayerToStartPosition(bool isFirstPlayer)
        {
            X = isFirstPlayer ? FIRST_PLAYER_START_X : SECOND_PLAYER_START_X;
            Y = isFirstPlayer ? FIRST_PLAYER_START_Y : SECOND_PLAYER_START_Y;
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
