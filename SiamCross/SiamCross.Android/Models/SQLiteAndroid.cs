using System.IO;
using SiamCross.DataBase;
using SiamCross.Droid.Models;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteAndroid))]
namespace SiamCross.Droid.Models
{
    public class SQLiteAndroid : ISQLite
    {
        public SQLiteAndroid() { }
        public string GetDatabasePath(string sqliteFilename)
        {
            string documentsPath =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, sqliteFilename);
            return path;
        }
    }
}