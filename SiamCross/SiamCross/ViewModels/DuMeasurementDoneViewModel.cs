using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class DuMeasurementDoneViewModel: BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private DuMeasurement _measurement;

        public ObservableCollection<string> Fields { get; set; }
        public string SelectedField
        {
            get => _measurement.Field;
            set => _measurement.Field = value;
        }
        public string Well
        {
            get => _measurement.Well;
            set => _measurement.Well = value;
        }
        public string Bush
        {
            get => _measurement.Bush;
            set => _measurement.Bush = value;
        }
        public string Shop
        {
            get => _measurement.Shop;
            set => _measurement.Shop = value;
        }
        public string Date { get; }
        public string BufferPressure
        {
            get => _measurement.BufferPressure;
            set => _measurement.BufferPressure = value;
        }
        public string Comments
        {
            get => _measurement.Comment;
            set => _measurement.Comment = value;
        }

        public string DeviceName { get; }

        public string MeasurementType { get; }

        public string FluidLevel { get; }

        public string AnnularPressure { get; }

        public string NumberOfReflections { get; }

        public ObservableCollection<string> SoundSpeedCorrections { get; }

        public string SelectedSoundSpeedCorrection { get; set; }

        public string SoundSpeed { get; set; }

        public ICommand ShareCommand { get; set; }

        private double[,] _points;

        public DuMeasurementDoneViewModel(DuMeasurement measurement)
        {
            _measurement = measurement;
            Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());
            SoundSpeedCorrections = new ObservableCollection<string>();
            foreach (var elem in HandbookData.Instance.GetSoundSpeedList())
            {
                SoundSpeedCorrections.Add(elem.ToString());
            }

            FluidLevel = _measurement.FluidLevel.ToString();
            NumberOfReflections = _measurement.NumberOfReflections.ToString();
            AnnularPressure = _measurement.AnnularPressure.ToString();
            SelectedSoundSpeedCorrection = _measurement.SoundSpeedCorrection;
            SoundSpeed = _measurement.SoundSpeed;
            SelectedField = _measurement.Field;
            Well = _measurement.Well;
            Bush = _measurement.Bush;
            Shop = _measurement.Shop;
            Date = _measurement.DateTime.ToString();
            BufferPressure = _measurement.BufferPressure;
            Comments = _measurement.Comment;
            DeviceName = _measurement.Name;
            MeasurementType = _measurement.MeasurementType;

            ShareCommand = new Command(ShareCommandHandler);

             _points = EchogramConverter.GetPoints(measurement);

            MaxGraphX = Math.Round(GetMaximumX(), 0).ToString();
            MaxGraphY = Math.Round(GetMaximumY(), 0).ToString();
            MinGraphX = Math.Round(GetMinimumX(), 0).ToString();
            MinGraphY = Math.Round(GetMinimumY(), 0).ToString();
        }

        private DateTimeConverter _timeConverter = new DateTimeConverter();

        private async void ShareCommandHandler()
        {
            try
            {
                var xmlCreator = new XmlCreator();

                var xmlSaver = DependencyService.Get<IXmlSaver>();

                var name = CreateName(_measurement.Name, _measurement.DateTime);
                //xmlSaver.SaveXml(name, xmlCreator.CreateDdim2Xml(_measurement));

                string filepath = xmlSaver.GetFilepath(name);

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = Resource.ShareMeasurement,
                    File = new ShareFile(filepath)
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ShareCommandHandler");
                throw;
            }
        }

        private string CreateName(string deviceName, DateTime date)
        {
            return $"{deviceName}_{_timeConverter.DateTimeToString(date)}.xml"
                .Replace(':', '-');
        }

        public string MaxGraphY { get; set; }
        public string MinGraphY { get; set; }
        public string MaxGraphX { get; set; }
        public string MinGraphX { get; set; }

        public double GetMaximumX()
        {
            double max = _points[0, 0];
            for (int i = 0; i < _points.GetUpperBound(0); i++)
            {
                if (_points[i, 0] > max)
                {
                    max = _points[i, 0];
                }
            }
            return max;
        }

        public double GetMinimumX()
        {
            double min = _points[0, 0];
            for (int i = 0; i < _points.GetUpperBound(0); i++)
            {
                if (_points[i, 0] < min)
                {
                    min = _points[i, 0];
                }
            }
            return min;
        }

        public double GetMaximumY()
        {
            double max = _points[1, 0];
            for (int i = 0; i < _points.GetUpperBound(0); i++)
            {
                if (_points[i, 1] > max)
                {
                    max = _points[i, 1];
                }
            }
            return max;
        }

        public double GetMinimumY()
        {
            double min = _points[1, 0];
            for (int i = 0; i < _points.GetUpperBound(0); i++)
            {
                if (_points[i, 1] < min)
                {
                    min = _points[i, 1];
                }
            }
            return min;
        }
    }
}
