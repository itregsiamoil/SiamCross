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
using System.Globalization;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class Ddin2MeasurementDoneViewModel : BaseViewModel, IViewModel
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private Ddin2Measurement _measurement;

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
        public string BatteryVolt { get; }
        public string Temperature { get; }
        public string MainFirmware { get; }
        public string RadioFirmware { get; }

        public string MeasurementType { get; }
        public string ApertNumber
        {
            get => _measurement.ApertNumber.ToString();
            set
            {
                if (short.TryParse(value, out short apertNumber))
                    _measurement.ApertNumber = apertNumber;
            }
        }

        public string MaxLoad { get; }
        public string MinLoad { get; }
        public string Imtravel { get; }
        public string PumpRate { get; }

        public string UpperRodWeight { get; }
        public string LowerRodWeight { get;  }
        public string SelectedModelPump { get; }

        public string MaxGraphX { get; private set; }
        public string MaxGraphY { get; private set; }
        public ICommand ShareCommand { get; set; }

        public Ddin2MeasurementDoneViewModel(Ddin2Measurement measurement)
        {
            try
            {
                _measurement = measurement;
                Fields = new ObservableCollection<string>(HandbookData.Instance.GetFieldList());

                InitMaxMixGraphValue(_measurement.DynGraph.ToList(),
                    _measurement.Step, _measurement.WeightDiscr);

                SelectedField = _measurement.Field;
                Well = _measurement.Well;
                Bush = _measurement.Bush;
                Shop = _measurement.Shop;
                Date = _measurement.DateTime.ToString();
                BufferPressure = _measurement.BufferPressure;
                Comments = _measurement.Comment;
                DeviceName = _measurement.Name;
                BatteryVolt = _measurement.BatteryVolt;
                MainFirmware = _measurement.MainFirmware;
                Temperature = _measurement.Temperature;
                RadioFirmware = _measurement.RadioFirmware;
                MeasurementType = Resource.Dynamogram;
                ApertNumber = _measurement.ApertNumber.ToString();
                MaxLoad = _measurement.MaxWeight.ToString("N3", CultureInfo.InvariantCulture);
                MinLoad = _measurement.MinWeight.ToString("N3", CultureInfo.InvariantCulture);
                Imtravel = _measurement.TravelLength.ToString("N3", CultureInfo.InvariantCulture);
                PumpRate = _measurement.SwingCount.ToString("N3", CultureInfo.InvariantCulture);
                UpperRodWeight = _measurement.MaxBarbellWeight.ToString("N3", CultureInfo.InvariantCulture);
                LowerRodWeight = _measurement.MinBarbellWeight.ToString("N3", CultureInfo.InvariantCulture);
                switch (_measurement.ModelPump)
                {
                    case 0:
                        SelectedModelPump = Resource.BalancedModelPump;
                        break;
                    case 1:
                        SelectedModelPump = Resource.ChainModelPump;
                        break;
                    case 2:
                        SelectedModelPump = Resource.HydraulicModelPump;
                        break;
                    default:
                        break;
                }
                ShareCommand = new Command(ShareCommandHandler);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ddin2MeasurementVM constructor" + "\n");
                throw;
            }
        }

        private DateTimeConverter _timeConverter = new DateTimeConverter();

        private async void ShareCommandHandler()
        {
            try
            {
                var xmlCreator = new XmlCreator();

                var xmlSaver = DependencyService.Get<IXmlSaver>();

                var name = CreateName(_measurement.Name, _measurement.DateTime);
                xmlSaver.SaveXml(name, xmlCreator.CreateDdin2Xml(_measurement));

                string filepath = xmlSaver.GetFilepath(name);

                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = Resource.ShareMeasurement,
                    File = new ShareFile(filepath)
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ShareCommandHandler" + "\n");
                throw;
            }
        }

        private string CreateName(string deviceName, DateTime date)
        {
            return $"{deviceName}_{_timeConverter.DateTimeToString(date)}.xml"
                .Replace(':', '-');
        }

        private void InitMaxMixGraphValue(List<byte> graph, UInt16 step, UInt16 weightDiscret)
        {
            List<double> movement = new List<double>();
            List<double> weight = new List<double>();
            var discrets = DgmConverter.GetXYs(graph, step, weightDiscret);
            for (int i = 0; i < discrets.GetUpperBound(0); i++)
            {
                movement.Add(discrets[i, 0]);
                weight.Add(discrets[i, 1]);
            }

            MaxGraphX = Math.Round(movement.Max() / 10, 0).ToString();
            MaxGraphY = CutOffNumbers(Math.Round(weight.Max() / 1000, 3));
        }

        private string CutOffNumbers(double number)
        {
            string result = "";
            string s = number.ToString();
            if (s.Length > 4)
            {
                result += s[0];
                result += s[1];
                result += s[2];
                result += s[3];
                return result;
            }
            result = s;
            return result;
        }
    }
}
