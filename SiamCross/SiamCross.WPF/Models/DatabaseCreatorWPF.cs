using SiamCross.DataBase;
using System.Data.SQLite;

namespace SiamCross.WPF.Models
{
    public class DatabaseCreatorWPF : IDatabaseCreator
    {
        public void CreateDatabase(string patr)
        {
            SQLiteConnection.CreateFile(patr);
        }
    }
}
