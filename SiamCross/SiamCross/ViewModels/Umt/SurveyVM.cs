using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Umt;
using SiamCross.Models.Sensors.Umt.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;
using System;

namespace SiamCross.ViewModels.Umt
{
    public class SurveyVM : BaseSurveyVM
    {
        protected readonly SurveyCfg _ModelCfg;
        protected readonly UmtSurvey _ModelSurvey;

        public UInt32 IsAutoswitchToAPR
        {
            get => _ModelCfg.Period;
            set => _ModelCfg.Period = value;
        }
        public bool IsEnabledTempRecord
        {
            get => _ModelCfg.IsEnabledTempRecord;
            set => _ModelCfg.IsEnabledTempRecord = value;
        }
        public bool IsSheduler => 2 < _ModelSurvey.SurveyType.ToByte();

        public SurveyVM(ISensor sensor, UmtSurvey model)
            : base(sensor, model)
        {
            _ModelCfg = model.Config as SurveyCfg;
            _ModelSurvey = model;
        }


    }
}
