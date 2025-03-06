using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;

namespace Space_battle.Model
{
    internal class Bullet : GameObject
    {
        public Bullet(GameObjectType type, GameObjectStyle style, double x, double y, double angle)
            : base(new Position(x, y, angle, 40), type, style)
        {
            _style = style;
        }
        public Bullet(BinaryReader br) 
        {
            Deserialize(br);
        }

        protected override void SetForm()
        {
            _form = new Rectangle()
            {
                Height = 6,
                Width = 6,
                Fill = Brushes.White,
                Stroke = GetBrush()
            };
        }

        public override void Serialize(BinaryWriter bw)
        {
            bw.WriteGameObjectType(_type);
            bw.WriteGameObjectStyle(_style);
            bw.WritePosition(_position);
        }

        public override void Deserialize(BinaryReader br)
        {
            _type = br.ReadGameObjectType();
            _style = br.ReadGameObjectStyle();
            SetForm();
            MoveToPosition(br.ReadPosition());
        }

        private SolidColorBrush GetBrush()
        {
            switch (_style)
            {
                case GameObjectStyle.Red:
                    return Brushes.DarkRed;
                case GameObjectStyle.Yellow:
                    return Brushes.Yellow;
                default:
                    return Brushes.White;
            }
        }
    }
}
