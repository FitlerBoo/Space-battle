using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class Bullet : GameObject
    {
        bool _isFirstPlayer;
        public Bullet(bool isFirstStylePlayer, double x, double y, double angle)
            : base(new Position(x, y, angle, 40), isFirstStylePlayer)
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

        public override byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.Add(_isFirstPlayer ? (byte)0 : (byte)1);
            SerializeAndAddProperty(X, ref data);
            SerializeAndAddProperty(Y, ref data);
            SerializeAndAddProperty(Angle, ref data);
            return data.ToArray();
        }

        public override void Deserialize(byte[] data, ref int currentIndex)
        {
            var x = DeserializeProperty(data, ref currentIndex);
            var y = DeserializeProperty(data, ref currentIndex);
            var angle = DeserializeProperty(data, ref currentIndex);
            MoveToPosition(new Position(x, y, angle));
        }
    }
}
