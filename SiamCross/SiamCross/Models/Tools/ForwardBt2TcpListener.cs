using SiamCross.Models.Connection.Phy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Tools
{
    public class ForwardBt2TcpListener : IDisposable
    {
        private readonly SemaphoreSlim Lock = new SemaphoreSlim(1);
        private Task<int> ForwarderTask;
        private CancellationTokenSource Cts;
        private IPhyConnection _PhyConnection;
        private int _Port = 55000;

        public ForwardBt2TcpListener()
        {

        }
        private static async Task<int> DataForwardTcp2Bt(NetworkStream stream, IPhyConnection conn, CancellationToken ct)
        {
            byte[] buffer = new byte[Constants.MAX_PKG_SIZE];
            while (!ct.IsCancellationRequested)
            {
                int qty = await stream.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                Debug.WriteLine("REQ [" + BitConverter.ToString(buffer, 0, qty) + "]\n");
                await conn.WriteAsync(buffer, 0, qty, ct).ConfigureAwait(false);
            }
            return 0;
        }
        private static async Task<int> DataForwardBtToTcp(NetworkStream stream, IPhyConnection conn, CancellationToken ct)
        {
            byte[] buffer = new byte[Constants.MAX_PKG_SIZE];
            while (!ct.IsCancellationRequested)
            {
                int qty = await conn.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                Debug.WriteLine("RESPONSE [" + BitConverter.ToString(buffer, 0, qty) + "]\n");
                await stream.WriteAsync(buffer, 0, qty, ct).ConfigureAwait(false);
            }
            return 0;
        }
        private async Task<int> ExecuteForwarding(NetworkStream stream, IPhyConnection conn, CancellationToken ct)
        {
            try
            {
                using (await Lock.UseWaitAsync(ct).ConfigureAwait(false))
                {
                    if (null == stream)
                        return -1;
                }
                List<Task> t = new List<Task>(2)
                {
                    Task.Run(async () => await DataForwardTcp2Bt(stream, conn, Cts.Token)),
                    Task.Run(async () => await DataForwardBtToTcp(stream, conn, Cts.Token))
                };
                await Task.WhenAll(t).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }
        private async Task<int> StartForward(CancellationToken ct)
        {
            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(IPAddress.Any, _Port);
                _Port++;
                // запуск слушателя
                server.Start();

                while (!ct.IsCancellationRequested)
                {
                    Debug.WriteLine("Await connecting... ");
                    TcpClient client = await server.AcceptTcpClientAsync().ConfigureAwait(false);
                    while (!ct.IsCancellationRequested
                        && !await _PhyConnection.Connect(ct).ConfigureAwait(false))
                    {
                        await Task.Delay(Constants.SecondDelay, ct).ConfigureAwait(false);
                    }
                    Debug.WriteLine("Client connected");
                    // получаем сетевой поток для чтения и записи
                    NetworkStream tcpStream = client.GetStream();
                    await ExecuteForwarding(tcpStream, _PhyConnection, ct).ConfigureAwait(false);
                    // закрываем поток
                    tcpStream.Close();
                    tcpStream = null;
                    // закрываем подключение
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return 1;
            }
            finally
            {
                if (server != null)
                    server.Stop();
            }
            return 0;
        }

        public async Task Start(IPhyConnection phy, int port, CancellationToken ct = default)
        {
            using (await Lock.UseWaitAsync(ct).ConfigureAwait(false))
            {
                Cts?.Cancel();
                await ForwarderTask;
                Cts = new CancellationTokenSource();
                _Port = port;
                _PhyConnection = phy;
                ForwarderTask = Task.Run(async () => await StartForward(Cts.Token));
            }
        }
        public async Task<int> Stop(CancellationToken ct = default)
        {
            using (await Lock.UseWaitAsync(ct).ConfigureAwait(false))
            {
                Cts?.Cancel();
                return await ForwarderTask;
            }
        }
        public async void Dispose()
        {
            await Stop();
        }
    }
}
