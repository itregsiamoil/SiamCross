using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaStorage : BaseStorage
    {
        private readonly ISensor _Sensor;

        public uint StartRep;
        public uint CountRep;
        public uint StartEcho;
        public uint CountEcho;

        public uint AviableRep;
        public uint AviableEcho;


        async Task Read()
        {
            var manager = _Sensor.Model.Manager;
            var taskRead = new TaskStorageRead(this, _Sensor);
            await manager.Execute(taskRead);
        }
        async Task Update()
        {
            var manager = _Sensor.Model.Manager;
            var task = new TaskStorageUpdate(this, _Sensor);
            await manager.Execute(task);
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
            var manager = _Sensor.Model.Manager;
            var task = new TaskStorageClear(_Sensor);
            await manager.Execute(task);
            await Update();
        }

        public DuaStorage(ISensor sensor)
        {
            _Sensor = sensor;
            CmdUpdateStorageInfo = new AsyncCommand(
                Update,
                () => _Sensor.TaskManager.IsFree,
                null, false, false);
            CmdDownload = new AsyncCommand(
                Read,
                () => _Sensor.TaskManager.IsFree,
                null, false, false);
            CmdClearStorage = new AsyncCommand(
                Clear,
                () => _Sensor.TaskManager.IsFree,
                null, false, false);
        }



    }
}
