using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Umt
{
    public class SurveyCfg : BaseSurveyCfg
    {
        private readonly SensorModel _Sensor;

        public bool Synched = false;

        public UInt32 Period;
        public bool IsEnabledTempRecord;


        Task DoSave()
        {
            //var taskSaveInfo = new TaskSaveSurveyInfo(this, _Sensor);
            //await _Sensor.Manager.Execute(taskSaveInfo);
            return Task.CompletedTask;
        }
        Task DoLoad()
        {
            //var taskUpdate = new TaskLoadSurveyInfo(this, _Sensor);
            //await _Sensor.Manager.Execute(taskUpdate);
            return Task.CompletedTask;
        }

        public SurveyCfg(SensorModel sensor)
        {
            _Sensor = sensor;

            CmdLoadParam = new AsyncCommand(DoLoad,
                () => _Sensor.Manager.IsFree,
                null, false, false);

            CmdSaveParam = new AsyncCommand(DoSave,
                () => _Sensor.Manager.IsFree,
                null, false, false);

            //CmdShow = new AsyncCommand(DoShow,
            //    (Func<bool>)null,
            //    null, false, false);
        }
    }
}
