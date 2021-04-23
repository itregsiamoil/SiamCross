using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels.MeasurementViewModels
{
    public class BaseSurveyVM : BasePageVM, ISurvey, ISurveyCfg
    {
        public string Name => Model.Name;
        public string Description => Model.Description;

        public ISensor Sensor { get; }
        public ISurvey Model { get; }
        public ISurveyCfg Config { get; }

        public ICommand CmdSaveParam { get; }
        public ICommand CmdLoadParam { get; }
        public ICommand CmdShow { get; }
        public ICommand CmdWait { get; }
        public ICommand CmdStart { get; set; }
        public BaseSurveyVM(ISensor sensor, ISurvey model)
        {
            Sensor = sensor;
            Model = model;
            Config = Model?.Config;

            CmdSaveParam = Config?.CmdSaveParam;
            CmdLoadParam = Config?.CmdLoadParam;
            //CmdShow = Config?.CmdShow;
            CmdStart = Model?.CmdStart;
            CmdWait = Model?.CmdWait;

            CmdShow = new AsyncCommand(DoShow,
                (Func<bool>)null,
                null, false, false);

            if (null != Config)
                Config.PropertyChanged += StorageModel_PropertyChanged;
            Sensor.Model.Manager.OnChangeTask.ProgressChanged += SetTask;
        }
        private void StorageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != Model.Config)
                return;
            ChangeNotify(e.PropertyName);
        }

        async Task DoShow()
        {
            try
            {
                var view = PageNavigator.Get(this);
                if (null != view)
                {
                    await App.NavigationPage.Navigation.PushAsync(view);
                    CmdLoadParam?.Execute(this);
                }
            }
            catch (Exception)
            {

            }
        }
        void SetTask(object obj, ITask task)
        {
            RaiseCanExecuteChanged(CmdSaveParam);
            RaiseCanExecuteChanged(CmdLoadParam);
            RaiseCanExecuteChanged(CmdStart);
            RaiseCanExecuteChanged(CmdWait);
            RaiseCanExecuteChanged(CmdShow);
        }
        public override void Unsubscribe()
        {
            if (null != Config)
                Config.PropertyChanged -= StorageModel_PropertyChanged;
            Sensor.Model.Manager.OnChangeTask.ProgressChanged -= SetTask;
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        public void ResetSaved()
        {
            throw new NotImplementedException();
        }
    }
}
