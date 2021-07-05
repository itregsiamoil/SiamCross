using System.Threading.Tasks;

namespace SiamCross
{
    public static class UnitTests
    {
        public static Task Start()
        {
#if DEBUG
            //Pkg.Test();
#endif
            return Task.CompletedTask;
        }
    }
}
