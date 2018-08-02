using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Hatsushimo;
using Hatsushimo.Packets;
using Hatsushimo.Utils;
using NLog;

namespace Mikazuki
{
    public class Food
    {
        public int ID { get; private set; }
        public Vector2 Position { get; private set; }

        public Food(int id, Vector2 pos)
        {
            this.ID = id;
            this.Position = pos;
        }
    }
}
