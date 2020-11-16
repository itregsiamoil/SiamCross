using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Adapters
{
    public interface IPhyInterface
    {
        void Disable();
        void Enable();

        bool IsEnbaled { get; }
    }
}
