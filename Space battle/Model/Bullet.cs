using System.Windows.Media;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class Bullet : GameObject
    {
        bool _isFirstPlayer;
        public Bullet(bool isFirstStylePlayer, double x, double y, double angle)
            : base(new Position(x + 30, y + 30, angle, 40))
        {
            _isFirstPlayer = isFirstStylePlayer;
        }

        protected override void SetForm()
        {
            _form = new Rectangle()
            {
                Height = 6,
                Width = 6,
                Fill = Brushes.White,
                Stroke = _isFirstPlayer ? Brushes.Yellow : Brushes.DarkRed
            };
        }
    }
}
