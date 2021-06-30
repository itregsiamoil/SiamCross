using System.Collections.Generic;

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
                    soundSpeedsList.Add(
                        new KeyValuePair<float, float>(
                            float.Parse(lineValues[1].Replace(".", ",")),
                            float.Parse(lineValues[2].Replace(".", ","))));
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
                str += $" 0 {item.Key} {item.Value} \r\n";
            return str;
        }
    }
}
