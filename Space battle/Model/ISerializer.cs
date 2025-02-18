using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_battle.Model
{
    interface ISerializer
    {
        void Serialize(ISerializer serializer);
    }
}
