using Xamarin.Forms;

namespace SiamCross.Models.Adapters.PhyInterface
{
    public interface IBtLeInterfaceCross : IPhyInterface { }
    public static class FactoryBtLe
    {
        public static IPhyInterface GetCurent()
        {
            return DependencyService.Resolve<IBtLeInterfaceCross>();
        }
    }//static public class Factory
}
