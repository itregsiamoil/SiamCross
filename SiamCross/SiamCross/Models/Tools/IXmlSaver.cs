using System.Threading.Tasks;
using System.Xml.Linq;

namespace SiamCross.Models.Tools
{
    public interface IXmlSaver
    {
        Task UpdateStorageFolderAsync();
        Task<bool> SaveXml(string filename, XDocument xml);
        void DeleteXml(string filename);

        string GetFilepath(string filename);
    }
}
