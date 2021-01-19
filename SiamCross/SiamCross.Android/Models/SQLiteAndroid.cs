using Android.Runtime;
using SiamCross.DataBase;
using SiamCross.Droid.Models;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteAndroid))]
namespace SiamCross.Droid.Models
{
    [Preserve(AllMembers = true)]
    public class SQLiteAndroid : ISQLite
    {
        public SQLiteAndroid() { }
        public string GetDatabasePath(string sqliteFilename)
        {
            string documentsPath =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string path = Path.Combine(documentsPath, sqliteFilename);
            return path;
        }
    }
}