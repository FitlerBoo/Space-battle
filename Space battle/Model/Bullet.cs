using System.Windows.Media;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class Bullet : GameObject
    {
        public Bullet(bool isFirstPlayer, double x, double y, double angle)
            : base(isFirstPlayer ,x + 30, y + 30, angle) { }

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
