using System.ComponentModel;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection
{
    public interface IConnection : INotifyPropertyChanged
    {
        ConnectionState State { get; }
        Task<bool> Connect();
        Task<bool> Disconnect();
    }
}
