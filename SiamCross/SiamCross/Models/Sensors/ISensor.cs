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
        public readonly IProtocolConnection Connection;
        public readonly TaskManager Manager;



        public readonly SensorPosition Position;



        public readonly DeviceInfo Device;
        public readonly CommonInfo Info;
        public IStorage Storage { get; set; }
        public ISurveyCfg SurveyCfg { get; set; }
        public ConnectionHolder ConnHolder { get; }

        public readonly List<ISurvey> Surveys = new List<ISurvey>();

        public SensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
        {
            Device = deviceInfo;
            Connection = conn;
            Manager = new TaskManager();
            Position = new SensorPosition(this);
            Info = new CommonInfo();

            ConnHolder = new ConnectionHolder(Manager, Connection);
        }

        public virtual void Dispose()
        {
            Connection.Disconnect();
            ConnHolder?.Dispose();
            Connection.Dispose();
        }
    }


    public interface ISensor : IDisposable, INotifyPropertyChanged
    {
        SensorModel Model { get; }

        BaseStorageVM StorageVM { get; set; }
        IViewModel FactoryConfigVM { get; set; }
        IViewModel UserConfigVM { get; set; }
        IViewModel StateVM { get; set; }
        SurveysCollectionVM SurveysVM { get; }
        PositionVM PositionVM { get; }



        TaskManagerVM TaskManager { get; set; }



        ICommand ShowDetailViewCommand { get; set; }
        ICommand ShowSurveysViewCommand { get; set; }
        ICommand ShowInfoViewCommand { get; set; }

        //IReadOnlyList<SurveyVM> Surveys { get; set; }




        IProtocolConnection Connection { get; }

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
