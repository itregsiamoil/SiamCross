using SiamCross.Models.Sensors;
using System.Windows.Input;

namespace SiamCross.ViewModels.MeasurementViewModels
{
    public class SurveyVM : BaseVM
    {
        public SurveyVM(ISensor sensor, string name, string description)
        {
            //Page cfgPage = null;
            Sensor = sensor;
            Name = name;
            Description = description;

            ShowConfigViewCommand = BaseSensor.CreateAsyncCommand(() => this);

        }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public ISensor Sensor { get; private set; }

        public ICommand ShowConfigViewCommand { get; set; }
    }
}
