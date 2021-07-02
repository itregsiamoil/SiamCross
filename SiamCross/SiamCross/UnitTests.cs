using SiamCross.Models.Connection.Protocol.Siam;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross
{
    static public class UnitTests
    {
        static public Task Start()
        {
#if DEBUG
            Pkg.Test();
#endif
            return Task.CompletedTask;
        }
    }
}
