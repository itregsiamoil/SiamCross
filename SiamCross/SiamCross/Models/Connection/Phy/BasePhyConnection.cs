using SiamCross.Models.Adapters;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Phy
{
    public abstract class BasePhyConnection : IPhyConnection
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropChange(PropertyChangedEventArgs arg)
        {
            PropertyChanged?.Invoke(this, arg);
        }

        private ConnectionState _ConnState = ConnectionState.Disconnected;

        public ConnectionState State => _ConnState;
        protected void SetState(ConnectionState state)
        {
            if (state == _ConnState)
                return;
            _ConnState = state;
            OnPropChange(new PropertyChangedEventArgs(nameof(State)));
            Debug.WriteLine($"{this.GetType().Name} is {_ConnState}");
        }
        public abstract IPhyInterface PhyInterface { get; }
        public abstract int Rssi { get; }
        public abstract void UpdateRssi();
        public abstract void ClearRx();
        public abstract void ClearTx();
        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        public abstract Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        public abstract Task<bool> Connect();
        public abstract Task<bool> Disconnect();
    }
}
