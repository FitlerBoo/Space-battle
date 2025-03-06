using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_battle.Model
{
    internal static class ReaderExtensions
    {
        public static GameObjectType ReadGameObjectType(this BinaryReader br) => (GameObjectType)br.ReadByte();
        public static GameObjectStyle ReadGameObjectStyle(this BinaryReader br) => (GameObjectStyle)br.ReadByte();
        public static double ReadHealth(this BinaryReader br) => br.ReadDouble();
        public static int ReadBulletsCount(this BinaryReader br) => br.ReadInt32();
        public static Position ReadPosition(this BinaryReader br)
        {
            var x = br.ReadDouble();
            var y = br.ReadDouble();
            var angle = br.ReadDouble();
            return new Position(x, y, angle);
        }
    }
}
