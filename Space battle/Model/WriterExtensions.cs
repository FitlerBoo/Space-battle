using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_battle.Model
{
    internal static class WriterExtensions
    {
        public static void WriteGameObjectType(this BinaryWriter bw, GameObjectType GameObjectType)
        {
            bw.Write((byte)GameObjectType);
        }
        public static void WriteGameObjectStyle(this BinaryWriter bw, GameObjectStyle GameObjectStyle)
        {
            bw.Write((byte)GameObjectStyle);
        }
        public static void WriteHealth(this BinaryWriter bw, double health)
        {
            bw.Write(health);
        }
        public static void WriteBulletsCount(this BinaryWriter bw, int count)
        {
            bw.Write(count);
        }
        public static void WritePosition(this BinaryWriter bw, Position position)
        {   
            bw.Write(position.X);
            bw.Write(position.Y);
            bw.Write(position.MovementAngle);
        }
    }
}
