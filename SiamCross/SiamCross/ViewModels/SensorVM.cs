using SiamCross.Models.Connection;
using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors
{
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
        string Number { get; }
        string Label { get; }
        string Firmware { get; }
        string Battery { get; }
        string Temperature { get; }
        string RadioFirmware { get; }
        string Status { get; set; }
        Guid Id { get; }

        bool IsNewStatus { get; }
        bool IsOldStatus { get; }
    }
}
