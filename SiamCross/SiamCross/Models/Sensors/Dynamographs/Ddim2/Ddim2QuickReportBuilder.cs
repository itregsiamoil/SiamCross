﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Sensors.Dynamographs.Ddim2
{
    [Preserve(AllMembers = true)]
    public class Ddim2QuickReportBuilder
    {
        private string _batteryVoltage;
        private string _temperatute;
        private string _load;
        private string _acceleration;

        public string BatteryVoltage
        {
            get
            {
                return _batteryVoltage != null ?
                    "Напряжение: " + _batteryVoltage + ", В\n"
                    : "";
            }
            set
            {
                _batteryVoltage = value;
            }
        }

        public string Temperature
        {
            get
            {
                return _temperatute != null ?
                    "Температура: " + _temperatute + ", °C\n"
                    : "";
            }
            set
            {
                _temperatute = value;
            }
        }

        public string Load
        {
            get
            {
                return _load != null ?
                   "Нагрузка: " + _load + ", мВ\n"
                    : "";
            }
            set
            {
                _load = value;
            }
        }

        public string Acceleration
        {
            get
            {
                return _acceleration != null ?
                   "Ускорение: " + _acceleration + ", мВ"
                    : "";
            }
            set
            {
                _acceleration = value;
            }
        }

        public string GetReport()
        {
            return BatteryVoltage + Temperature + Load + Acceleration;
        }
    }
}
