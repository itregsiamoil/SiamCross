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
    public class SensorModel
    {
        public readonly TaskManager Manager = new TaskManager();
        public IStorage Storage { get; set; }

        public readonly List<ISurvey> Surveys = new List<ISurvey>();
    }


    public interface ISensor : IDisposable, INotifyPropertyChanged
    {
        SensorModel Model { get; }

        BaseStorageVM StorageVM { get; set; }
        IViewModel FactoryConfigVM { get; set; }
        IViewModel UserConfigVM { get; set; }
        IViewModel StateVM { get; set; }
        SurveysCollectionVM SurveysVM { get; }
        PositionInfoVM PositionVM { get; set; }



        TaskManagerVM TaskManager { get; set; }



        ICommand ShowDetailViewCommand { get; set; }
        ICommand ShowSurveysViewCommand { get; set; }
        ICommand ShowInfoViewCommand { get; set; }

        //IReadOnlyList<SurveyVM> Surveys { get; set; }




        IProtocolConnection Connection { get; }

        List<MemStruct> Memory { get; }

        //Task ActivateAsync();
        //Task DeactivateAsync();
        Task<bool> DoActivate(CancellationToken token = default);

        string ConnStateStr { get; }
        bool Activate { get; set; }
        bool IsAlive { get; }
        bool IsMeasurement { get; }
        bool IsEnableQickInfo { get; set; }
        float MeasureProgress { get; set; }
        Task<bool> QuickReport(CancellationToken cancelToken);
        Task StartMeasurement(object measurementParameters);
        ScannedDeviceInfo ScannedDeviceInfo { get; }
        CommonInfo Info { get; }


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
