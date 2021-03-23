using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.ViewModels;
using SiamCross.ViewModels.MeasurementViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface ISensor : IDisposable, INotifyPropertyChanged
    {
        IMeasurementsDownloader Downloader { get; set; }
        ICommand ShowDetailViewCommand { get; set; }
        ICommand ShowUserConfigViewCommand { get; set; }
        ICommand ShowFactoryConfigViewCommand { get; set; }
        ICommand ShowSurveysViewCommand { get; set; }

        ICommand ShowStateViewCommand { get; set; }
        ICommand ShowDownloadsViewCommand { get; set; }


        IReadOnlyCollection<SurveyVM> Surveys { get; set; }



        PositionInfoVM Position { get; set; }
        IProtocolConnection Connection { get; }

        List<MemStruct> Memory { get; }

        //Task ActivateAsync();
        //Task DeactivateAsync();

        string ConnStateStr { get; }
        bool Activate { get; set; }
        bool IsAlive { get; }
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
