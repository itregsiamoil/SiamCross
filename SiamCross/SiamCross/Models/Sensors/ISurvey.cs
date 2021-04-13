using SiamCross.ViewModels;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface ISurvey
    {
        string Name { get; }
        string Description { get; }


        ISurveyCfg Config { get; }
        ICommand CmdStart { get; }
    }
    public class BaseSurvey : BaseVM, ISurvey
    {
        protected readonly ISensor _Sensor;
        public string Name { get; }
        public string Description { get; }
        public ISurveyCfg Config { get; set; }
        public ICommand CmdStart { get; set; }

        public BaseSurvey(ISensor sensor, ISurveyCfg cfg, string name, string description)
        {
            _Sensor = sensor;
            Name = name;
            Description = description;
            Config = cfg;
        }
    }

}
