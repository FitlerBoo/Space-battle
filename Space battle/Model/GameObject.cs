using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace Space_battle.Model
{
    abstract class GameObject
    {
        protected Position _position;
        protected Rectangle _form;
        public double X => _position.X;
        public double Y => _position.Y;
        public Rect HitBox => new Rect(_position.X, _position.Y, _form.Width, _form.Height);
        public Rectangle Form => _form;

        public GameObject(Position pos)
        {
            MoveToPosition(pos);
            SetForm();
            Transform();
        }

        private void Transform()
        {
            _form.RenderTransform = new RotateTransform(-_position.GetAngleStep() * (_position.MovementAngle % 36),
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
    }
}
