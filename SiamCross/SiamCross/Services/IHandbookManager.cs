using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public interface IHandbookManager
    {
        Dictionary<string, int> LoadFields();
        void SaveFields(Dictionary<string, int> fieldDict);
    }
}
