using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SiamCross.ViewModels
{
    public class MeasurementsViewModel : BaseViewModel, IViewModel
    {
        public class MeasurementView
        {
            public string Field { get; set; }
            public DateTime Date { get; set; }
            public string MeasurementType { get; set; }
            public string Comments { get; set; }
        }

        public ObservableCollection<MeasurementView> Measurements { get; set; }

        public MeasurementsViewModel()
        {
            var ddim2Measurements = DataRepository.Instance.GetDdim2Items();
            var ddin2Measurements = DataRepository.Instance.GetDdin2Items();
            foreach (var m in ddim2Measurements)
            {
                Measurements.Add(
                    new MeasurementView
                    {
                        Field = m.Field,
                        Date = m.DateTime,
                        MeasurementType = "Динамограмма",
                        Comments = m.Comment
                    });
            }
            foreach (var m in ddin2Measurements)
            {
                Measurements.Add(
                    new MeasurementView
                    {
                        Field = m.Field,
                        Date = m.DateTime,
                        MeasurementType = "Динамограмма",
                        Comments = m.Comment
                    });
            }
            Measurements.OrderBy(m => m.Date);
        }
    }
}
