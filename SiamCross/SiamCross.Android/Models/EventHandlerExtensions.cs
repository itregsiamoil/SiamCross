using System;
using System.Threading;

namespace SiamCross.Droid.Models
{
	static class EventHandlerExtensions
	{
		public static void Raise(this EventHandler handler, object sender, EventArgs e)
		{
			var temp = Volatile.Read(ref handler);
			if (temp != null)
				temp(sender, e);
		}

		public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
		{
			var temp = Volatile.Read(ref handler);
			if (temp != null)
				temp(sender, e);
		}
	}
}