﻿using SiamCross.Models.Adapters;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Phy
{
    public abstract class BasePhyConnection : IPhyConnection
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void ChangeNotify([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private ConnectionState _ConnState = ConnectionState.Disconnected;

        public ConnectionState State => _ConnState;
        protected void SetState(ConnectionState state)
        {
            if (state == _ConnState)
                return;
            _ConnState = state;
            ChangeNotify(nameof(State));
            Debug.WriteLine($"{GetType().Name} is {_ConnState}");
        }
        protected void CheckConnection()
        {
            if (ConnectionState.Connected != State)
                throw new IOException("Not connected device in " + PhyInterface.Name);
        }
        public abstract IPhyInterface PhyInterface { get; }
        public abstract int Rssi { get; }
        public abstract int Mtu { get; }
        public abstract Task UpdateRssi();
        public abstract Task ClearRx();
        public abstract Task ClearTx();
        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        public abstract Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
        public abstract Task<bool> Connect(CancellationToken ct);
        public abstract Task<bool> Disconnect();
        public abstract void Dispose();

    }
}
