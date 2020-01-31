using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SiamCross.Droid.Models;
using SiamCross.Models.Tools;
using Xamarin.Forms;

[assembly: Dependency(typeof(XmlSaverAndroid))]
namespace SiamCross.Droid.Models
{
    public class XmlSaverAndroid : IXmlSaver
    {
        private readonly string _path = Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDownloads).AbsolutePath;

        private readonly string _folder = "Measurements";

        private readonly object _locker = new object();

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

        public void SaveXml(string filename, XDocument xml)
        {
            //string s = Directory.CreateDirectory(_path + 
            //    (Path.DirectorySeparatorChar + _folder)).FullName;
            string s = Directory.CreateDirectory(@"/storage/emulated/0/" + (Path.DirectorySeparatorChar + _folder)).FullName;


            if (Directory.Exists(s))
            {
                var fullPath = s + (Path.DirectorySeparatorChar + filename);
                xml.Save(fullPath);
                MediaScannerConnection.ScanFile(Android.App.Application.Context, new String[] { fullPath }, null, null);
            }

            //string s1 = Directory.CreateDirectory(_path +
            //    (Path.DirectorySeparatorChar + _folder)).FullName;

            //if (Directory.Exists(s1))
            //{
            //    var fullPath1 = s1 + (Path.DirectorySeparatorChar + filename);
            //    xml.Save(fullPath1);
            //    MediaScannerConnection.ScanFile(Android.App.Application.Context, new String[] { fullPath1 }, null, null);
            //}
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
            string path = Directory.CreateDirectory(
                @"/storage/emulated/0/" + (Path.DirectorySeparatorChar + _folder)).FullName;

            if (File.Exists(path))
                result = path;

            return result;
        }
    }
}