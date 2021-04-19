using SiamCross.Models.Sensors.Dua;
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
        public enum PositionSource
        {
            Device = 0,
            DataBase = 1,
            SurveyDataBase = 2
        }
        public readonly SensorModel SensorModel;
        public readonly Position _Position = new Position();

        public uint FieldId
        {
            get => _Position.Field;
            set { _Position.Field = value; ChangeNotify(); }
        }
        public string Well
        {
            get => _Position.Well;
            set { _Position.Well = value; ChangeNotify(); }
        }
        public string Bush
        {
            get => _Position.Bush;
            set { _Position.Bush = value; ChangeNotify(); }
        }
        public uint Shop
        {
            get => _Position.Shop;
            set { _Position.Shop = value; ChangeNotify(); }
        }
        public PositionSource Source;

        public ICommand CmdMakeNew { get; }
        public ICommand CmdLoad { get; }
        public ICommand CmdSave { get; }
        public SensorPosition(SensorModel sensorModel, PositionSource source = PositionSource.Device)
        {
            SensorModel = sensorModel;
            Source = source;

            CmdMakeNew = new AsyncCommand(ShowMakeNewPosition
                , (Func<object, bool>)null, null, false, false);
            CmdLoad = new AsyncCommand(DoLoad
                , (Func<object, bool>)null, null, false, false);
            CmdSave = new AsyncCommand(DoSave
                , (Func<object, bool>)null, null, false, false);
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
            var task = new TaskPositionLoad(this);
            await SensorModel.Manager.Execute(task);

        }
        async Task DoSave()
        {
            var task = new TaskPositionSave(this);
            await SensorModel.Manager.Execute(task);
        }

    }
}
