using System.Collections.Generic;
using System.Globalization;

namespace SiamCross.Models.Tools
{
    public static class SoundSpeedParser
    {
        public static List<KeyValuePair<float, float>> ToList(string fileText)
        {
            List<KeyValuePair<float, float>> soundSpeedsList = new List<KeyValuePair<float, float>>();

            string[] lines = fileText.Split('\n');

            foreach (string line in lines)
            {
                if (line.Length == 0)
                {
                    continue;
                }
                List<string> lineValues = new List<string>();
                foreach (string str in line.Split(' '))
                {
                    if (str != "")
                    {
                        lineValues.Add(str);
                    }
                }
                try
                {
                    var val1 = float.Parse(lineValues[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                    var val2 = float.Parse(lineValues[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                    soundSpeedsList.Add(new KeyValuePair<float, float>(val1, val2));
                }
                catch
                {
                    return null;
                }
            }
            return soundSpeedsList.Count != 0 ? soundSpeedsList : null;
        }
        public static string ToString(List<KeyValuePair<float, float>> values)
        {
            string str = string.Empty;
            foreach (var item in values)
                str += $" 0 "
                    + item.Key.ToString(CultureInfo.InvariantCulture)
                    + " "
                    + item.Value.ToString(CultureInfo.InvariantCulture)
                    + "\r\n";
            return str;
        }
    }
}
