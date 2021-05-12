using SiamCross.Models;
using SiamCross.Models.Sensors;
using SiamCross.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels.MeasurementViewModels
{
    public class BaseSurveyVM : BasePageVM, ISurveyVM
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
        async Task<JobStatus> DoSurvey()
        {
            var result = await Sensor.Model.Manager.Execute(Config?.TaskSave);
            if (JobStatus.Сomplete != result)
                return result;
            result = await Sensor.Model.Manager.Execute(Model.TaskStart);
            if (JobStatus.Сomplete != result)
                return result;
            return await DoWaitSurvey();
        }
        async Task<JobStatus> DoWaitSurvey()
        {
            return await Sensor.Model.Manager.Execute(Config?.TaskWait);
        }
        async Task<JobStatus> DoSaveParam()
        {
            return await Sensor.Model.Manager.Execute(Config?.TaskSave);
        }
        async Task<JobStatus> DoLoadParam()
        {
            return await Sensor.Model.Manager.Execute(Config?.TaskLoad);
        }

        public BaseSurveyVM(ISensor sensor, ISurvey model)
        {
            Sensor = sensor;
            Model = model;
            Config = Model?.Config;

            CmdSaveParam = new AsyncCommand(DoSaveParam,
                () => Sensor.Model.Manager.IsFree,
                    null, false, false);

            CmdLoadParam = new AsyncCommand(DoLoadParam,
                () => Sensor.Model.Manager.IsFree,
                    null, false, false);

            CmdStart = new AsyncCommand(DoSurvey,
                () => Sensor.Model.Manager.IsFree,
                    null, false, false);

            CmdWait = new AsyncCommand(DoWaitSurvey,
                () => Sensor.Model.Manager.IsFree,
                null, false, false);

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
