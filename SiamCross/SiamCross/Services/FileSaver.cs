using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SiamCross.Services
{
    public class FileSaver
    {
        private IFileManager _fileManager;

        public FileSaver(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public void SaveXml(string filename, XDocument xml)
        {
            xml.Save(filename);
        }
    }
}
