using SiamCross.ViewModels;

namespace SiamCross.Models
{
    public class SurveyDoneModel : BaseVM
    {
        readonly MeasureData _Data;
        public PositionModel Position { get; }
        public DeviceInfoModel DeviceInfo { get; }
        public SurveyInfoModel SurveyInfo { get; }
        public DistributionInfoModel MailDistribution { get; }
        public DistributionInfoModel FileDistribution { get; }

        public SurveyDoneModel(MeasureData data)
        {
            _Data = data;
            Position = new PositionModel(_Data.Position);
            DeviceInfo = new DeviceInfoModel(_Data.Device);
            SurveyInfo = new SurveyInfoModel(_Data.Measure);
            MailDistribution = new DistributionInfoModel(_Data.MailDistribution);
            FileDistribution = new DistributionInfoModel(_Data.FileDistribution);
        }
    }
}
