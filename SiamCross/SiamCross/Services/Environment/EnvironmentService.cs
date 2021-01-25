using System;

namespace SiamCross.Services.Environment
{
    public sealed class EnvironmentService : IEnvironment
    {
        private static readonly Lazy<EnvironmentService> _instance =
            new Lazy<EnvironmentService>(() => new EnvironmentService());
        public static IEnvironment Instance => _instance.Value;

        private readonly IEnvironment _object;
        private EnvironmentService()
        {
            _object = Xamarin.Forms.DependencyService.Get<IEnvironment>();
        }
        public string GetDir_Downloads()
        {
            return _object.GetDir_Downloads(); ;
        }
        public string GetDir_Measurements()
        {
            return _object.GetDir_Measurements(); ;
        }
    }
}
