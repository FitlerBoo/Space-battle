using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Space_battle.Offline
{
    abstract class SingleGameObject
    {
        protected Location _location;
        protected Rectangle _form;
        protected bool _isSecondPlayer;
        public Rect HitBox => new Rect(_location.X, _location.Y, _form.Width, _form.Height);
        public SingleGameObject(bool isSecondPlayer)
        {
            _isSecondPlayer = isSecondPlayer;
            _location = new Location(isSecondPlayer);
        }
        public SingleGameObject(double x, double y, bool isSecondPlayer, double angle)
        {
            _isSecondPlayer= isSecondPlayer;
            _location = new Location(x, y, angle);
        }

        // TODO: Как правильно назвать этот метод и реализовать перемещение 
        public void Move(bool increaseSpeed)
        {
            _location.CalculatePlayerMovementSpeed(increaseSpeed);
            _location.Move();
            Canvas.SetTop(_form, _location.Y);
            Canvas.SetLeft(_form, _location.X);
        }
        public void Move()
        {
            _location.Move();
            Canvas.SetTop(_form, _location.Y);
            Canvas.SetLeft(_form, _location.X);
        }

        /// <summary>
        /// true = left / false = right
        /// </summary>
        public void RotateObject(bool side)
        {
            _location.ChangeAngle(side);
            _form.RenderTransform = new RotateTransform(-_location.GetAngleStep() * _location.MovementAngle,
                    _form.ActualWidth / 2,
                    _form.ActualHeight / 2);
        }

        public Rectangle GetForm()
        {
            return _form;
        }
    }
}
