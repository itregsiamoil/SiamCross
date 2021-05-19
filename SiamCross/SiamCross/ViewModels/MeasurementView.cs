using SiamCross.Models;
using System;

namespace SiamCross.ViewModels
{
    public class MeasurementView : BaseVM
    {
        public PositionVM Position { get; }
        public MeasureData MeasureData { get; private set; }

        public MeasurementView(MeasureData data)
        {
            MeasureData = data;
            Position = new PositionVM(new PositionModel(MeasureData.Position));
        }
        public MeasurementView()
        {
            MeasureData = new MeasureData(
                new Position()
                , new Models.DeviceInfo()
                , new CommonInfo()
                , new MeasurementInfo());
        }

        public long Id => MeasureData.Id;
        public string Name => MeasureData.Device.Name;
        public string Number => MeasureData.Device.Number.ToString();
        public DateTime BeginTimestamp => MeasureData.Measure.BeginTimestamp;
        public DateTime EndTimestamp => MeasureData.Measure.EndTimestamp;
        public uint MeasureKind => MeasureData.Measure.Kind;
        public string Comment => MeasureData.Measure.Comment;

        public string LastSentTimestamp
        {
            get => DateTime.MinValue == MeasureData.MailDistribution.Timestamp ? string.Empty : MeasureData.MailDistribution.Timestamp.ToString();
            set { DateTime.TryParse(value, out MeasureData.MailDistribution.Timestamp); ChangeNotify(); }
        }
        public string LastSentRecipient
        {
            get => MeasureData.MailDistribution.Destination;
            set { MeasureData.MailDistribution.Destination = value; ChangeNotify(); }
        }

        public string LastSaveTimestamp
        {
            get => DateTime.MinValue == MeasureData.FileDistribution.Timestamp ? string.Empty : MeasureData.FileDistribution.Timestamp.ToString();
            set { DateTime.TryParse(value, out MeasureData.FileDistribution.Timestamp); ChangeNotify(); }
        }
        public string LastSaveFolder
        {
            get => MeasureData.FileDistribution.Destination;
            set { MeasureData.FileDistribution.Destination = value; ChangeNotify(); }
        }

        // view info 
        private bool _IsRunning = false;
        public bool IsRunning
        {
            get => _IsRunning;
            set => SetProperty(ref _IsRunning, value);
        }

        bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }


    }
}
