using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection
{
    public interface IConnection : INotifyPropertyChanged, IDisposable
    {
        ConnectionState State { get; }
        Task<bool> Connect(CancellationToken ct);
        Task<bool> Disconnect();
    }
}
