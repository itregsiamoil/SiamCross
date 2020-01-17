using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.DataBase
{
    public interface IDatabaseCreator
    {
        void CreateDatabase(string patr);
    }
}
