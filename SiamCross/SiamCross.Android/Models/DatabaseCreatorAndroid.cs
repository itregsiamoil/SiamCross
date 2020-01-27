using Android.Runtime;
using Mono.Data.Sqlite;
using SiamCross.DataBase;
using SiamCross.Droid.Models;
using Xamarin.Forms;

[assembly: Dependency(typeof(DatabaseCreatorAndroid))]
namespace SiamCross.Droid.Models
{
    [Preserve(AllMembers = true)]
    public class DatabaseCreatorAndroid : IDatabaseCreator
    {
        public void CreateDatabase(string patr)
        {
            SqliteConnection.CreateFile(patr);
        }
    }
}
