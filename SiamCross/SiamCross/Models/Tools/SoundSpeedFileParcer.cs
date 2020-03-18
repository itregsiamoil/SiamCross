using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class SoundSpeedFileParcer
    {
        public Dictionary<float, float> TryToParce(Stream stream)
        {
            var soundSpeedsDictionary = new Dictionary<float, float>();

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
                        soundSpeedsDictionary.Add(float.Parse(lineValues[1].Replace(".", ",")),
                            float.Parse(lineValues[2].Replace(".", ",")));
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return soundSpeedsDictionary;
        }
    }
}
