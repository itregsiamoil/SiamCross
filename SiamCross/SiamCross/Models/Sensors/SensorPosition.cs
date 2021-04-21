using SiamCross.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors
{
    [Serializable]
    public class SensorPosition : BaseVM
    {
        public readonly SensorModel Sensor;
        readonly Position _Position = new Position();

        public ITask TaskLoad { get; set; }
        public ITask TaskSave { get; set; }
        public Position Saved { get; private set; }
        public Position Current
        {
            get => _Position;
            set
            {
                if (null == value)
                {
                    _Position.Field = 0;
                    _Position.Well = "0";
                    _Position.Bush = "0";
                    _Position.Shop = 0;
                }
                else
                {
                    _Position.Field = value.Field;
                    _Position.Well = value.Well;
                    _Position.Bush = value.Bush;
                    _Position.Shop = value.Shop;
                }
                ChangeNotify();
            }
        }

        public ICommand CmdMakeNew { get; }
        public ICommand CmdLoad { get; }
        public ICommand CmdSave { get; }
        public SensorPosition(SensorModel sensorModel)
        {
            Sensor = sensorModel;

            CmdMakeNew = new AsyncCommand(ShowMakeNewPosition
                , (Func<object, bool>)null, null, false, false);
            CmdLoad = new AsyncCommand(DoLoad
                , () => Sensor.Manager.IsFree, null, false, false);
            CmdSave = new AsyncCommand(DoSave
                , () => Sensor.Manager.IsFree, null, false, false);
        }

        async Task ShowMakeNewPosition()
        {
            try
            {
                await App.NavigationPage.Navigation.PushModalAsync(new Views.AddFieldPage());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                //throw;
            }
        }
        async Task DoLoad()
        {
            if (null != TaskLoad && await Sensor.Manager.Execute(TaskLoad))
            {
                if (null == Saved)
                    Saved = new Position();
                Saved.Field = Current.Field;
                Saved.Well = Current.Well;
                Saved.Bush = Current.Bush;
                Saved.Shop = Current.Shop;
                ChangeNotify(nameof(Saved));
            }
        }
        async Task DoSave()
        {
            if (null != TaskSave && await Sensor.Manager.Execute(TaskSave))
            {
                if (null == Saved)
                    Saved = new Position();
                Saved.Field = Current.Field;
                Saved.Well = Current.Well;
                Saved.Bush = Current.Bush;
                Saved.Shop = Current.Shop;
                ChangeNotify(nameof(Saved));
            }
        }

    }
}
