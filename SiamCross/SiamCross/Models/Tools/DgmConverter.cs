using System;
using System.Collections.Generic;
using System.Linq;

namespace SiamCross.Models.Tools
{
    public static class DgmConverter
    {
        public static List<double[]> GetXY(List<byte[]> data,
                                       int weightDiscr,
                                       int stepDiscr)
        {
            List<byte> bytesList = new List<byte>();
            foreach (byte[] bytes in data)
            {
                foreach (byte b in bytes)
                {
                    bytesList.Add(b);
                }
            }

            short step = (short)(stepDiscr / 10);

            List<double[]> points = GetXYFromBytes(bytesList);

            for (int i = 0; i < points.Count; i++)
            {
                // Рассчитываем Y
                points[i][1] = Math.Abs((points[i][1] * weightDiscr) / 1000d);
                if (i == 0)
                {
                    points[i][0] = (points[i][0] * step) / 1000d;
                }
                else
                {
                    // Рассчитываем X
                    points[i][0] = (points[i][0] * step) / 1000d + points[i - 1][0];
                }
            }

            double min = points[0].Min();

            for (int j = 0; j < points.Count; j++)
            {
                points[j][0] = points[j][0] - min;
            }

            return points;
        }

        private static List<double[]> GetXYFromBytes(List<byte> data)
        {
            List<double[]> result = new List<double[]>();
            for (int i = 0; i < data.Count() / 2; i++)
            {
                int index = i * 2;
                string byte1 = Convert.ToString(data[index], 2);
                string byte2 = Convert.ToString(data[index + 1], 2);
                //var bytes = new byte[2] { data[i], data[i+1] };
                //var fullWord = BitConverter.ToUInt16(bytes, 0);
                //var fullBinary = Convert.ToString(fullWord, 2);
                //fullBinary = AddZerosToBinary16Bits(fullBinary);
                //Console.WriteLine(fullBinary);
                //string x = fullBinary.Substring(10, 6);
                //string y = fullBinary.Substring(0, 10);

                byte1 = AddZerosToBinary(byte1);
                byte2 = AddZerosToBinary(byte2);
                string fullByte = byte1 + byte2;
                Console.WriteLine(fullByte);
                string x = fullByte.Substring(10, 6);
                string y = fullByte.Substring(0, 10);
                result.Add(new double[2] { Convert.ToUInt16(x, 2), Convert.ToUInt16(y, 2) });
            }
            return result;
        }

        public static double[,] GetXYs(List<byte> data
            , UInt16 stepDiscr
            , UInt16 weightDiscr
            , UInt16 qty
            , out float min_x, out float max_x
            , out float min_y, out float max_y)
        {
            min_x = float.MaxValue;
            min_y = float.MaxValue;
            max_x = float.MinValue;
            max_y = float.MinValue;

            short[] words = TransformRawBytes(data);
            UInt16 step = stepDiscr;
            int count = words.Count();
            double[,] points = new double[count, 2];

            //double min = Double.MaxValue;

            for (int i = 0; i < qty; i++)
            {
                int x = ExtractTravel(words[i]);
                int y = ExtractWeight(words[i]);

                double prev_val = (0 == i) ? 0 : points[i - 1, 0];
                points[i, 0] = ((x * step) / 1000f) + prev_val;
                points[i, 1] = Math.Abs((y * weightDiscr));

                if (points[i, 0] < min_x)
                    min_x = (float)points[i, 0];
                if (points[i, 1] < min_y)
                    min_y = (float)points[i, 1];
                if (points[i, 0] > max_x)
                    max_x = (float)points[i, 0];
                if (points[i, 1] > max_y)
                    max_y = (float)points[i, 1];

            }
            return points;
        }
        public static double[,] GetXYs(List<byte> data,
                             UInt16 stepDiscr,
                             UInt16 weightDiscr)
        {
            return GetPoints2(data, stepDiscr, weightDiscr);
        }
        private static double[,] GetPoints2(List<byte> data, UInt16 stepDiscr, UInt16 weightDiscr)
        {
            short[] words = TransformRawBytes(data);
            UInt16 step = stepDiscr;
            int count = words.Count();
            double[,] points = new double[count, 2];

            double min = Double.MaxValue;

            for (int i = 0; i < count; i++)
            {
                int x = ExtractTravel(words[i]);
                int y = ExtractWeight(words[i]);
                /*
                if (i == 0)
                {
                    points[i, 0] = 0;
                    points[i, 1] = 0;
                }
                else if (i == 1)
                {
                    points[i, 0] = (x * step) / 1000f;
                }
                else
                */
                {
                    double prev_val = (0 == i) ? 0 : points[i - 1, 0];
                    points[i, 0] = ((x * step) / 1000f) + prev_val;
                }
                points[i, 1] = Math.Abs((y * weightDiscr));

                if (points[i, 0] < min)
                    min = points[i, 0];
            }

            for (int i = 0; i < count; i++)
            {
                points[i, 0] = points[i, 0] - min;
            }

            return points;
        }
        private static double[,] GetPoints(List<byte> data, short stepDiscr, short weightDiscr)
        {
            short[] words = TransformRawBytes(data);
            short step = (short)(stepDiscr / 10);
            int count = words.Count();
            double[,] points = new double[count, 2];
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                {
                    points[i, 0] = (ExtractTravel(words[i]) * step) / 1000f;
                }
                else
                {
                    points[i, 0] = (ExtractTravel(words[i]) * step) / 1000f + points[i - 1, 0];
                }
                points[i, 1] = (ExtractWeight(words[i]) * weightDiscr) / 1000f;
            }

            double min = Double.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (min > points[i, 0])
                {
                    min = points[i, 0];
                }
            }

            for (int i = 0; i < count; i++)
            {
                points[i, 0] = points[i, 0] - min;
            }

            return points;
        }

        private static short[] TransformRawBytes(List<byte> rawData)
        {
            List<short> words = new List<short>();
            for (int i = 0; i + 1 < rawData.Count; i += 2)
            {
                byte[] bytes = new byte[2] { rawData[i], rawData[i + 1] };
                short word = BitConverter.ToInt16(bytes, 0);
                words.Add(word);
            }
            return words.ToArray();
        }

        private static string AddZerosToBinary16Bits(string binary)
        {
            if (binary.Length < 16)
            {
                while (binary.Length < 16)
                {
                    binary = binary.Insert(0, "0");
                }
            }
            return binary;
        }

        private static string AddZerosToBinary(string binary)
        {
            if (binary.Length < 8)
            {
                while (binary.Length < 8)
                {
                    binary = binary.Insert(0, "0");
                }
            }
            return binary;
        }

        private static int ExtractWeight(short number)
        {
            short mask = 1023;
            return number & mask;
        }

        private static int ExtractTravel(short number)
        {
            int mask = 64512;
            int result = (number & mask) >> 10;
            if (result > 31)
            {
                result = result - 64;
            }
            return result;
        }
    }
}
