using SiamCross.Models;
using System;

namespace SiamCross.ViewModels
{
    public class MeasurementView : BaseVM
    {
        public MeasureData MeasureData { get; private set; }

        public MeasurementView(MeasureData data)
        {
            MeasureData = data;
        }
        public MeasurementView()
        {
            MeasureData = new MeasureData(
                new Position()
                , new DeviceInfo()
                , new CommonInfo()
                , new MeasurementInfo());
        }

        public long Id
        { get => MeasureData.Id; set { MeasureData.Id = value; ChangeNotify(); } }
        public string Name
        { get => MeasureData.Device.Name; set { MeasureData.Device.Name = value; ChangeNotify(); } }
        public string Field
        {
            get => MeasureData.Position.Field.ToString();
            set
            {
                if (uint.TryParse(value, out uint val))
                {
                    MeasureData.Position.Field = val;
                    ChangeNotify();
                }
            }
        }
        public string Comments
        { get => MeasureData.Measure.Comment; set { MeasureData.Measure.Comment = value; ChangeNotify(); } }
        public DateTime Date
        { get => MeasureData.Measure.EndTimestamp; set { MeasureData.Measure.EndTimestamp = value; ChangeNotify(); } }
        public uint MeasureKind
        { get => MeasureData.Measure.Kind; set { MeasureData.Measure.Kind = value; ChangeNotify(); } }
        public string MeasureKindName
        {
            get
            {
                if (MeasurementIndex.Instance.TryGetName(MeasureData.Measure.Kind, out string name))
                    return name;
                return string.Empty;
            }
            set
            {
                if (uint.TryParse(value, out uint idx))
                {
                    if (MeasurementIndex.Instance.TryGetName(idx, out string name))
                    {
                        MeasureData.Measure.Kind = idx;
                        ChangeNotify();
                    }
                    return;
                }
                if (MeasurementIndex.Instance.TryGetId(value, out uint idxx))
                {
                    MeasureData.Measure.Kind = idxx;
                    ChangeNotify();
                }
            }
        }

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
        public bool Sending
        { get => _Sending; set { _Sending = value; ChangeNotify(); } }
        private bool _Sending = false;
        public bool Saving
        { get => _Saving; set { _Saving = value; ChangeNotify(); } }
        private bool _Saving;
        public bool IsSelected
        {
            get => _IsSelected;
            set
            {
                if (_IsSelected == value)
                    return;
                _IsSelected = value;
                ChangeNotify();
            }
        }
        private bool _IsSelected = false;

    }
}
