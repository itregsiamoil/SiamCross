using SiamCross.ViewModels;
using SiamCross.ViewModels.Dmg;
using SiamCross.ViewModels.Dua;
using SiamCross.Views;
using SiamCross.Views.DDIN2;
using SiamCross.Views.Dua;
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
            Register(typeof(PositionVM), () => new PositionEditPage());
            Register(typeof(SensorDetailsVM), () => new SensorDetailsPage());

            Register(typeof(DmgStorageVM), () => new DmgDownloadPage());
            Register(typeof(ViewModels.Dmg.Survey.DynamogrammVM)
                , () => new DynamogrammPage());

            Register(typeof(DuaStorageVM), () => new DuaStoragePage());
            Register(typeof(FactoryConfigVM), () => new FactoryConfigPage());
            Register(typeof(UserConfigVM), () => new UserConfigPage());
            //ViewFactoryService.Register(typeof(StateVM), () => new StatePage());
            Register(typeof(SurveysCollectionVM), () => new SurveysCollectionPage());

            Register(typeof(ViewModels.Dua.Survey.SurveyVM)
                , () => new LevelPage());
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

            view.BindingContext = bindingContext;
            return view;
        }
        public static T Get<T>(Type type, object bindingContext) where T : ContentPage
        {
            if (!(Get(type) is T view))
                return null;

            view.BindingContext = bindingContext;
            return view;
        }
        public static ContentPage Get(Type type)
        {
            if (_Views.TryGetValue(type, out ContentPage view))
                return view;

            if (!_Factory.TryGetValue(type, out Func<ContentPage> fn))
                return null;

            view = fn.Invoke();
            _Views[type] = view;
            return view;
        }


        public static Task ShowPage(IViewModel vm)
        {
            if (null == vm)
                return Task.CompletedTask;
            var page = Get(vm);
            if (null == page)
                return Task.CompletedTask;
            return App.NavigationPage.Navigation.PushAsync(page);
        }

        public static AsyncCommand CreateAsyncCommand(Func<IViewModel> fnGetVM)
        {
            return new AsyncCommand(() => ShowPage(fnGetVM())
                , (Func<object, bool>)null, null, false, false);
        }
        public static AsyncCommand CreateAsyncCommand(IViewModel vm)
        {
            Task exec()
            {
                try
                {
                    if (null == vm)
                        return Task.CompletedTask;
                    var page = Get(vm);
                    if (null == page)
                        return Task.CompletedTask;
                    return App.NavigationPage.Navigation.PushAsync(page);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex, "EXCEPTION "
                        + System.Reflection.MethodBase.GetCurrentMethod().Name
                        + "\n msg=" + ex.Message
                        + "\n type=" + ex.GetType()
                        + "\n stack=" + ex.StackTrace + "\n");

                }
                return Task.CompletedTask;
            }
            return new AsyncCommand(exec
                , (Func<object, bool>)null, null, false, false);
        }
    }
}
