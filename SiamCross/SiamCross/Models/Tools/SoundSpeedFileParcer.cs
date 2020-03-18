using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class SoundSpeedFileParcer
    {
        public List<KeyValuePair<float, float>> TryToParce(Stream stream)
        {
            var soundSpeedsList = new List<KeyValuePair<float, float>>();

            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                string[] lines = text.Split('\n');

                foreach(var line in lines)
                {
                    if(line.Length == 0)
                    {
                        continue;
                    }
                    List<string> lineValues = new List<string>(); 
                    foreach(var str in line.Split(' '))
                    {
                        if(str !="")
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
            }

            return soundSpeedsList;
        }
    }
}
