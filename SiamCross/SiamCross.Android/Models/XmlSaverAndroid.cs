using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Android.App;
using Android.Content;
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
        private string _path = Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDownloads).AbsolutePath;

        public void DeleteXml(string filename)
        {
            string s = Directory.CreateDirectory(_path +
                (Path.DirectorySeparatorChar + "Measurements")).FullName;

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
            string s = Directory.CreateDirectory(_path + 
                (Path.DirectorySeparatorChar + "Measurements")).FullName;

            if (Directory.Exists(s))
            {
                xml.Save(s + (Path.DirectorySeparatorChar + filename));
            }
        }
    }
}