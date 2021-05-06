using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public class SensorModel : IDisposable
    {
        public readonly DeviceInfo Device;

        public readonly IProtocolConnection Connection;
        public readonly TaskManager Manager;
        public readonly ConnectionHolder ConnHolder;

        public readonly SensorPosition Position;
        public readonly List<ISurvey> Surveys;
        public readonly CommonInfo Info;
        public IStorage Storage { get; set; }
        public ISurveyCfg SurveyCfg { get; set; }

        public SensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
        {
            Device = deviceInfo;
            Connection = conn;
            Manager = new TaskManager();
            ConnHolder = new ConnectionHolder(Manager, Connection);

            Position = new SensorPosition(this);
            Surveys = new List<ISurvey>();
            Info = new CommonInfo();
        }

        public virtual void Dispose()
        {
            Connection.Disconnect();

            ConnHolder?.Dispose();
            Manager?.Dispose();
            Connection.Dispose();
        }
    }


    public interface ISensor : IDisposable, INotifyPropertyChanged
    {
        SensorModel Model { get; }

        IProtocolConnection Connection { get; }
        TaskManagerVM TaskManager { get; set; }
        BaseStorageVM StorageVM { get; set; }
        SurveysCollectionVM SurveysVM { get; }
        SensorPositionVM PositionVM { get; }
        //IViewModel FactoryConfigVM { get; set; }
        //IViewModel UserConfigVM { get; set; }
        //IViewModel StateVM { get; set; }
        ICommand ShowDetailViewCommand { get; set; }
        ICommand ShowSurveysViewCommand { get; set; }
        ICommand ShowInfoViewCommand { get; set; }

        string ConnStateStr { get; }
        bool Activate { get; set; }
        bool IsMeasurement { get; }
        bool IsEnableQickInfo { get; set; }
        float MeasureProgress { get; set; }
        Task<bool> QuickReport(CancellationToken cancelToken);
        Task StartMeasurement(object measurementParameters);
        ScannedDeviceInfo ScannedDeviceInfo { get; }

        string Name { get; }
        string Type { get; }
        string Firmware { get; }
        string Battery { get; }
        string Temperature { get; }
        string RadioFirmware { get; }
        string Status { get; set; }
        Guid Id { get; }

    }
}
