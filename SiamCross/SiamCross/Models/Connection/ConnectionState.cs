namespace SiamCross.Models.Connection
{
    public enum ConnectionState
    {
        Disconnected = 0,
        PendingConnect = 1,
        Connected = 2,
        PendingDisconnect = 3
    }
    public static class ConnectionStateAdapter
    {
        public static string ToString(ConnectionState conn)
        {
            switch (conn)
            {
                default:
                case ConnectionState.Disconnected: return Resource.StatConn_Disconnected;
                case ConnectionState.PendingConnect: return Resource.StatConn_PendingConnect;
                case ConnectionState.Connected: return Resource.StatConn_Connected;
                case ConnectionState.PendingDisconnect: return Resource.StatConn_PendingDisconnect;
            }
        }
    }
}
