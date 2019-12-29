using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SiamCross.Views
{
    public static class NavigationHelper
    {
        public static void RemovePage(Type Page)
        {
            int pages = Application.Current.MainPage.Navigation.NavigationStack.Count;
            INavigation Navigation = Application.Current.MainPage.Navigation;
            if (pages > 0)
            {
                for (int i = 0; i < pages; i++)
                {
                    if (Navigation.NavigationStack[i].GetType() == Page.GetType() && 
                        Navigation.NavigationStack[i] != Navigation.NavigationStack.LastOrDefault())
                    {
                        Navigation.RemovePage(Navigation.NavigationStack[i]);
                        break;
                    }
                }
            }
        }
    }
}
