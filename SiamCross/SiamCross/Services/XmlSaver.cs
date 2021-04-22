using SiamCross.Services.Environment;
using SiamCross.Services.MediaScanner;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SiamCross.Services
{
    public class XmlSaver
    {
        public static async Task<string> SaveXml(string filename, XDocument xml)
        {
            string path = EnvironmentService.Instance.GetDir_Measurements();

            //string s = Directory.CreateDirectory(_path + 
            //    (Path.DirectorySeparatorChar + _folder)).FullName;
            string s = Directory.CreateDirectory(path).FullName;
            if (!Directory.Exists(s))
                return null;

            string fullPath = Path.Combine(path, filename);

            FileStream fs = await TryCreateFileStream(fullPath);
            if (null == fs)
                return null;
            using (fs)
            {
                System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
                {
                    Async = false,
                    OmitXmlDeclaration = true,
                    Indent = true,
                    NewLineOnAttributes = true
                };

                using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fs, settings))
                {
                    xml.Save(writer);
                    writer.Flush();
                    writer.Close();
                }

                await fs.FlushAsync();
                fs.Close();
            }

            await MediaScannerService.Instance.Scan(fullPath);
            return fullPath;
        }
        static async Task<FileStream> TryCreateFileStream(string filename)
        {
            FileStream fs = null;
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                    await MediaScannerService.Instance.Scan(filename);
                }
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EXCEPTION {ex.Message} {ex.GetType()}\n{ex.StackTrace}");
            }
            return fs;
        }
    }
}