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
                if(double.TryParse(value, out double val))
                    _ModelCfg.Rod = val;
                else
                    _ModelCfg.Rod = 12;
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
        public UInt32 DynPeriod
        {
            get => _ModelCfg.DynPeriod;
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

        public double PumpRate
        {
            get => _ModelCfg.PumpRate;
            set => _ModelCfg.PumpRate = value;
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
