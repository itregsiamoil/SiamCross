using SiamCross.ViewModels;
using SiamCross.ViewModels.Dmg;
using SiamCross.ViewModels.Dmg.Surveys;
using SiamCross.Views;
using SiamCross.Views.DDIN2;
using SiamCross.Views.Dmg;
using SiamCross.Views.MenuItems;
using SiamCross.Views.MenuItems.HandbookPanel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.Services
{

    public static class PageNavigator
    {
        static readonly Dictionary<Type, Func<ContentPage>> _Factory = new Dictionary<Type, Func<ContentPage>>();
        static readonly Dictionary<Type, ContentPage> _Views = new Dictionary<Type, ContentPage>();

        static PageNavigator()
        {
            //Register(typeof(SurveyVM), ()=> new SurveyView());
        }
        public static Task Init()
        {
            Register(typeof(MailSettingsVM), () => new MailSettingsPage());
            Register(typeof(FieldsDirVM), () => new DirectoryPage(() => null));
            Register(typeof(SoundSpeedListVM), () => new DirectoryPage(() => null));
            Register(typeof(MeasurementsVMService), () => new MeasurementsPage());

            Register(typeof(SensorPositionVM), () => new PositionEditPage());
            Register(typeof(SensorDetailsVM), () => new SensorDetailsPage());
            Register(typeof(SurveysCollectionVM), () => new SurveysCollectionPage());

            //Register(typeof(FactoryConfigVM), () => new Views.Dua.FactoryConfigPage());
            //Register(typeof(UserConfigVM), () => new Views.Dua.UserConfigPage());
            //ViewFactoryService.Register(typeof(StateVM), () => new StatePage());

            Register(typeof(DmgStorageVM), () => new DmgDownloadPage());
            //Register(typeof(ViewModels.Dmg.Survey.DynamogrammVM), () => new DynamogrammPage());
            Register(typeof(DynamogrammSurveyCfgVM), () => new DynamogrammSurveyCfgPage());
            Register(typeof(ValveTestSurveyCfgVM), () => new ValveTestSurveyCfgPage());
            Register(typeof(RodsWeightSurveyCfgVM), () => new RodsWeightSurveyCfgPage());

            Register(typeof(ViewModels.Dua.Survey.SurveyVM), () => new Views.Dua.SurvayCfgPage());
            Register(typeof(ViewModels.Dua.DuaStorageVM), () => new Views.Dua.DuaStoragePage());

            Register(typeof(ViewModels.Umt.SurveyVM), () => new Views.Umt.SurvayCfgPage());
            Register(typeof(ViewModels.Umt.StorageVM), () => new Views.Umt.StoragePage());

            return Task.CompletedTask;
        }
        public static void Register(Type type, ContentPage view)
        {
            _Views[type] = view;
        }
        public static void Register(Type type, Func<ContentPage> createFunc)
        {
            _Factory[type] = createFunc;
        }
        public static ContentPage Get(object bindingContext)
        {
            var type = bindingContext.GetType();
            var view = Get(type);
            if (null == view)
                return null;
            if (!Equals(view.BindingContext, bindingContext))
                view.BindingContext = bindingContext;
            return view;
        }
        public static T Get<T>(Type type, object bindingContext) where T : ContentPage
        {
            if (!(Get(type) is T view))
                return null;
            if (!Equals(view.BindingContext, bindingContext))
                view.BindingContext = bindingContext;
            return view;
        }
        public static ContentPage Get(Type type)
        {
            if (_Views.TryGetValue(type, out ContentPage view))
            {
                return view;
            }

            if (!_Factory.TryGetValue(type, out Func<ContentPage> fn))
                return null;

            view = fn.Invoke();
            _Views[type] = view;
            return view;
        }

        public static async Task ShowPageAsync(IViewModel vm)
        {
            try
            {
                if (null == vm)
                    return;

                IReadOnlyList<Page> stack = App.NavigationPage.Navigation.NavigationStack;
                int pos;
                for (pos = 1; pos < stack.Count; ++pos)
                {
                    var pg = stack[pos];
                    if (Equals(pg.BindingContext?.GetType(), vm.GetType()))
                        break;
                }
                if (pos < App.NavigationPage.Navigation.NavigationStack.Count)
                {
                    if (pos == stack.Count - 1)
                        return;
                    var samePage = stack[pos];
                    App.NavigationPage.Navigation.RemovePage(samePage);
                    await App.NavigationPage.Navigation.PushAsync(samePage);
                    return;
                    /*
                    while (pos < App.NavigationPage.Navigation.NavigationStack.Count - 1)
                        await App.NavigationPage.Navigation.PopAsync();
                    return;
                    */
                }

                var page = Get(vm);
                if (null == page)
                    return;
                await App.NavigationPage.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION {ex.Message} {ex.GetType()}\n{ex.StackTrace}");
            }
        }
        public static AsyncCommand CreateAsyncCommand(Func<IViewModel> fnGetVM)
        {
            return new AsyncCommand(() => ShowPageAsync(fnGetVM())
                , (Func<object, bool>)null, null, false, false);
        }
        public static async Task<bool> ShowDeleteQuestion()
        {
            return await Application.Current.MainPage.DisplayAlert(
                  Resource.Attention,
                  Resource.DeleteQuestion,
                  Resource.YesButton,
                  Resource.NotButton);
        }
    }
}
