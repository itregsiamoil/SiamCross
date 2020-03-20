using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Services.UserOutput
{
    public interface IFileOpenDialog
    {
        Task<string> Show();
    }
}
