using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services;
using System;
using System.Linq;

namespace SiamCross.Models.Tools
{
    public static class EchogramConverter
    {
        public static double[,] GetPoints(DuMeasurement duMeasurement
            , out float min_x, out float max_x
            , out float min_y, out float max_y)
        {
            min_x = float.MaxValue;
            min_y = float.MaxValue;
            max_x = float.MinValue;
            max_y = float.MinValue;

            if (null == duMeasurement.Echogram)
                return new double[0, 2];

            double[,] points = new double[duMeasurement.Echogram.Length, 2];
            float tableSpeedCorrection = 0;
            float xDiscrete;
            if (duMeasurement.SoundSpeed == "")
            {
                SoundSpeedModel table = HandbookData.Instance.GetSoundSpeedList().SingleOrDefault(
                    t => t.ToString() == duMeasurement.SoundSpeedCorrection);
                tableSpeedCorrection = table.GetApproximatedSpeedFromTable(duMeasurement.AnnularPressure);
                xDiscrete = tableSpeedCorrection / Constants.EhoFixedSoundSpeed;
            }
            else
            {
                xDiscrete = Convert.ToSingle(duMeasurement.SoundSpeed) / Constants.EhoFixedSoundSpeed;
            }

            double xCoordinate = 0;
            for (int i = 0; i < duMeasurement.Echogram.Length; i++)
            {
                double yCoordinate = 0d;
                int multipler = 1;
                if (duMeasurement.Echogram[i] > 127)
                {
                    multipler = -1;
                    yCoordinate = duMeasurement.Echogram[i] - 127;
                }
                else
                {
                    yCoordinate = duMeasurement.Echogram[i];
                }
                //yCoordinate = Math.Pow(yCoordinate, 0.5f / 0.35)* multipler;
                yCoordinate = yCoordinate * multipler;

                points[i, 0] = xCoordinate;
                points[i, 1] = yCoordinate;

                xCoordinate += xDiscrete;

                if (points[i, 0] < min_x)
                    min_x = (float)points[i, 0];
                if (points[i, 1] < min_y)
                    min_y = (float)points[i, 1];
                if (points[i, 0] > max_x)
                    max_x = (float)points[i, 0];
                if (points[i, 1] > max_y)
                    max_y = (float)points[i, 1];
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

            return points;
        }
    }
}
