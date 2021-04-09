using Xamarin.CommunityToolkit.ObjectModel;

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

        public DuaStorage(ISensor sensor)
        {
            _Sensor = sensor;

            var manager = _Sensor.Model.Manager;
            var task = new TaskStorageUpdate(this, _Sensor);

            CmdUpdateStorageInfo = new AsyncCommand(
                () => manager.Execute(task),
                () => _Sensor.TaskManager.IsFree,
                null, false, false);


            var taskRead = new TaskStorageRead(this, _Sensor);

            CmdDownload = new AsyncCommand(
                () => manager.Execute(taskRead),
                () => _Sensor.TaskManager.IsFree,
                null, false, false);

        }



    }
}
