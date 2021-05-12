using System;
using System.IO;

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
        public string GetDir_LocalApplicationData()
        {
            return _object.GetDir_LocalApplicationData(); ;
        }
        public static FileStream CreateTempFileSurvey()
        {
            var path = Path.Combine(
                System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal), "bin");
            var dir = Directory.CreateDirectory(path);
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    string filename = Path.Combine(dir.FullName, "tmp" + i.ToString());
                    return new FileStream(filename, FileMode.CreateNew);
                }
                catch (Exception)
                {

                }
            }
            return null;
        }


    }
}
