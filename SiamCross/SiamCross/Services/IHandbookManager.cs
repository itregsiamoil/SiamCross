using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    public interface IHandbookManager
    {
        Dictionary<string, int> LoadFields();
        void SaveFields(Dictionary<string, int> fieldDict);
    }
}
