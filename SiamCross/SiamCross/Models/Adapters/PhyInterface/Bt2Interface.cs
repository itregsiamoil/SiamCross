using Autofac;
using SiamCross.AppObjects;

namespace SiamCross.Models.Adapters.PhyInterface.Bt2
{
    public interface IBt2InterfaceCross : IPhyInterface    { }
    static public class Factory
    {
        static public IPhyInterface GetCurent()
        {
            var bt2ifc = AppContainer.Container.Resolve<IBt2InterfaceCross>();
            return bt2ifc;
        }
    }//static public class Factory
}
