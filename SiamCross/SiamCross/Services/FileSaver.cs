﻿using SiamCross.Models.Tools;
using System.IO;
using System.Xml.Linq;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public class FileSaver
    {
        private IFileManager _fileManager;

        public FileSaver(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public void SaveXml(string filename, XDocument xml)
        {
            var path = Path.Combine(
                   System.Environment.GetFolderPath(
                       System.Environment.SpecialFolder.Personal), filename);
            xml.Save(path);
        }

        public void DeleteFile(string filename)
        {
            var path = Path.Combine(
                   System.Environment.GetFolderPath(
                       System.Environment.SpecialFolder.Personal), filename);

            if (!File.Exists(path)) return;

            File.Delete(path);
        }

        public string GetFilePath(string filename)
        {
            return Path.Combine(
                   System.Environment.GetFolderPath(
                       System.Environment.SpecialFolder.Personal), filename);
        }
    }
}
