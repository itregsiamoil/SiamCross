using Android.Bluetooth;
using Android.Content;
using System;
using System.Diagnostics;

namespace SiamCross.Droid.Models
{

    public static class BtBroadcastReceiver
    {
        private static readonly BtDisconnectReceiver _BtReciever = new BtDisconnectReceiver();
        public static event Action<BluetoothDevice> OnDisconectedEvent;


        public static void NotifyDisconnected(BluetoothDevice dvc)
        {
            OnDisconectedEvent?.Invoke(dvc);
        }

        public static void Register()
        {
            MainActivity.CurrentActivity
                .RegisterReceiver(_BtReciever, new IntentFilter(BluetoothDevice.ActionAclDisconnected));
        }

        public static void Unregister()
        {
            MainActivity.CurrentActivity
                .UnregisterReceiver(_BtReciever);
        }
    }


    [BroadcastReceiver]
    /*
    [IntentFilter(new[] {
         BluetoothDevice.ActionAclConnected
        , BluetoothDevice.ActionAclDisconnected
        , BluetoothDevice.ActionAclDisconnectRequested
    })]
    */
    [Android.Runtime.Preserve(AllMembers = true)]
    internal class BtDisconnectReceiver : BroadcastReceiver
    {
        public BtDisconnectReceiver()
        {

        }
        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent?.Action;
            BluetoothDevice device = (BluetoothDevice)intent?.GetParcelableExtra(BluetoothDevice.ExtraDevice);
            string device_name = device?.Name;
            Debug.WriteLine($"OnReceive {device_name} action={action}");
            if (BluetoothDevice.ActionAclDisconnected.Equals(action))
            {
                BtBroadcastReceiver.NotifyDisconnected(device);
            }
        }



    }


}