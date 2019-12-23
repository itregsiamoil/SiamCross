using SiamCross.DataBase;
using System.IO;
using Windows.Storage;

namespace SiamCross.WPF.Models
{
    public class SQLiteWPF : ISQLite
    {
        public SQLiteWPF() { }
        public string GetDatabasePath(string sqliteFilename)
        {
            // для доступа к файлам используем API Windows.Storage
            string path = Path.Combine(Directory.GetCurrentDirectory(), sqliteFilename);
            return path;
        }
    }
}
