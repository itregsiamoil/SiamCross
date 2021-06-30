using SiamCross.Models.Tools;
using SiamCross.Services;
using SiamCross.Services.Environment;
using SiamCross.Services.StdDialog;
using SiamCross.Services.Toast;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SiamCross.ViewModels
{
    public class SoundSpeedItemVM : BaseVM
    {
        private readonly SoundSpeedModel _targetSoundSpeed;
        public List<KeyValuePair<float, float>> Points { get; private set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public ICommand CmdImport { get; }
        public ICommand CmdExport { get; }
        public ICommand CmdSave { get; }
        public string MinGraphX { get; set; }
        public string MinGraphY { get; set; }
        public string MaxGraphX { get; set; }
        public string MaxGraphY { get; set; }

        public SoundSpeedItemVM(SoundSpeedModel soundSpeed)
        {
            CmdImport = new AsyncCommand(DoCmdImport,
                (Func<bool>)null, null, false, false);
            CmdExport = new AsyncCommand(DoCmdExport,
                (Func<bool>)null, null, false, false);
            CmdSave = new AsyncCommand(DoCmdSave,
                (Func<bool>)null, null, false, false);

            _targetSoundSpeed = soundSpeed;
            if (null != _targetSoundSpeed)
            {
                Name = _targetSoundSpeed.Name;
                Code = _targetSoundSpeed.Code.ToString();
                Points = _targetSoundSpeed.LevelSpeedTable;
                InitGraph();
            }

        }

        private void InitGraph()
        {
            if (null == Points)
                return;

            MinGraphX = Math.Round(GetMinimumX(), 1).ToString();
            MaxGraphX = Math.Round(GetMaximumX(), 1).ToString();
            MinGraphY = Math.Round(GetMinimumY(), 1).ToString();
            MaxGraphY = Math.Round(GetMaximumY(), 1).ToString();
        }

        private async Task DoCmdImport()
        {
            try
            {
                string path = await StdDialogService.Instance.ShowOpenDialog();
                List<KeyValuePair<float, float>> newSoundTable;
                using (StreamReader reader = new StreamReader(path))
                    newSoundTable = SoundSpeedParser.ToList(reader.ReadToEnd());
                if (newSoundTable == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        Resource.Attention,
                        Resource.WrongFormatOrContent,
                        Resource.Ok);
                    return;
                }
                Points = newSoundTable;
                InitGraph();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
            }
        }
        private async Task DoCmdExport()
        {
            try
            {
                var str = SoundSpeedParser.ToString(Points);
                var path = Path.Combine(EnvironmentService.Instance.GetDir_Downloads(), Name);

                if (string.IsNullOrEmpty(Name) || File.Exists(path))
                {
                    ToastService.Instance.LongAlert("Отсутсвует имя или такой файл уже существует");
                    return;
                }
                using (var fs = File.CreateText(path))
                {
                    await fs.WriteAsync(SoundSpeedParser.ToString(Points));
                    fs.Close();
                }
                ToastService.Instance.LongAlert($"Файл сохранён\n{path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
            }
        }
        private async Task DoCmdSave()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name)
                    || string.IsNullOrWhiteSpace(Code)
                    || null == Points || 0 == Points.Count)
                {
                    ToastService.Instance.LongAlert(Resource.FillInAllTheFields);
                    return;
                }
                if (null != _targetSoundSpeed)
                    await Repo.SoundSpeedDir.DeleteAsync(_targetSoundSpeed.Code);

                var newModel = new SoundSpeedModel(uint.Parse(Code), Name, Points);
                await Repo.SoundSpeedDir.AddAsync(newModel);
                await App.NavigationPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception {ex.Message}\n{ex.StackTrace}");
            }
        }

        public double GetMaximumX()
        {
            double max = Points[0].Key;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Key > max)
                {
                    max = pair.Key;
                }
            }

            return max;
        }

        public double GetMinimumX()
        {
            double min = Points[0].Key;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Key < min)
                {
                    min = pair.Key;
                }
            }

            return min;
        }

        public double GetMinimumY()
        {
            double min = Points[0].Value;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Value < min)
                {
                    min = pair.Value;
                }
            }

            return min;
        }

        public double GetMaximumY()
        {
            double max = Points[0].Value;

            foreach (KeyValuePair<float, float> pair in Points)
            {
                if (pair.Value > max)
                {
                    max = pair.Value;
                }
            }

            return max;
        }

    }
}
