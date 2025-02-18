using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System;
using System.Net.Sockets;

namespace Space_battle.Model
{
    abstract class GameObject
    {
        protected Position _position;
        protected Rectangle _form;
        protected bool _isFirstPlayerStyle;

        public double X => _position.X;
        public double Y => _position.Y;
        public double Speed => _position.Speed;
        protected double Angle => _position.MovementAngle;
        public Rect HitBox => new Rect(_position.X, _position.Y, _form.Width, _form.Height);
        public Rectangle Form => _form;

        public GameObject(Position pos, bool style)
        {
            _isFirstPlayerStyle = style;
            SetForm();
            MoveToPosition(pos);
            Transform();
        }

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
            MakeVisualMovement();
        }

        public void MakeVisualMovement()
        {
            Canvas.SetLeft(_form, _position.X);
            Canvas.SetTop(_form, _position.Y);
        }
        protected abstract void SetForm();
        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] data, ref int currentIndex);

        protected void SerializeAndAddProperty(double value, ref List<byte> data)
        {
            var valueByte = BitConverter.GetBytes(value);
            data.Add((byte)valueByte.Length);
            data.AddRange(valueByte);
        }
        protected double DeserializeProperty(byte[] data, ref int currentIndex)
        {
            var length = data[currentIndex++];
            byte[] byteData = new byte[length];
            for (int i = 0; i < length; i++)
                byteData[i] = data[currentIndex++];
            var deserValue = BitConverter.ToDouble(byteData, 0);
            return deserValue;
        }
    }
}
