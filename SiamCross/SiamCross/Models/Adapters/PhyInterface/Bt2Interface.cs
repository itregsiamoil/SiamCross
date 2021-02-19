using Xamarin.Forms;

namespace SiamCross.Models.Adapters.PhyInterface
{
    public interface IBt2InterfaceCross : IPhyInterface { }
    public static class FactoryBt2
    {
        public static IPhyInterface GetCurent()
        {
            return DependencyService.Resolve<IBt2InterfaceCross>();
        }
    }//static public class Factory

}
