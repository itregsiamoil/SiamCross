using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.Models.Sensors.Umt
{
    public class Storage : BaseStorage
    {
        public readonly SensorModel SensorModel;

        ulong _TotalSpace = 0;
        double _EmptySpaceRatio = 0;
        UInt16 _SurveyQty = 0;
        uint _StartRep;
        uint _CountRep;


        public ulong TotalSpace
        {
            get => _TotalSpace;
            set => SetProperty(ref _TotalSpace, value);
        }
        public double EmptySpaceRatio
        {
            get => _EmptySpaceRatio;
            set => SetProperty(ref _EmptySpaceRatio, value);
        }
        public UInt16 SurveyQty
        {
            get => _SurveyQty;
            set => SetProperty(ref _SurveyQty, value);
        }
        public uint StartRep
        {
            get => _StartRep;
            set => SetProperty(ref _StartRep, value);
        }
        public uint CountRep
        {
            get => _CountRep;
            set => SetProperty(ref _CountRep, value);
        }

        async Task Read()
        {
            var taskRead = new TaskStorageRead(this, SensorModel);
            await SensorModel.Manager.Execute(taskRead);
        }
        async Task Update()
        {
            var task = new TaskStorageUpdate(this, SensorModel);
            await SensorModel.Manager.Execute(task);
        }
        async Task Clear()
        {
            bool result = await Application.Current.MainPage.DisplayAlert(
                string.Empty,
                "Очистить хранилище?",
                Resource.YesButton,
                Resource.NotButton);
            if (!result)
                return;
            var task = new TaskStorageClear(SensorModel);
            await SensorModel.Manager.Execute(task);
            await Update();
        }

        public Storage(SensorModel sensor)
        {
            SensorModel = sensor;
            CmdUpdateStorageInfo = new AsyncCommand(
                Update,
                () => SensorModel.Manager.IsFree,
                null, false, false);
            CmdDownload = new AsyncCommand(
                Read,
                () => SensorModel.Manager.IsFree,
                null, false, false);
            CmdClearStorage = new AsyncCommand(
                Clear,
                () => SensorModel.Manager.IsFree,
                null, false, false);
        }

    }
}
