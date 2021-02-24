using System;
using System.Threading;

namespace SiamCross.Droid.Models
{
    internal static class EventHandlerExtensions
    {
        public static void Raise(this EventHandler handler, object sender, EventArgs e)
        {
            EventHandler temp = Volatile.Read(ref handler);
            temp?.Invoke(sender, e);
        }

        public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
        {
            EventHandler<T> temp = Volatile.Read(ref handler);
            temp?.Invoke(sender, e);
        }
    }
}