using Autofac;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Logging;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SiamCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class DuMeasurementDoneViewModel : BaseVM
    {
        private static readonly Logger _logger = AppContainer.Container.Resolve<ILogManager>().GetLog();

        private readonly DuMeasurement _measurement;

        public string SelectedField => _measurement.Field;
        public string Well => _measurement.Well;
        public string Bush => _measurement.Bush;
        public string Shop => _measurement.Shop;
        public string Date => _measurement.DateTime.ToString();

        public string BufferPressure => _measurement.BufferPressure.ToString("F3", CultureInfo.InvariantCulture);
        public string PumpDepth => _measurement.PumpDepth.ToString("F3", CultureInfo.InvariantCulture);
        public string Comments => _measurement.Comment;
        public string DeviceName => _measurement.Name;
        public string BatteryVolt => _measurement.BatteryVolt;
        public string Temperature => _measurement.Temperature;
        public string MainFirmware => _measurement.MainFirmware;
        public string RadioFirmware => _measurement.RadioFirmware;
        public string MeasurementType => _measurement.MeasurementType;
        public string FluidLevel => _measurement.FluidLevel.ToString("N3", CultureInfo.InvariantCulture);
        public string AnnularPressure => _measurement.AnnularPressure.ToString("N3", CultureInfo.InvariantCulture);
        public string NumberOfReflections => _measurement.NumberOfReflections.ToString(CultureInfo.InvariantCulture);
        public ObservableCollection<string> SoundSpeedCorrections { get; }
        public string SelectedSoundSpeedCorrection => _measurement.SoundSpeedCorrection;
        public string SoundSpeed => _measurement.SoundSpeed;
        public ICommand ShareCommand { get; set; }

        public DuMeasurementDoneViewModel(DuMeasurement measurement)
        {
            _measurement = measurement;
            SoundSpeedCorrections = new ObservableCollection<string>();
            foreach (SoundSpeedModel elem in HandbookData.Instance.GetSoundSpeedList())
            {
                SoundSpeedCorrections.Add(elem.ToString());
            }

            ShareCommand = new Command(ShareCommandHandler);


        }

        public void SetAxisLimits(in float min_x, in float max_x, in float min_y, in float max_y)
        {
            MinGraphX = min_x.ToString("N0");
            MaxGraphX = (max_x).ToString("N0");

            MinGraphY = ((0 > min_y) ?
                Math.Pow(Math.Abs(min_y), 0.5f / 0.35) * (-1)
                : Math.Pow(min_y, 0.5f / 0.35)).ToString("N0");
            MaxGraphY = ((0 > max_y) ?
                Math.Pow(Math.Abs(max_y), 0.5f / 0.35) * (-1)
                : Math.Pow(max_y, 0.5f / 0.35)).ToString("N0");
        }

        private async void ShareCommandHandler()
        {
            try
            {
                XmlCreator xmlCreator = new XmlCreator();

                string name = CreateName(_measurement.Name, _measurement.DateTime);
                string filepath = await XmlSaver.SaveXml(name, xmlCreator.CreateDuXml(_measurement));

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

        public string MaxGraphY { get; private set; }
        public string MinGraphY { get; private set; }
        public string MaxGraphX { get; private set; }
        public string MinGraphX { get; private set; }


    }
}
