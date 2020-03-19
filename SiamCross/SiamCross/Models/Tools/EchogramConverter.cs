using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Tools
{
    public static class EchogramConverter
    {
        public static float[,] GetXYs(DuMeasurement measurement)
        {
            float[,] result = new float[measurement.Echogram.Length, 2];
            if (measurement.SoundSpeed == "")
            {
                var table = HandbookData.Instance.GetSoundSpeedList().SingleOrDefault(
                    t => t.ToString() == measurement.SoundSpeedCorrection);
                if (table != null)
                {
                    float v = table.LevelSpeedTable[measurement.AnnularPressure];
                    float xDiscrete = 3000 * v / 341.33f;

                    for (int i = 0; i < measurement.Echogram.Length; i++)
                    {
                        var yDiscrete = -(measurement.Echogram[i] - 128);
                        double y = Math.Pow(yDiscrete, 1 / 0.35);
                    }
                }
            }
            return null;
        }
    }
}
