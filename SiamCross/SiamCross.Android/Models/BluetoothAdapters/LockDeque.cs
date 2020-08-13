using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Javax.Net.Ssl;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

namespace SiamCross.Droid.Models.BluetoothAdapters
{
    public class Transaction 
    {
        public enum RetCode{ 
           retOk=0
           , ErrTimeout
           , ErrCRC
           , ErrLen
        };
        public RetCode mResult;

        public byte[] mRequest;
        public List<byte[]> mResponse = new List<byte[]>();

        Transaction(byte[] rq)
        {
            mRequest = rq;
        }
    };



    public class LockDeque
    {
        Queue<Transaction> mDq = new Queue<Transaction>();
        object lockObj = new object();

        public void Push(Transaction tr)
        {
            Monitor.Enter(lockObj);
            mDq.Enqueue(tr);
            Monitor.Exit(lockObj);
            Pushed?.Invoke();
        }

        public Transaction Pop()
        {
            Transaction tmp;
            Monitor.Enter(lockObj);
            tmp = mDq.Dequeue();
            Monitor.Exit(lockObj);
            return tmp;
        }

        public int Size()
        {
            int sz=0;
            Monitor.Enter(lockObj);
            sz = mDq.Count;
            Monitor.Exit(lockObj);
            return sz;
        }

        public event Action Pushed;
    }


    public class DequeExecute
    {
        public LockDeque mLockDeque = new LockDeque();
        public ICharacteristic Sender;
        
        object lockCurrTr = new object();

        Transaction mCurrTr;


        public DequeExecute() 
        {
            mLockDeque.Pushed += OnPushed;
        }

        public void OnResponse(byte[] data)
        {
            if (!Monitor.Wait(lockCurrTr) && mCurrTr != null)
            {
                mCurrTr.mResponse.Add(data);
                // TODO: проверить пакет и выставить результаты проверки
                mCurrTr.mResult = Transaction.RetCode.retOk;
                // TODO выполнить action об окончании транзакции

            }
        }

        public void Timeout()
        {
            if (!Monitor.Wait(lockCurrTr) && mCurrTr != null)
            {
                mCurrTr.mResult = Transaction.RetCode.ErrTimeout;
                // TODO выполнить action об окончании транзакции и передать куда-то
            }
                
            
        }

        void StartTimeout(int mills)
        {
            // TODO запустить таймер с событием -> Timeout()
        }


        public void OnPushed()
        {
            bool end = true;
            do
            {
                //Monitor.Wait(lockCurrTr);
                Monitor.Enter(lockCurrTr);
                mCurrTr = mLockDeque.Pop();
                if (mCurrTr!=null)
                {
                    StartTimeout(1000);
                    Sender?.WriteAsync(mCurrTr.mRequest);
                    Monitor.Exit(lockCurrTr);
                    // TODO: ждать события об окончании транзакции
                }
                else
                    Monitor.Exit(lockCurrTr);

                {
                    Monitor.Enter(lockCurrTr);
                    if (0 < mLockDeque.Size())
                    {
                        end = true;
                        mCurrTr = mLockDeque.Pop();
                    }
                    else
                    {
                        end = false;
                        mCurrTr = null;
                    }
                    Monitor.Exit(lockCurrTr);
                }
            } while (end);
        }
    }
}