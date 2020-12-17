using SiamCross.Droid.Models;
using SiamCross.Models.Tools;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Forms;

[assembly: Dependency(typeof(XmlSaverAndroid))]
namespace SiamCross.Droid.Models
{
    public class XmlSaverAndroid : IXmlSaver
    {
        private readonly string _path = Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDownloads).AbsolutePath;

        private readonly string _folder = "Measurements";

        public void DeleteXml(string filename)
        {
            string s = Directory.CreateDirectory(_path +
                (Path.DirectorySeparatorChar + _folder)).FullName;

            if (Directory.Exists(s))
            {
                var filepath = s + Path.DirectorySeparatorChar + filename;
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
            }
        }
        public async Task<bool> SaveXml(string filename, XDocument xml)
        {
            //string s = Directory.CreateDirectory(_path + 
            //    (Path.DirectorySeparatorChar + _folder)).FullName;
            string s = Directory.CreateDirectory(@"/storage/emulated/0/" + (Path.DirectorySeparatorChar + _folder)).FullName;
            if (!Directory.Exists(s))
                return false;

            var fullPath = s + (Path.DirectorySeparatorChar + filename);
            FileStream fs = TryCreateFileStream(fullPath);
            if(null==fs)
                return false;
            //File.SetAttributes(fullPath, FileAttributes.Normal);
            //FileIOPermission filePermission =new FileIOPermission(FileIOPermissionAccess.AllAccess, fullPath);
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Async = true;
            System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(fs, settings);
            CancellationTokenSource cts = new CancellationTokenSource(3000);
            await xml.SaveAsync(writer, cts.Token);
            //MediaScannerConnection.ScanFile(Android.App.Application.Context, new String[] { fullPath }, null, null);
            await fs.FlushAsync();
            writer?.Dispose();
            await fs.DisposeAsync();
            cts?.Dispose();
            return true;
        }
        FileStream TryCreateFileStream(string filename)
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
        /// <summary>
        /// Получить путь к файлу
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <returns>Путь к файлу или null</returns>
        public string GetFilepath(string filename)
        {
            string result = null;

            //var path = _path + 
            //    Path.DirectorySeparatorChar +
            //    _folder + Path.DirectorySeparatorChar 
            //    + filename;
            string directory = Directory.CreateDirectory(
                @"/storage/emulated/0/" + (Path.DirectorySeparatorChar + _folder)).FullName;
            string path = directory + (Path.DirectorySeparatorChar + filename);
            if (File.Exists(path))
                result = path;

            return result;
        }
    }
}