using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiamCross.Models.Tools
{
    public static class DgmConverter
    {
        public static List<double[]> GetXY(List<byte[]> data,
                                       int weightDiscr,
                                       int stepDiscr)
        {
            var bytesList = new List<byte>();
            foreach (var bytes in data)
            {
                foreach (var b in bytes)
                {
                    bytesList.Add(b);
                }
            }

            short step = (short)(stepDiscr / 10);

            var points = GetXYFromBytes(bytesList);

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

            var min = points[0].Min();

            for (int j = 0; j < points.Count; j++)
            {
                points[j][0] = points[j][0] - min;
            }

            return points;
        }

        private static List<double[]> GetXYFromBytes(List<byte> data)
        {
            var result = new List<double[]>();
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

        public static double[,] GetXYs(List<byte> data,
                                     short stepDiscr,
                                     short weightDiscr)
        {
            return GetPoints2(data, stepDiscr, weightDiscr);
        }

        private static double[,] GetPoints2(List<byte> data, short stepDiscr, short weightDiscr)
        {
            short[] words = TransformRawBytes(data);
            //short step = (short)(stepDiscr / 10);
            short step = stepDiscr;
            int count = words.Count();
            double[,] points = new double[count, 2];

            for (int i = 0; i < count; i++)
            {
                //string binary = Convert.ToString(words[i], 2);
                //binary = AddZerosToBinary16Bits(binary);
                //string ySubstring = binary.Substring(5);
                //string xSubstring = binary.Substring(1, 5);
                //xSubstring = AddZerosToBinary(xSubstring);
                //long y = Convert.ToInt64(ySubstring);
                //long x = Convert.ToSByte(xSubstring);
                //if (binary[0] == '1')
                //{
                //    x *= -1;
                //}
                int x = ExtractTravel(words[i]);
                int y = ExtractWeight(words[i]);

                //Console.WriteLine($"Дискреты x: {x} y: {y}");

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
                {
                    points[i, 0] = ((x * step) / 1000f) + points[i - 1, 0];
                }
                points[i, 1] = Math.Abs((y * weightDiscr));
                //Console.WriteLine($"Точка {i}  x: {points[i, 0]} y: {points[i, 1]}");
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
                var bytes = new byte[2] { rawData[i], rawData[i + 1] };
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
