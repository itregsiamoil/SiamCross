using System.Xml.Linq;

namespace SiamCross.Models.Tools
{
    public interface IXmlSaver
    {
        void SaveXml(string filename, XDocument xml);
        void DeleteXml(string filename);
    }
}
