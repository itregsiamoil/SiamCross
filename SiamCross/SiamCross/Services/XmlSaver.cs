using SiamCross.Services.Environment;
using System;
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
            FileStream fs = TryCreateFileStream(fullPath);
            if (null == fs)
                return null;
            //File.SetAttributes(fullPath, FileAttributes.Normal);
            //FileIOPermission filePermission =new FileIOPermission(FileIOPermissionAccess.AllAccess, fullPath);
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings
            {
                Async = true,
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = true
            };

            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fs, settings);
            xml.Save(writer);
            //MediaScannerConnection.ScanFile(Android.App.Application.Context, new String[] { fullPath }, null, null);
            await writer?.FlushAsync();
            await fs.FlushAsync();
            writer?.Dispose();
            fs.Dispose();
            return fullPath;
        }
        private static FileStream TryCreateFileStream(string filename)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch (Exception)
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                    fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                }
            }
            return fs;
        }
    }
}