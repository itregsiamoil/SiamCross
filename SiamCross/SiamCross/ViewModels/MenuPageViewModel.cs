using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using SiamCross.Models;
using Xamarin.Forms;
using SiamCross.Views;

namespace SiamCross.ViewModels
{
    public class MenuPageViewModel
    {
        public ICommand GoHomeCommand { get; set; }
        public ICommand GoSecondCommand { get; set; }
        public ICommand GoThirdCommand { get; set; }
        public MenuPageViewModel()
        {
            GoHomeCommand = new Command(GoHome);
            GoSecondCommand = new Command(GoSecond);
            GoThirdCommand = new Command(GoThird);
        }

        void GoHome(object obj)
        {
            App.NavigationPage.Navigation.PopToRootAsync();
            App.MenuIsPresented = false;
        }

        void GoSecond(object obj)
        {
            App.NavigationPage.Navigation.PushAsync(new Home()); //the content page you wanna load on this click event 
            App.MenuIsPresented = false;
        }

        void GoThird(object obj)
        {
            App.NavigationPage.Navigation.PushAsync(new ClinicInformation());
            App.MenuIsPresented = false;
        }
    }
}
