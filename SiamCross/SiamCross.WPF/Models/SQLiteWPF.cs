using System.IO;
using Windows.Storage;

namespace SiamCross.WPF.Models
{
    public class SQLiteWPF
    {
        public SQLiteWPF() { }
        public string GetDatabasePath(string sqliteFilename)
        {
            // для доступа к файлам используем API Windows.Storage
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);
            return path;
        }
    }
}
