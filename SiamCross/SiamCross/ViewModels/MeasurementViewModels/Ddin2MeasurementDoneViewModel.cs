using NLog;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class Ddin2MeasurementDoneViewModel : BaseVM
    {
        private static readonly Logger _logger = DependencyService.Get<ILogManager>().GetLog();

        private readonly Ddin2Measurement _measurement;

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
        public double BufferPressure
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
        public string LowerRodWeight { get; }
        public string SelectedModelPump { get; }

        public string MaxGraphX { get; private set; }
        public string MaxGraphY { get; private set; }
        public string MinGraphX { get; private set; }
        public string MinGraphY { get; private set; }
        public ICommand ShareCommand { get; set; }

        public Ddin2MeasurementDoneViewModel(Ddin2Measurement measurement)
        {
            try
            {
                _measurement = measurement;

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

        private async void ShareCommandHandler()
        {
            try
            {
                XmlCreator xmlCreator = new XmlCreator();
                string name = CreateName(_measurement.Name, _measurement.DateTime);
                string filepath = await XmlSaver.SaveXml(name, xmlCreator.CreateDdin2Xml(_measurement));

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

        private static string CreateName(string deviceName, DateTime date)
        {
            return $"{deviceName}_{DateTimeConverter.DateTimeToString(date)}.xml"
                .Replace(':', '-');
        }

        private void InitMaxMixGraphValue(List<byte> graph, UInt16 step, UInt16 weightDiscret)
        {
            /*
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
            */
            MinGraphX = (0).ToString();
            MaxGraphX = _measurement.TravelLength.ToString("N2");
            MinGraphY = _measurement.MinWeight.ToString("N2");
            MaxGraphY = _measurement.MaxWeight.ToString("N2");
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
