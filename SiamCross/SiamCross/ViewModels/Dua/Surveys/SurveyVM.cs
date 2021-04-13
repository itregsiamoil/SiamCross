using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dua;
using SiamCross.Models.Sensors.Dua.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;
using System;

namespace SiamCross.ViewModels.Dua.Survey
{
    public class SurveyVM : BaseSurveyVM
    {
        protected readonly DuaSurveyCfg _ModelCfg;
        protected readonly DuaSurvey _ModelSurvey;

        public bool IsAutoswitchToAPR
        {
            get => _ModelCfg.IsAutoswitchToAPR;
            set => _ModelCfg.IsAutoswitchToAPR = value;
        }
        public bool IsValveAutomaticEnabled
        {
            get => _ModelCfg.IsValveAutomaticEnabled;
            set => _ModelCfg.IsValveAutomaticEnabled = value;
        }
        public bool IsValveDurationShort
        {
            get => _ModelCfg.IsValveDurationShort;
            set => _ModelCfg.IsValveDurationShort = value;
        }
        public bool IsValveDirectionInput
        {
            get => _ModelCfg.IsValveDirectionInput;
            set => _ModelCfg.IsValveDirectionInput = value;
        }
        public bool IsPiezoDepthMax
        {
            get => _ModelCfg.IsPiezoDepthMax;
            set => _ModelCfg.IsPiezoDepthMax = value;
        }
        public bool IsPiezoAdditionalGain
        {
            get => _ModelCfg.IsPiezoAdditionalGain;
            set => _ModelCfg.IsPiezoAdditionalGain = value;
        }
        public double SoundSpeedFixed
        {
            get => _ModelCfg.SoundSpeedFixed;
            set => _ModelCfg.SoundSpeedFixed = value;
        }
        public ushort SoundSpeedTableId
        {
            get => _ModelCfg.SoundSpeedTableId;
            set
            {
                _ModelCfg.SoundSpeedTableId = value;
                ChangeNotify(nameof(IsSoundSpeedCustom));
                ChangeNotify(nameof(IsSoundSpeedTable));
            }
        }
        public bool IsSoundSpeedCustom
        {
            get => 0 == _ModelCfg.SoundSpeedTableId;
            set
            {
                _ModelCfg.SoundSpeedTableId = (UInt16)(value ? 0 : 1);
                ChangeNotify(nameof(SoundSpeedTableId));
            }
        }
        public bool IsSoundSpeedTable
        {
            get => 0 != _ModelCfg.SoundSpeedTableId;
            set
            {
                _ModelCfg.SoundSpeedTableId = (UInt16)(value ? 1 : 0);
                ChangeNotify(nameof(SoundSpeedTableId));
            }
        }
        public byte PressurePeriodIndex
        {
            get => _ModelCfg.PressurePeriodIndex;
            set => _ModelCfg.PressurePeriodIndex = value;
        }
        public byte PressureDelayIndex
        {
            get => _ModelCfg.PressureDelayIndex;
            set => _ModelCfg.PressureDelayIndex = value;
        }

        public byte LevelPeriodIndex0
        {
            get => _ModelCfg.LevelPeriodIndex[0];
            set => _ModelCfg.LevelPeriodIndex[0] = value;
        }
        public byte LevelPeriodIndex1
        {
            get => _ModelCfg.LevelPeriodIndex[1];
            set => _ModelCfg.LevelPeriodIndex[1] = value;
        }
        public byte LevelPeriodIndex2
        {
            get => _ModelCfg.LevelPeriodIndex[2];
            set => _ModelCfg.LevelPeriodIndex[2] = value;
        }
        public byte LevelPeriodIndex3
        {
            get => _ModelCfg.LevelPeriodIndex[3];
            set => _ModelCfg.LevelPeriodIndex[3] = value;
        }
        public byte LevelPeriodIndex4
        {
            get => _ModelCfg.LevelPeriodIndex[4];
            set => _ModelCfg.LevelPeriodIndex[4] = value;
        }
        public byte LevelDelayIndex0
        {
            get => _ModelCfg.LevelDelayIndex[0];
            set => _ModelCfg.LevelDelayIndex[0] = value;
        }
        public byte LevelDelayIndex1
        {
            get => _ModelCfg.LevelDelayIndex[1];
            set => _ModelCfg.LevelDelayIndex[1] = value;
        }
        public byte LevelDelayIndex2
        {
            get => _ModelCfg.LevelDelayIndex[2];
            set => _ModelCfg.LevelDelayIndex[2] = value;
        }
        public byte LevelDelayIndex3
        {
            get => _ModelCfg.LevelDelayIndex[3];
            set => _ModelCfg.LevelDelayIndex[3] = value;
        }
        public byte LevelDelayIndex4
        {
            get => _ModelCfg.LevelDelayIndex[4];
            set => _ModelCfg.LevelDelayIndex[4] = value;
        }
        public byte SurveyType => _ModelSurvey.SurveyType;

        public SurveyVM(ISensor sensor, DuaSurvey model)
            : base(sensor, model)
        {
            _ModelCfg = model.Config as DuaSurveyCfg;
            _ModelSurvey = model;
        }


    }


}
