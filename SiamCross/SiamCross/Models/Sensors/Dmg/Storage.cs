using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.Models.Sensors.Dmg
{
    public class Storage : BaseStorageModel
    {
        public readonly SensorModel SensorModel;

        private bool _Aviable;
        public bool Aviable
        {
            get => _Aviable;
            set => SetProperty(ref _Aviable, value);
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
        async Task Read()
        {
            var taskRead = new TaskStorageRead(SensorModel);
            await SensorModel.Manager.Execute(taskRead);
        }
        async Task Update()
        {
            var task = new TaskStorageUpdate(SensorModel);
            await SensorModel.Manager.Execute(task);
        }
        async Task Clear()
        {
            bool result = await Application.Current.MainPage.DisplayAlert(
                string.Empty,
                $"{Resource.Clear} {Resource.Storage}?",
                Resource.YesButton,
                Resource.NotButton);
            if (!result)
                return;
            var task = new TaskStorageClear(SensorModel);
            await SensorModel.Manager.Execute(task);
            await Update();
        }

    }
}
