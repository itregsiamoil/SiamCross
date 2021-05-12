﻿using SiamCross.ViewModels;
using System.Windows.Input;

namespace SiamCross.Models.Sensors
{
    public interface ISurvey
    {
        string Name { get; }
        string Description { get; }
        ISurveyCfg Config { get; }
        ITask TaskStart { get; set; }
    }
    public interface ISurveyVM
    {
        string Name { get; }
        string Description { get; }
        ISurveyCfg Config { get; }
        ICommand CmdStart { get; }
    }
    public class BaseSurvey : BaseVM, ISurvey
    {
        protected readonly SensorModel _Sensor;
        public string Name { get; }
        public string Description { get; }
        public ISurveyCfg Config { get; set; }
        public ITask TaskStart { get; set; }

        public BaseSurvey(SensorModel sensor, ISurveyCfg cfg, string name, string description)
        {
            _Sensor = sensor;
            Name = name;
            Description = description;
            Config = cfg;
        }
    }

}
