using Autofac;
using SiamCross.AppObjects;

namespace SiamCross.Models.Adapters.PhyInterface.Bt2
{
    public interface Bt2InterfaceCross : IPhyInterface    { }
    static public class Factory
    {
        static public IPhyInterface GetCurent()
        {
            var bt2ifc = AppContainer.Container.Resolve<Bt2InterfaceCross>();
            return bt2ifc;
        }
    }//static public class Factory
}
