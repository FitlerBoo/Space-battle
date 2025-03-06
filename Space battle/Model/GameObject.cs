using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.IO;
using System.Runtime.CompilerServices;

namespace Space_battle.Model
{
    enum GameObjectType
    {
        Spaceship = 0,
        Bullet = 1
    }

    internal enum GameObjectStyle
    {
        Red = 1,
        Yellow = 2
    }

    abstract class GameObject
    {
        protected Position _position;
        protected Rectangle _form;
        protected GameObjectType _type;
        protected GameObjectStyle _style;

        public GameObjectStyle Style => _style;
        public double X => _position.X;
        public double Y => _position.Y;
        public double Speed => _position.Speed;
        protected double Angle => _position.MovementAngle;
        public Rect HitBox => new Rect(_position.X, _position.Y, _form.Width, _form.Height);
        public Rectangle Form => _form;

        public GameObject(Position pos, GameObjectType type, GameObjectStyle style)
        {
            _type = type;
            _style = style;
            SetForm();
            MoveToPosition(pos);
            Transform();
        }

        public GameObject() { }

        private void Transform()
        {
            _form.RenderTransform = new RotateTransform(-_position.GetAngleStep() * ((_position.MovementAngle + 9) % 36),
                    _form.Width / 2,
                    _form.Height / 2);
        }

        public void RotateObject(bool side)
        {
            _position.ChangeAngle(side);
            Transform();
        }

        public void Move()
        {
            _position.Move();
            MakeVisualMovement();
        }

        public void MoveToPosition(Position pos)
        {
            _position = pos;
            Transform();
            MakeVisualMovement();
        }

        public void MakeVisualMovement()
        {
            Canvas.SetLeft(_form, _position.X);
            Canvas.SetTop(_form, _position.Y);
        }
        protected abstract void SetForm();
        public abstract void Serialize(BinaryWriter bw);
        public abstract void Deserialize(BinaryReader br);
    }
}
