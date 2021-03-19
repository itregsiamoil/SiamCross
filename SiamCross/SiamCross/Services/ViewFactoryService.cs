using SiamCross.ViewModels.MeasurementViewModels;
using SiamCross.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SiamCross.Services
{
    
    static public class ViewFactoryService
    {
        static Dictionary<Type, Func<ContentPage>> _Factory = new Dictionary<Type, Func<ContentPage>>();
        static Dictionary<Type, ContentPage> _Views = new Dictionary<Type, ContentPage>();

        static ViewFactoryService()
        {
            //Register(typeof(SurveyVM), ()=> new SurveyView());
        }


        public static void Register(Type type, ContentPage view)
        {
            _Views[type] = view;
        }
        public static void Register(Type type, Func<ContentPage> createFunc)
        {
            _Factory[type] = createFunc;
        }

        public static T Get<T>(Type type, object bindingContext) where T : ContentPage
        {
            var view = Get(type) as T;
            if (null == view)
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


    }
}
