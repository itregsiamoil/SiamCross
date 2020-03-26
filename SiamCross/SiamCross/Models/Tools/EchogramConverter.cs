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
                    float v = table.GetApproximatedSpeedFromTable(measurement.AnnularPressure); //
                    float xDiscrete = 3000 * v / 341.33f; // убрать 3к

                    for (int i = 0; i < measurement.Echogram.Length; i++)
                    {
                        var point = -(measurement.Echogram[i] - 128);
                        bool negative = point < 0 ? true : false;
                        point = negative ? point * -1 : point;
                        var power = 1 / 0.35;
                        var yd = (double)point;
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
                float xDiscrete = v / 341.33f;

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

        public static double[,] GetPoints(DuMeasurement duMeasurement)
        {
            double[,] result = new double[duMeasurement.Echogram.Length, 2];
            float tableSpeedCorrection = 0;
            float xDiscrete;
            if (duMeasurement.SoundSpeed == "")
            {
                var table = HandbookData.Instance.GetSoundSpeedList().SingleOrDefault(
                    t => t.ToString() == duMeasurement.SoundSpeedCorrection);
                tableSpeedCorrection = table.GetApproximatedSpeedFromTable(duMeasurement.AnnularPressure);
                xDiscrete = tableSpeedCorrection / Constants.EhoFixedSoundSpeed;
            }
            else
            {
                xDiscrete = Convert.ToSingle(duMeasurement.SoundSpeed) / Constants.EhoFixedSoundSpeed;
            }

            double xCoordinate = 0;
            for(int i = 0; i < duMeasurement.Echogram.Length; i++)
            {
                double yCoordinate = duMeasurement.Echogram[i] > 127 ?
                                  (duMeasurement.Echogram[i] - 128) * (-1) :
                                  duMeasurement.Echogram[i];
                //yCoordinate = yCoordinate < 0 ?
                //              Math.Pow(yCoordinate, 0.5 / 0.35) * (-1) :           // после возведения в степень знак пропадает поэтому дополнительно домножаем на (-1)
                //              Math.Pow(yCoordinate, 0.5 / 0.35);

                result[i, 0] = xCoordinate;
                result[i, 1] = yCoordinate;

                xCoordinate += xDiscrete;
            }

            //double[,] testArray = new double[duMeasurement.Echogram.Length, 2];
            //for (int i = 0; i < result.Length / 2; i++)
            //{
            //    testArray[i, 0] = result[i, 0];
            //    testArray[i, 1] = 0;
            //    if (i <= 100) testArray[i, 1] = 0;
            //    if (i > 100 && i <= 200) testArray[i, 1] = 10;
            //    if (i > 200 && i <= 300) testArray[i, 1] = 20;
            //    if (i > 300 && i <= 400) testArray[i, 1] = 30;
            //    if (i > 400 && i <= 500) testArray[i, 1] = 40;
            //    if (i > 500 && i <= 600) testArray[i, 1] = 50;
            //    if (i > 600 && i <= 700) testArray[i, 1] = 40;
            //    if (i > 700 && i <= 800) testArray[i, 1] = 30;
            //    if (i > 800 && i <= 900) testArray[i, 1] = 20;
            //    if (i > 900 && i <= 1000) testArray[i, 1] = 10;
            //    if (i > 1000 && i <= 1100) testArray[i, 1] = 0;
            //    if (i > 1100 && i <= 1200) testArray[i, 1] = -10;
            //    if (i > 1200 && i <= 1300) testArray[i, 1] = -20;
            //    if (i > 1300 && i <= 1400) testArray[i, 1] = -30;
            //    if (i > 1400 && i <= 1500) testArray[i, 1] = -40;
            //    if (i > 1500 && i <= 1600) testArray[i, 1] = -50;
            //    if (i > 1600 && i <= 1700) testArray[i, 1] = -40;
            //    if (i > 1700 && i <= 1800) testArray[i, 1] = -30;
            //    if (i > 1800 && i <= 1900) testArray[i, 1] = -20;
            //    if (i > 1900 && i <= 2000) testArray[i, 1] = -10;
            //    if (i > 2000 && i <= 2100) testArray[i, 1] = 0;
            //    if (i > 2100 && i <= 2200) testArray[i, 1] = 10;
            //    if (i > 2200 && i <= 2300) testArray[i, 1] = 20;
            //    if (i > 2300 && i <= 2400) testArray[i, 1] = 30;
            //    if (i > 2400 && i <= 2500) testArray[i, 1] = 40;
            //    if (i > 2500) testArray[i, 1] = 50;

            //}

            return result;
        }
    }
}
