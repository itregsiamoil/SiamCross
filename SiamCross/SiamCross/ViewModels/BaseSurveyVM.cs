using SiamCross.Models.Sensors;
using SiamCross.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.ViewModels.MeasurementViewModels
{
    public class BaseSurveyVM : BaseVM, ISurvey
    {
        public ISensor Sensor { get; }
        public string Name { get => Model.Name; set => Model.Name = value; }
        public string Description { get => Model.Description; set => Model.Description = value; }

        public BaseSurvey Model { get; }
        public ICommand CmdStart { get; set; }
        public ICommand CmdSaveParam { get; set; }
        public ICommand CmdLoadParam { get; set; }
        public ICommand ShowConfigViewCommand { get; set; }
        public BaseSurveyVM(ISensor sensor, BaseSurvey model)
        {
            Sensor = sensor;
            Model = model;
            CmdStart = Model?.CmdStart;
            CmdSaveParam = Model?.CmdSaveParam;
            CmdLoadParam = Model?.CmdLoadParam;
            Model.PropertyChanged += StorageModel_PropertyChanged;

            ShowConfigViewCommand = new AsyncCommand(Show
                , (Func<object, bool>)null, null, false, true);
        }
        private void StorageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender != Model)
                return;
            ChangeNotify(e.PropertyName);
        }
        private async Task Show()
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
    }
}
