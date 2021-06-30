using SiamCross.Services;

namespace SiamCross.Models
{
    public class PositionModel : ViewModels.BaseVM
    {
        readonly Position _Data;
        public uint FieldId
        {
            get => _Data.Field;
            set { SetProperty(ref _Data.Field, value); ChangeNotify(nameof(FieldName)); }
        }
        public string FieldName
        {
            get
            {
                if (Repo.FieldDir.DictById.TryGetValue(_Data.Field, out FieldItem item))
                    return item.Title;
                return string.Empty;
            }
        }
        public string Well
        {
            get => _Data.Well;
            set => SetProperty(ref _Data.Well, value);
        }
        public string Bush
        {
            get => _Data.Bush;
            set => SetProperty(ref _Data.Bush, value);
        }
        public uint Shop
        {
            get => _Data.Shop;
            set => SetProperty(ref _Data.Shop, value);
        }
        public PositionModel(Position data)
        {
            _Data = data;
        }
        public string AsString => $"{Resource.Field}: {FieldName}[{FieldId}]"
                    + $"\n{Resource.Well}: {Well}"
                    + $" {Resource.Bush}: {Bush}"
                    + $" {Resource.Shop}: {Shop}";
    }
}
