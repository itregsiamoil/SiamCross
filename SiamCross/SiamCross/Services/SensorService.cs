using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Plugin.Messaging;
using SiamCross.AppObjects;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Ddim2.Measurement;
using SiamCross.Models.Sensors.Ddin2.Measurement;
using SiamCross.Models.Tools;
using SiamCross.Views;
using Xamarin.Forms;

namespace SiamCross.Services
{
    public sealed class SensorService 
    {
        private static readonly Lazy<SensorService> _instance =
            new Lazy<SensorService>(() => new SensorService());

        public int SensorsCount => _sensors.Count;

        private SensorService()
        {
            _sensors = new List<ISensor>();
        }

        public static SensorService Instance { get => _instance.Value; }

        private List<ISensor> _sensors;

        public IEnumerable<ISensor> Sensors => _sensors;

        public event Action<SensorData> SensorAdded;

        public event Action<SensorData> SensorDataChanged;

        public void SensorDataChangedHandler(SensorData data)
        {
            SensorDataChanged?.Invoke(data);
        }

        public void AddSensor(ScannedDeviceInfo deviceInfo)
        {
            foreach(var sensor in Sensors)
            {
                if(sensor.SensorData.Name == deviceInfo.Name)
                {
                    return;
                }
            }

            var addebleSensor = SensorFactory.CreateSensor(deviceInfo);
            if (addebleSensor == null)
            {
                return;
            }

            _sensors.Add(addebleSensor);
            
            SensorAdded?.Invoke(addebleSensor.SensorData);

            new Thread(async () =>
            {
                await SaveDevicesService.SaveDevices(
                    _sensors.Select(s => s?.ScannedDeviceInfo));
            }).Start();
        }

        public async Task DeleteSensor(int id)
        {
            var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
            if (sensor != null)
            {
                //await sensor.BluetoothAdapter.Disconnect();
                _sensors.Remove(sensor);
                sensor.Dispose();
            }
            new Thread(async () =>
            {
                await SaveDevicesService.SaveDevices(
                    _sensors.Select(s => s?.ScannedDeviceInfo));
            }).Start();
        }

        public ISaveDevicesService SaveDevicesService { get; set; }
        public IFileManager AppCotainer { get; private set; }

        public void LoadSavedDevices()
        {
            if (SaveDevicesService == null) return;

            var thread = new Thread(async () => 
            {
                var savedDevices = await SaveDevicesService.LoadDevices();

                foreach (var device in savedDevices)
                {
                    var addebleSensor = SensorFactory.CreateSensor(device);
                    if (addebleSensor == null)
                    {
                        return;
                    }
                    _sensors.Add(addebleSensor);
                    SensorAdded?.Invoke(addebleSensor.SensorData);
                }
            });
            thread.Start();
        }

        public async Task StartMeasurementOnSensor(int id, object parameters)
        {
            var sensor = _sensors.FirstOrDefault(s => s.SensorData.Id == id);
            if (sensor != null)
            {
                await sensor.StartMeasurement(parameters);
            }
        }

        public async void MeasurementHandler(object measurementArgs)
        {
            int addbleId;
            switch (measurementArgs)
            {
                case Ddim2MeasurementData ddim2Data:
                    var dbModelDdim2 = new Ddim2Measurement(ddim2Data);
                    addbleId = DataRepository.Instance.SaveDdim2Measurement(dbModelDdim2);

                    await App.NavigationPage.Navigation.PushModalAsync(
                            new Ddim2MeasurementDonePage(
                                DataRepository.Instance.GetDdim2MeasurementById(addbleId)),
                                true);

                    var qwe1 = new FileSaver(AppContainer.Container.Resolve<IFileManager>());
                    var name1 = ("ddim2_" + 
                        new DateTimeConverter().DateTimeToString(dbModelDdim2.DateTime) + ".xml").Replace(':', '-');
                    qwe1.SaveXml(name1, new XmlCreator().CreateDdim2Xml(dbModelDdim2));

                 //   EmailService.Instance.SendEmailWithFile(name1);

                    break;
                case Ddin2MeasurementData ddin2Data:
                    var dbModelDdin2 = new Ddin2Measurement(ddin2Data);
                    addbleId = DataRepository.Instance.SaveDdin2Measurement(dbModelDdin2);
                    var dbObj = DataRepository.Instance.GetDdin2MeasurementById(dbModelDdin2.Id);
                    await App.NavigationPage.Navigation.PushModalAsync(
                           new Ddin2MeasurementDonePage(
                               dbObj),
                               true);

                    var qwe = new FileSaver(AppContainer.Container.Resolve<IFileManager>());
                    var name = ("ddin2_" +
                        new DateTimeConverter().DateTimeToString(dbModelDdin2.DateTime) + ".xml").Replace(':', '-');
                    qwe.SaveXml(name, new XmlCreator().CreateDdin2Xml(dbModelDdin2));

                    //EmailService.Instance.SendEmailWithFile(name);
                    
                    break;
                default:
                    break;
            }
        }

        private void SendEmail(string name)
        {
            switch (Device.RuntimePlatform)
            {
                case Device.WPF:
                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(), name);

                    if (File.Exists(path))
                    {
                        var emailMessenger =
                            CrossMessaging.Current.EmailMessenger;

                        if (emailMessenger.CanSendEmail)
                        {
                            var email = new EmailMessageBuilder()
                            .To("gelcen777@gmail.com")
                            .Subject("Xamarin Messaging Plugin")
                            .Body("Well hello there from Xam.Messaging.Plugin")
                            .WithAttachment(path, "measurement")
                            .Build();

                            emailMessenger.SendEmail(email);
                        }
                    }
                    break;
                case Device.Android:
                    var file = Path.Combine(
                        System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Personal), name);

                    if (file != null && File.Exists(file))
                    {
                        var emailMessenger = 
                            CrossMessaging.Current.EmailMessenger;

                        if (emailMessenger.CanSendEmail)
                        {
                            var email = new EmailMessageBuilder()
                            .To("gelcen777@gmail.com")
                            .Subject("Xamarin Messaging Plugin")
                            .Body("Well hello there from Xam.Messaging.Plugin")
                            .WithAttachment(file, "measurement")
                            .Build();
                            emailMessenger.SendEmail(email);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
