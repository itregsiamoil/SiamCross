using System;

namespace SiamCross.Models.Connection.Protocol
{
    public class ProtocolException : Exception
    {
        public ProtocolException() { }
        public ProtocolException(string message) : base(message) { }
        public ProtocolException(string message, Exception inner) : base(message, inner) { }
    }

    public class IOTimeoutException : ProtocolException
    {
        public IOTimeoutException() { }
        public IOTimeoutException(string message) : base(message) { }
        public IOTimeoutException(string message, Exception inner) : base(message, inner) { }
    }

    public class IOErrPkgException : ProtocolException
    {
        public IOErrPkgException() { }
        public IOErrPkgException(string message) : base(message) { }
        public IOErrPkgException(string message, Exception inner) : base(message, inner) { }
    }
    public class IOCrcException : ProtocolException
    {
        public IOCrcException() { }
        public IOCrcException(string message) : base(message) { }
        public IOCrcException(string message, Exception inner) : base(message, inner) { }
    }

}
