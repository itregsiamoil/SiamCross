namespace SiamCross.Models.Tools
{
    internal struct IORestriction
    {
        public IORestriction(int to = 1000, int r = 1)
        {
            Timeout = 0;
            Retry = r;
        }

        public int Timeout;
        public int Retry;
    }

    internal struct Timeouts
    {
        public Timeouts(IORestriction r = default, IORestriction w = default)
        {
            Read = r;
            Write = w;
        }
        public IORestriction Read;
        public IORestriction Write;

    }

    internal class Transaction
    {
        public enum Status
        {
            Empty = 0,
            Ready = 1,
            Complete = 2,
            Failed = 10
        }
        public enum IOError
        {
            No = 0,
            IOError = 10,
            IOTransportError = 11,
            IOWriteTimeout = 12,
            IOReadTimeout = 13,
        }
        public enum PacketError
        {
            DvcErrorPacket = 0,
            DvcError = 20,
        }

        protected IProtocolConnection mTransport;
        protected Timeouts mTtimeout;

        public Transaction(IProtocolConnection transport, Timeouts to)
        {
            mTransport = transport;
            mTtimeout = to;
        }


        public Transaction(IProtocolConnection transport)
        {
            mTransport = transport;
            mTtimeout = new Timeouts(new IORestriction(3000, byte.MaxValue)
                                      , new IORestriction(2000, 3));
        }

        //private Memory<byte> mReq;
        //private Memory<byte> mRes;

    }
}
