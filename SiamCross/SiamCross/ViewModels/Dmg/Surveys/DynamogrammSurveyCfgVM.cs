using SiamCross.Models.Sensors;
using SiamCross.Models.Sensors.Dmg;
using SiamCross.Models.Sensors.Dmg.Surveys;
using SiamCross.ViewModels.MeasurementViewModels;
using System;

namespace SiamCross.ViewModels.Dmg.Surveys
{
    public class DynamogrammSurveyCfgVM : BaseSurveyVM
    {
        protected readonly DynamogrammSurveyCfg _ModelCfg;

        public string RodString
        {
            get => _ModelCfg.Rod.ToString();
            set
            {
                double.TryParse(value, out double val);
                _ModelCfg.Rod = val;
            }
        }
        public string DynPeriodString
        {
            get => _ModelCfg.DynPeriod.ToString();
            set
            {
                double.TryParse(value, out double val);
                _ModelCfg.DynPeriod = val;
            }
        }


        public double Rod
        {
            get
            {
                ChangeNotify(nameof(RodString));
                return _ModelCfg.Rod;
            }
            set => _ModelCfg.Rod = value;
        }
        public double DynPeriod
        {
            get
            {
                ChangeNotify(nameof(DynPeriodString));
                return _ModelCfg.DynPeriod;
            }
            set => _ModelCfg.DynPeriod = value;//ChangeNotify(nameof(PumpRate));
        }
        public UInt16 ApertNumber
        {
            get => _ModelCfg.ApertNumber;
            set => _ModelCfg.ApertNumber = value;
        }
        public UInt16 Imtravel
        {
            get => _ModelCfg.Imtravel;
            set => _ModelCfg.Imtravel = value;
        }
        public UInt16 ModelPump
        {
            get => _ModelCfg.ModelPump;
            set => _ModelCfg.ModelPump = value;
        }

        public bool ShowResult { get; set; }
        public bool SyncSurvey { get; set; }

        public DynamogrammSurveyCfgVM(ISensor sensorVM, DynamogrammSurvey model)
            : base(sensorVM, model)
        {
            _ModelCfg = model.Config as DynamogrammSurveyCfg;
        }
    }
}
