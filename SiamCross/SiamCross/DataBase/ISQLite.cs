using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.DataBase
{
    public interface ISQLite
    {
        string GetDatabasePath(string filename);
    }
}
