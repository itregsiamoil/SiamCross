using SiamCross.Models.Connection.Phy;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Connection.Protocol
{
    public abstract class BaseProtocol : IProtocolConnection
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropChange(PropertyChangedEventArgs arg)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }

        private ConnectionState _ConnState = ConnectionState.Disconnected;
        protected void SetState(ConnectionState state)
        {
            _ConnState = state;
            OnPropChange(new PropertyChangedEventArgs(nameof(State)));
        }

        public ConnectionState State => _ConnState;


        protected IPhyConnection mPhyConn = null;
        public IPhyConnection PhyConnection => mPhyConn;
        public ushort MaxReqLen { get; set; }

        private byte _Address = 1;
        public byte Address 
        {
            get => _Address;
            set 
            { 
                if(0 < value &&  128 > value)
                {
                    _Address = value;
                    OnPropChange(new PropertyChangedEventArgs(nameof(Address)));
                }
            } 
        }

        public BaseProtocol(IPhyConnection base_conn, byte address)
        {
            _Address = address;
            mPhyConn = base_conn;
            MaxReqLen = 200;
        }
        public async Task<bool> Connect()
        {
            if (ConnectionState.Connected == mPhyConn.State)
                return true;
            
            SetState(ConnectionState.PendingConnect);

            bool result = false;
            try
            {
                result = await mPhyConn.Connect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                result = false;
                await Disconnect();
            }
            finally
            {
                if (result)
                {
                    if (ConnectionState.Connected != State)
                        SetState(ConnectionState.Connected);
                }
                else
                {
                    if (ConnectionState.Disconnected != State)
                        SetState(ConnectionState.Disconnected);
                }

            }
            return result;
        }
        public async Task<bool> Disconnect()
        {
            if (ConnectionState.Disconnected == mPhyConn.State)
                return true;

            bool ret = false;
            SetState(ConnectionState.PendingDisconnect);
            try
            {
                ret = await mPhyConn.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            finally
            {
                SetState(ConnectionState.Disconnected);
            }
            return ret;
        }



        public abstract Task<byte[]> Exchange(byte[] req);
        public abstract Task<bool> ReadMemoryAsync(uint addr, uint len
            , byte[] dst, int dst_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default);

        public abstract Task<bool> WriteMemoryAsync(uint addr, uint len
            , byte[] src, int src_start = 0
            , Action<float> onStepProgress = null, CancellationToken cancellationToken = default);

    }
}
