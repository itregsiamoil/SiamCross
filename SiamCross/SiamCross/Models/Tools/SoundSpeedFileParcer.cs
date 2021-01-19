using System.Collections.Generic;

namespace SiamCross.Models.Tools
{
    public class SoundSpeedFileParcer
    {
        public List<KeyValuePair<float, float>> TryToParce(string fileText)
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
    }
}
