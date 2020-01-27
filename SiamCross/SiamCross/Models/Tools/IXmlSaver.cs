using System.Xml.Linq;
using Xamarin.Forms.Internals;

namespace SiamCross.Models.Tools
{
    [Preserve(AllMembers = true)]
    public interface IXmlSaver
    {
        void SaveXml(string filename, XDocument xml);
        void DeleteXml(string filename);

        string GetFilepath(string filename);
    }
}
