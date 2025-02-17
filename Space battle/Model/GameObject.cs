using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Space_battle.Model
{
    abstract class GameObject
    {
        protected bool _isFirstPlayer;
        protected Position _position;
        protected Rectangle _form;
        public Rect HitBox => new Rect(_position.X, _position.Y, _form.Width, _form.Height);
        public Rectangle GetForm() => _form;

        public GameObject(bool isFirstPlayer)
        {
            _isFirstPlayer = isFirstPlayer;
            _position = new Position(isFirstPlayer);
            SetForm();
            Transform();
        }
        public GameObject(bool isFirstPlayer, double x, double y, double angle)
        {
            _isFirstPlayer = isFirstPlayer;
            _position = new Position(x, y, angle);
            SetForm();
            Transform();
        }

        public void Transform()
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
        }

        protected abstract void SetForm();
    }
}
