using SiamCross.Models.Connection.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SiamCross.Models.Sensors.Dua.Surveys
{
    public class StaticLevel: BaseSurvey
    {
        private readonly ISensor _Sensor;
        private readonly IProtocolConnection _Connection;

        public bool IsValveAutomaticEnabled;
        public bool IsValveDurationShort;
        public bool IsValveDirectionInput;
        public bool IsPiezoDepthMax;
        public bool IsPiezoAdditionalGain;
        public double SoundSpeedFixed = DefaultSoundSpeedFixed;
        public ushort SoundSpeedTableId;

        public static readonly double DefaultSoundSpeedFixed = 320;

        public StaticLevel(ISensor sensor)
        {
            _Sensor = sensor;
            _Connection = sensor.Connection;

            var manager = _Sensor.Model.Manager;
            var taskUpdate = new TaskUpdateStaticLevelInfo(this, _Sensor);
            CmdUpdate = new AsyncCommand(
                () => manager.Execute(taskUpdate),
                () => _Sensor.TaskManager.IsFree,
                null, false, false);

            var taskSurvey = new TaskSurveyStaticLevel(this, _Sensor);
            CmdStart= new AsyncCommand(
                () => manager.Execute(taskSurvey),
                () => _Sensor.TaskManager.IsFree,
                null, false, false);
        }
    }
}
