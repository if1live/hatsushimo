using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hatsushimo.NetChan
{
    public interface IPacket : ISerialize
    {
        short Type { get; }
    }
}
