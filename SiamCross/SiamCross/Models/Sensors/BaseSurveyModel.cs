using SiamCross.ViewModels;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.Models.Sensors
{
    public abstract class BaseSurveyModel : BaseVM
    {
        protected readonly SensorModel _Sensor;
        public string Name { get; }
        public string Description { get; }
        public BaseSurveyCfgModel Config { get; set; }
        public ITask TaskStart { get; set; }
        public virtual BaseSurveyVM GetCfgVM(ISensor sensorVM) { return null; }

        public BaseSurveyModel(SensorModel sensor, BaseSurveyCfgModel cfg, string name, string description)
        {
            _Sensor = sensor;
            Name = name;
            Description = description;
            Config = cfg;
        }
    }

}
