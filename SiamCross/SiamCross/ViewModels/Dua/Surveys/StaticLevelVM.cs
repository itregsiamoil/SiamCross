using SiamCross.Models.Sensors;
using SiamCross.ViewModels.MeasurementViewModels;

namespace SiamCross.ViewModels.Dua.Survey
{
    public class StaticLevelVM : SurveyVM
    {
        public StaticLevelVM(ISensor sensor, string name, string description)
            : base(sensor, name, description)
        {

        }
    }
}
