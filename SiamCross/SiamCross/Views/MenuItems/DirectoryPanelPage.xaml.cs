﻿using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views.MenuItems
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DirectoryPanelPage : ContentPage
    {
        public DirectoryPanelPage()
        {
            var vm = new ViewModel<DirectoryViewModel>();
            this.BindingContext = vm.GetViewModel;
            InitializeComponent();
        }
    }
}