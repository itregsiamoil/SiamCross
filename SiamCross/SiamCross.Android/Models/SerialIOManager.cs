//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Hardware.Usb;
//using Android.OS;
//using Android.Runtime;
//using Android.Util;
//using Android.Views;
//using Android.Widget;
//using Hoho.Android.UsbSerial.Driver;
//using Hoho.Android.UsbSerial.Util;

//namespace SiamCross.Droid.Models
//{
//	public class SerialIOManager : IDisposable
//	{
//		static readonly string TAG = typeof(SerialIOManager).Name;
//		const int READ_WAIT_MILLIS = 200;
//		const int DEFAULT_BUFFERSIZE = 4096;
//		const int DEFAULT_BAUDRATE = 9600;
//		const int DEFAULT_DATABITS = 8;
//		const Parity DEFAULT_PARITY = Parity.None;
//		const StopBits DEFAULT_STOPBITS = StopBits.One;

//		readonly IUsbSerialPort port;
//		object syncState = new object();
//		byte[] buffer;
//		CancellationTokenSource cancelationTokenSource;
//		bool isOpen;

//		public SerialIOManager(IUsbSerialPort port)
//		{
//			this.port = port;
//			BaudRate = DEFAULT_BAUDRATE;
//			Parity = DEFAULT_PARITY;
//			DataBits = DEFAULT_DATABITS;
//			StopBits = DEFAULT_STOPBITS;
//		}

//		public int BaudRate { get; set; }

//		public Parity Parity { get; set; }

//		public int DataBits { get; set; }

//		public StopBits StopBits { get; set; }

//		public event EventHandler<SerialDataReceivedArgs> DataReceived;

//		public event EventHandler<UnhandledExceptionEventArgs> ErrorReceived;

//		public void Open(UsbManager usbManager, int bufferSize = DEFAULT_BUFFERSIZE)
//		{
//			if (disposed)
//				throw new ObjectDisposedException(GetType().Name);
//			if (IsOpen)
//				throw new InvalidOperationException();

//			var connection = usbManager.OpenDevice(port.Driver.Device);
//			if (connection == null)
//				throw new Java.IO.IOException("Failed to open device");
//			isOpen = true;

//			buffer = new byte[bufferSize];
//			port.Open(connection);
//			port.SetParameters(BaudRate, DataBits, StopBits, Parity);

//			cancelationTokenSource = new CancellationTokenSource();
//			var cancelationToken = cancelationTokenSource.Token;
//			cancelationToken.Register(() => Log.Info(TAG, "Cancellation Requested"));

//			Task.Run(() => {
//				Log.Info(TAG, "Task Started!");
//				try
//				{
//					while (true)
//					{
//						cancelationToken.ThrowIfCancellationRequested();

//						Step(); // execute step
//					}
//				}
//				catch (System.OperationCanceledException)
//				{
//					throw;
//				}
//				catch (Exception e)
//				{
//					Log.Warn(TAG, "Task ending due to exception: " + e.Message, e);
//					ErrorReceived.Raise(this, new UnhandledExceptionEventArgs(e, false));
//				}
//				finally
//				{
//					port.Close();
//					buffer = null;
//					isOpen = false;
//					Log.Info(TAG, "Task Ended!");
//				}
//			}, cancelationToken);
//		}

//		public void Close()
//		{
//			if (disposed)
//				throw new ObjectDisposedException(GetType().Name);
//			if (!IsOpen)
//				throw new InvalidOperationException();

//			// cancel task
//			cancelationTokenSource.Cancel();
//		}

//		public bool IsOpen
//		{
//			get
//			{
//				return isOpen;
//			}
//		}

//		void Step()
//		{
//			// handle incoming data.
//			var len = port.Read(buffer, 40);
//			if (len > 0)
//			{
//				Log.Debug(TAG, "Read data len=" + len);

//				var data = new byte[len];
//				Array.Copy(buffer, data, len);
//				DataReceived.Raise(this, new SerialDataReceivedArgs(data));
//			}
//		}

//		public int Write(String str, int timeout)
//		{
//			int bytesWritten = 0;
//			try
//			{
//				List<byte> buff = new List<byte>();
//				buff.AddRange(Encoding.ASCII.GetBytes(str));

//				bytesWritten = port.Write(buff.ToArray(), timeout);
//			}
//			catch (Exception ex)
//			{
//				System.Diagnostics.Debug.WriteLine(ex.Message + "\n");
//			}
//			return bytesWritten;
//		}

//		#region Dispose pattern implementation

//		bool disposed;

//		protected virtual void Dispose(bool disposing)
//		{
//			if (disposed)
//				return;

//			if (disposing)
//			{
//				Close();
//			}

//			disposed = true;
//		}

//		~SerialIOManager()
//		{
//			Dispose(false);
//		}

//		#region IDisposable implementation

//		public void Dispose()
//		{
//			Dispose(true);
//			GC.SuppressFinalize(this);
//		}

//		#endregion

//		#endregion

//	}
//}