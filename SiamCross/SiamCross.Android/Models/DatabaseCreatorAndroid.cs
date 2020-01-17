using Mono.Data.Sqlite;
using SiamCross.DataBase;

namespace SiamCross.Droid.Models
{
    public class DatabaseCreatorAndroid : IDatabaseCreator
    {
        public void CreateDatabase(string patr)
        {
            SqliteConnection.CreateFile(patr);
        }
    }
}
