using Autofac;
using SiamCross.AppObjects;

namespace SiamCross.Models.Adapters.PhyInterface.Bt2
{
    public interface IBt2InterfaceCross : IPhyInterface { }
    public static class Factory
    {
        public static IPhyInterface GetCurent()
        {
            IBt2InterfaceCross bt2ifc = AppContainer.Container.Resolve<IBt2InterfaceCross>();
            return bt2ifc;
        }
    }//static public class Factory
}
