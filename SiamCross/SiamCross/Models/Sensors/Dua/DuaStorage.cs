using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaStorage : BaseStorage
    {
        public readonly SensorModel SensorModel;

        public uint StartRep;
        public uint CountRep;
        public uint StartEcho;
        public uint CountEcho;

        public uint AviableRep;
        public uint AviableEcho;


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

        public DuaStorage(SensorModel sensor)
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
