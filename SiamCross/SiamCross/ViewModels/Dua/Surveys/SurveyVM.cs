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
            get => IsVisibleLevelSheduler || _ModelCfg.IsValveAutomaticEnabled;
            set
            {
                if (IsVisibleLevelSheduler)
                    _ModelCfg.IsValveAutomaticEnabled = true;
                else
                    _ModelCfg.IsValveAutomaticEnabled = value;
                ChangeNotify(nameof(IsValveAutomaticEnabled));
            }
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
        public byte PressureQuantityIndex
        {
            get => _ModelCfg.PressureQuantityIndex;
            set => _ModelCfg.PressureQuantityIndex = value;
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
        public byte LevelQuantityIndex0
        {
            get => _ModelCfg.LevelQuantityIndex[0];
            set => _ModelCfg.LevelQuantityIndex[0] = value;
        }
        public byte LevelQuantityIndex1
        {
            get => _ModelCfg.LevelQuantityIndex[1];
            set => _ModelCfg.LevelQuantityIndex[1] = value;
        }
        public byte LevelQuantityIndex2
        {
            get => _ModelCfg.LevelQuantityIndex[2];
            set => _ModelCfg.LevelQuantityIndex[2] = value;
        }
        public byte LevelQuantityIndex3
        {
            get => _ModelCfg.LevelQuantityIndex[3];
            set => _ModelCfg.LevelQuantityIndex[3] = value;
        }
        public byte LevelQuantityIndex4
        {
            get => _ModelCfg.LevelQuantityIndex[4];
            set => _ModelCfg.LevelQuantityIndex[4] = value;
        }
        public string Timestamp => _ModelCfg.Timestamp.ToString("G");

        public Kind SurveyType => _ModelSurvey.SurveyType;
        public bool IsSheduler => 2 < _ModelSurvey.SurveyType.ToByte();
        public bool IsLevelSurvey => (0 < _ModelSurvey.SurveyType.ToByte() && 5 > _ModelSurvey.SurveyType.ToByte());
        public bool IsVisibleLevelSheduler => (Kind.LRC == _ModelSurvey.SurveyType || Kind.LDC == _ModelSurvey.SurveyType);


        public SurveyVM(ISensor sensor, DuaSurvey model)
            : base(sensor, model)
        {
            _ModelCfg = model.Config as DuaSurveyCfg;
            _ModelSurvey = model;
        }


    }


}
