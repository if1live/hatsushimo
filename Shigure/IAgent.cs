using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Hatsushimo;
using Hatsushimo.NetChan;
using Hatsushimo.Packets;
using Optional.Unsafe;

namespace Shigure
{
    public interface IAgent
    {
        Task<bool> Run();
    }
}
