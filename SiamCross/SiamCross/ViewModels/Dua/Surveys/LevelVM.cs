using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dua.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;
using System;

namespace SiamCross.ViewModels.Dua.Survey
{
    public class LevelVM : BaseSurveyVM
    {
        private readonly Level _Model;

        public bool IsValveAutomaticEnabled
        {
            get => _Model.IsValveAutomaticEnabled;
            set => _Model.IsValveAutomaticEnabled = value;
        }
        public bool IsValveDurationShort
        {
            get => _Model.IsValveDurationShort;
            set => _Model.IsValveDurationShort = value;
        }
        public bool IsValveDirectionInput
        {
            get => _Model.IsValveDirectionInput;
            set => _Model.IsValveDirectionInput = value;
        }
        public bool IsPiezoDepthMax
        {
            get => _Model.IsPiezoDepthMax;
            set => _Model.IsPiezoDepthMax = value;
        }
        public bool IsPiezoAdditionalGain
        {
            get => _Model.IsPiezoAdditionalGain;
            set => _Model.IsPiezoAdditionalGain = value;
        }
        public double SoundSpeedFixed
        {
            get => _Model.SoundSpeedFixed;
            set => _Model.SoundSpeedFixed = value;
        }
        public ushort SoundSpeedTableId
        {
            get => _Model.SoundSpeedTableId;
            set
            {
                _Model.SoundSpeedTableId = value;
                ChangeNotify(nameof(IsSoundSpeedCustom));
                ChangeNotify(nameof(IsSoundSpeedTable));
            }
        }
        public bool IsSoundSpeedCustom
        {
            get => 0 == _Model.SoundSpeedTableId;
            set
            {
                _Model.SoundSpeedTableId = (UInt16)(value? 0 : 1);
                ChangeNotify(nameof(SoundSpeedTableId));
            }
        }
        public bool IsSoundSpeedTable
        {
            get => 0 != _Model.SoundSpeedTableId;
            set
            {
                _Model.SoundSpeedTableId = (UInt16)(value ? 1 : 0);
                ChangeNotify(nameof(SoundSpeedTableId));
            }
        }

        public LevelVM(ISensor sensor, Level model)
            : base(sensor, model)
        {
            _Model = model;
        }

    }
}
