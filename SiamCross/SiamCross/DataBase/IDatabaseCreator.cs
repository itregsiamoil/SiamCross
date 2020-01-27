using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace SiamCross.DataBase
{
    [Preserve(AllMembers = true)]
    public interface IDatabaseCreator
    {
        void CreateDatabase(string patr);
    }
}
