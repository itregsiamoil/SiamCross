using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Sensors.Dmg.Surveys;
using SiamCross.ViewModels.Dmg;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dmg
{
    public class DmgSensorModel : SensorModel
    {
        public DmgSensorModel(IProtocolConnection conn, DeviceInfo deviceInfo)
           : base(conn, deviceInfo)
        {
            Position.TaskLoad = new TaskPositionLoad(Position);
            Position.TaskSave = new TaskPositionSave(Position);

            Storage = new Storage(this);
            TaskWait = new TaskSurveyWait(this);

            //Surveys.Add(new Dynamogramm(this));
            Surveys.Add(new DynamogrammSurvey(this));
            Surveys.Add(new ValveTestSurvey(this));
            Surveys.Add(new RodsWeightSurvey(this));

            OnConnectQueue.Add(new TaskUpdateConfig(this));

            ConnHolder.CmdUpdateStatus = new AsyncCommand(
                UpdateStatus,
                () => Manager.IsFree,
                null, false, false);
        }
        async Task UpdateStatus()
        {
            var task = new TaskUpdateStatus(this);
            await Manager.Execute(task);
        }
    }

    public abstract class DmgBaseSensor : BaseSensor2
    {
        public readonly MemStruct _SurvayParam;
        public readonly MemVarUInt16 Rod;
        public readonly MemVarUInt32 DynPeriod;
        public readonly MemVarUInt16 ApertNumber;
        public readonly MemVarUInt16 Imtravel;
        public readonly MemVarUInt16 ModelPump;
        public readonly MemStruct _NonvolatileParam;
        public readonly MemVarFloat Nkp;
        public readonly MemVarFloat Rkp;
        public readonly MemVarFloat ZeroG;
        public readonly MemVarFloat PositiveG;
        public readonly MemVarUInt32 EnableInterval;
        public readonly MemVarFloat ZeroOffset;
        public readonly MemVarFloat SlopeRatio;
        public readonly MemVarFloat NegativeG;
        public readonly MemVarUInt16 SleepDisable;
        public readonly MemVarUInt16 SleepTimeout;
        public readonly MemStruct _CurrentParam;
        public readonly MemVarUInt16 BatteryVoltage;
        public readonly MemVarInt16 Тemperature;
        public readonly MemVarFloat LoadChanel;
        public readonly MemVarFloat AccelerationChanel;
        public readonly MemStruct _Operating;
        public readonly MemVarUInt16 CtrlReg;
        public readonly MemVarUInt16 StatReg;
        public readonly MemVarUInt32 ErrorReg;

        public readonly MemStruct _Report;
        public readonly MemVarUInt16 MaxWeight;
        public readonly MemVarUInt16 MinWeight;
        public readonly MemVarUInt16 Travel;
        public readonly MemVarUInt16 Period;
        public readonly MemVarUInt16 Step;
        public readonly MemVarUInt16 WeightDiscr;
        public readonly MemVarUInt16 TimeDiscr;


        public DmgBaseSensor(SensorModel model)
            : base(model)
        {
            StorageVM = new StorageVM(this);

            //var dmgVM = new DynamogrammVM(this, Model.Surveys[0] as BaseSurvey);
            //SurveysVM.SurveysCollection.Add(dmgVM);

            //FactoryConfigVM = new FactoryConfigVM(this);
            //UserConfigVM = new UserConfigVM(this);
            //StateVM = new StateVM(this);

            Model.ConnHolder.PropertyChanged += OnHolderChange;
            IsNewStatus = true;
        }
        void OnHolderChange(object sender, PropertyChangedEventArgs e)
        {
            if (null == sender || nameof(Model.ConnHolder.IsActivated) != e.PropertyName)
                return;
            ChangeNotify(nameof(Activate));
        }
        public override void Dispose()
        {
            Model.ConnHolder.PropertyChanged -= OnHolderChange;
            base.Dispose();
        }
        public override Task<bool> QuickReport(CancellationToken cancelToken)
        {
            return Task.FromResult(true);
        }
        public override bool Activate
        {
            get => Model.ConnHolder.IsActivated;
            set
            {
                Model.ConnHolder.IsActivated = value;
                ChangeNotify(nameof(Activate));
            }
        }

    }//DmgBaseSensor
}
