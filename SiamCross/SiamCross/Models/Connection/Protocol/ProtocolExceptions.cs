using System;

namespace SiamCross.Models.Connection.Protocol
{
    public class IOTimeoutException : Exception
    {
        public IOTimeoutException() { }
        public IOTimeoutException(string message) : base(message) { }
        public IOTimeoutException(string message, Exception inner) : base(message, inner) { }
    }

    public class IOErrPkgException : Exception
    {
        public IOErrPkgException() { }
        public IOErrPkgException(string message) : base(message) { }
        public IOErrPkgException(string message, Exception inner) : base(message, inner) { }
    }
    public class IOCrcException : Exception
    {
        public IOCrcException() { }
        public IOCrcException(string message) : base(message) { }
        public IOCrcException(string message, Exception inner) : base(message, inner) { }
    }

}
