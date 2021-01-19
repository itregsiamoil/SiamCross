using Android.Runtime;
using Mono.Data.Sqlite;
using SiamCross.DataBase;

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
