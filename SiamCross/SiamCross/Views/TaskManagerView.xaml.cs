﻿using SiamCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SiamCross.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskManagerView : ContentView
    {
        public static readonly BindableProperty TaskManagerProperty = BindableProperty.Create(nameof(TaskManager)
            , typeof(TaskManagerVM), typeof(TaskManagerView), null, BindingMode.OneWay);
        public TaskManagerVM TaskManager
        {
            get => (TaskManagerVM)GetValue(TaskManagerProperty);
            set=> SetValue(TaskManagerProperty, value);
        }

        public TaskManagerView()
        {
            InitializeComponent();
            
        }
    }
}