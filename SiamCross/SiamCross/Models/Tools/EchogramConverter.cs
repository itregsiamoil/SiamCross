using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using System;
using System.Linq;

namespace SiamCross.Models.Tools
{
    public static class EchogramConverter
    {
        public static double[,] GetXYs(DuMeasurement measurement)
        {
            double[,] result = new double[measurement.Echogram.Length, 2];
            if (measurement.SoundSpeed == "")
            {
                var table = HandbookData.Instance.GetSoundSpeedList().SingleOrDefault(
                    t => t.ToString() == measurement.SoundSpeedCorrection);
                if (table != null)
                {
                    float v = table.LevelSpeedTable[measurement.AnnularPressure];
                    float xDiscrete = v / 341.33f;

                    for (int i = 0; i < measurement.Echogram.Length; i++)
                    {
                        var yDiscrete = -(measurement.Echogram[i] - 128);
                        bool negative = yDiscrete < 0 ? true : false;
                        yDiscrete = negative ? yDiscrete * -1 : yDiscrete;
                        var power = 1 / 0.35;
                        var yd = (double)yDiscrete;
                        double y = Math.Pow(yd, power);
                        y = negative ? y * -1 : y;
                        if (i == 0)
                        {
                            result[i, 0] = 0.0;
                            result[i, 1] = y;
                        }
                        else
                        {
                            result[i, 0] = result[i - 1, 0] + xDiscrete;
                            result[i, 1] = y;
                        }
                    }
                    return result;
                }
            }
            else
            {
                float v = Convert.ToSingle(measurement.SoundSpeed);
                float xDiscrete = 3000 * v / 341.33f;

                for (int i = 0; i < measurement.Echogram.Length; i++)
                {
                    var yDiscrete = -(measurement.Echogram[i] - 128);
                    double y = Math.Pow(yDiscrete, 1 / 0.35);
                    if (i == 0)
                    {
                        result[i, 0] = 0.0;
                        result[i, 1] = y;
                    }
                    else
                    {
                        result[i, 0] = result[i - 1, 0] + xDiscrete;
                        result[i, 1] = y;
                    }
                }
                return result;
            }
            return null;
        }
    }
}
