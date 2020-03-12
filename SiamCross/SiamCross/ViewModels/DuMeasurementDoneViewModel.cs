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

        public DuMeasurementDoneViewModel(DuMeasurement measurement)
        {
            _measurement = measurement;
            Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());
            SoundSpeedCorrections = new ObservableCollection<string>
            {
                Resource.DynamicLevel,

            };

            SelectedField = _measurement.Field;
            Well = _measurement.Well;
            Bush = _measurement.Bush;
            Shop = _measurement.Shop;
            Date = _measurement.DateTime.ToString();
            BufferPressure = _measurement.BufferPressure;
            Comments = _measurement.Comment;
            DeviceName = _measurement.Name;
            MeasurementType = Resource.Echogram;

            ShareCommand = new Command(ShareCommandHandler);
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
    }
}
