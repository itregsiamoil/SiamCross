//#define DEBUG_UNIT
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SiamCross.Models.Tools
{

    public class DataBuffer
    {
        private Object mLock = new Object();
        private CrcModbusCalculator _crcCalulator = new CrcModbusCalculator();
        private byte[] mBuff = new byte[512];
        private UInt32 mBuffLength = 0;
        private UInt32 DoGetLength()
        {
            lock (mLock)
            {
                return mBuffLength;
            }
        }
        public UInt32 Length { get => DoGetLength(); }

        public bool Append(byte[] src, int len)
        {
            lock (mLock)
                DoAppend(src.AsSpan().Slice(0, len));
            return true;
        }
        public bool Append(byte[] src)
        {
            lock (mLock)
            {
                DoAppend(src);
            }
            return true;
        }
        public void Clear()
        {
            lock (mLock)
            {
                DebugLog.WriteLine($"Clear buffer");
                mBuffLength = 0;
            }
        }
        public byte[] Extract()
        {
            lock (mLock)
            {
                return DoExtractPkg();
            }
        }
        public byte[] AppendAndExtract(byte[] inputBytes)
        {
            lock (mLock)
            {
                DoAppend(inputBytes);
                return DoExtractPkg();
            }
        }
        public byte[] DoExtractPkg()
        {
            UInt32 before = mBuffLength;
            byte[] ret = { };// TryGetPackage();
            UInt32 after = 0;
            while (0 == ret.Length && before != after)
            {
                before = mBuffLength;
                ret = TryGetPackage();
                after = mBuffLength;
            }
            return ret;
        }
        private void DoReduce(UInt32 qty)
        {
            UInt32 before = mBuffLength;
            if (mBuffLength < qty || mBuff.Length < qty)
            {
                DebugLog.WriteLine($"Buffer reduce error: {mBuffLength.ToString()} - {qty.ToString()}");
                mBuffLength = qty = 0;
            }
            // 0
            //mBuff.RemoveRange(0, qty);
            // 1
            //Array.Copy(mBuff, qty, mBuff, 0, mBuff.Length - qty);
            //mBuffLength -= qty;
            // 2
            mBuffLength -= qty;
            Span<byte> dst_buf = mBuff;
            Span<byte> src_buf = mBuff.AsSpan().Slice((int)qty, (int)mBuffLength);
            src_buf.CopyTo(dst_buf);
            DebugLog.WriteLine($"Reducing " + qty.ToString() + " bytes "
                + $"BuffLength {before.ToString()} -> {mBuffLength.ToString()}");
            //3
            //mBuffLength -= qty;
            //byte[] tmp = new byte[mBuffLength];
            //Array.Copy(mBuff, qty, tmp, 0, mBuffLength);
            //Array.Copy(tmp, 0, mBuff, 0, mBuffLength);

        }
        private void DoAppend(byte[] src)
        {
            DoAppend(src.AsSpan());
        }
        private void DoAppend(Span<byte> src)
        {
            UInt32 before = mBuffLength;

            int src_len = src.Length;
            if (src_len > mBuff.Length)
                src_len = mBuff.Length;

            int free_space = mBuff.Length - (int)mBuffLength;
            if (0 > free_space)
            {
                DebugLog.WriteLine($"Wrong buffer length is: " +
                    $"{mBuffLength.ToString()}");
                mBuffLength = 0;
            }

            if (src_len > free_space)
            {
                UInt32 reduce_size = (UInt32)src_len - (UInt32)free_space;
                DoReduce(reduce_size);
                DebugLog.WriteLine($"Buffer overrun: add" +
                    $" {src_len.ToString()}  length is {mBuffLength.ToString()}");
            }
            //Array.Copy(src, 0, mBuff, mBuffLength, src.Length);
            Span<byte> src_buf = src.Slice(0, src_len);
            Span<byte> dst_buf = mBuff.AsSpan().Slice((int)mBuffLength, (int)mBuff.Length - (int)mBuffLength);
            src_buf.CopyTo(dst_buf);
            mBuffLength += (UInt32)src_len;

            DebugLog.WriteLine($"Appending " + src.Length.ToString() +" bytes "
                 +$": [ {BitConverter.ToString(src.ToArray())} ] "
                 + $"BuffLength {before.ToString()} -> {mBuffLength.ToString()}");
        }
        private byte[] TryGetPackage()
        {
            ReadOnlySpan<byte> buf = mBuff.AsSpan().Slice(0, (int)mBuffLength);
            DebugLog.WriteLine($"TryGetPackage in : {BitConverter.ToString(buf.ToArray())}");

            const int min_qty = 12;
            UInt32 buf_qty = mBuffLength;
            if (min_qty > buf_qty)
                return new byte[] { };

            bool pkg_start_exist = false;
            UInt32 pkg_start = 0;
            // 
            for (UInt32 i = 0; i < buf_qty; ++i)
            {
                if (0x0D == mBuff[i])
                {
                    pkg_start = i;
                    pkg_start_exist = true;
                    break;
                }
            }
            if (!pkg_start_exist)
            {
                DoReduce(buf_qty);
                return new byte[] { };
            }
            if (min_qty-1 > (buf_qty - pkg_start - 1))
            {
                DebugLog.WriteLine($"Data required ");
                return new byte[] { };
            }
            if (0x0A != mBuff[pkg_start + 1])
            {
                DoReduce(pkg_start + 1);
                return new byte[] { };
            }
            // addr
            if (127 < mBuff[pkg_start + 2])
            {
                DoReduce(pkg_start + 2);
                return new byte[] { };
            }



            // check cmd
            //ReadOnlySpan<byte> cmd_arr = buf.Slice((int)pkg_start + 3, 1);
            byte cmd = mBuff[(int)pkg_start + 3];

            switch (cmd)
            {
                case 0x01:
                case 0x02:
                case 0x81:
                case 0x82:
                    break;
                default:
                    DoReduce(pkg_start + 2);
                    return new byte[] { };

            }

            
            //ReadOnlySpan<byte> addr_arr = buf.Slice((int)pkg_start + 4, 4);
            UInt32 addr = BitConverter.ToUInt32(mBuff, (int)pkg_start + 4);
            //ReadOnlySpan<byte> data_len_arr = buf.Slice((int)pkg_start + 8, 2);
            UInt16 data_len = BitConverter.ToUInt16(mBuff, (int)pkg_start + 8);
            //ReadOnlySpan<byte> rq_crc_arr = buf.Slice((int)pkg_start + 10, 2);
            UInt16 rq_crc = BitConverter.ToUInt16(mBuff, (int)pkg_start + 10);
            byte[] rq_calc_crc_arr = _crcCalulator.ModbusCrc(mBuff, (int)pkg_start + 2, 12 - 2 - 2);
            UInt16 rq_calc_crc = BitConverter.ToUInt16(rq_calc_crc_arr, 0);

            if (rq_calc_crc != rq_crc)
            {
                DebugLog.WriteLine($"Request CRC mismatch: {BitConverter.ToString(buf.ToArray())}");
                DoReduce(pkg_start + 2);
                return new byte[] { };
            }



            if (0x01 == cmd)
            {
                if (12 + data_len + 2 > (buf_qty - pkg_start))
                {
                    DebugLog.WriteLine($"Data required data_len=: "
                        + data_len.ToString()+ " buflen="+(buf_qty - pkg_start).ToString());
                    return new byte[] { };
                }
                    
                UInt32 pkg_len = (UInt32)12 + data_len + 2;
                UInt32 pkg_end = pkg_start + pkg_len;

                ReadOnlySpan<byte> rs_crc_arr = buf.Slice((int)pkg_start + 12 + data_len, 2);
                UInt16 rs_crc = BitConverter.ToUInt16(mBuff, (int)pkg_start + 12 + data_len);
                byte[] rs_calc_crc_arr = _crcCalulator.ModbusCrc(mBuff, (int)pkg_start + 12, data_len);
                UInt16 rs_calc_crc = BitConverter.ToUInt16(rs_calc_crc_arr, 0);


                if (rs_calc_crc != rs_crc)
                {
                    DebugLog.WriteLine($"Response CRC mismatch: {BitConverter.ToString(buf.ToArray())}");
                    DoReduce(pkg_start + 2);
                    return new byte[] { };
                }
                //byte[] pkg = buf.Slice((int)pkg_start, (int)pkg_len).ToArray();

                byte[] pkg = new byte[pkg_len];
                Array.Copy(mBuff, pkg_start, pkg, 0, (int)pkg_len);

                DebugLog.WriteLine($"pkg {cmd.ToString()} assembled");
                DoReduce(pkg_end);
                return pkg;
            }
            if (0x02 == mBuff[pkg_start + 3]
                || 0x81 == mBuff[pkg_start + 3] || 0x82 == mBuff[pkg_start + 3])
            {
                if (12 > (buf_qty - pkg_start))
                {
                    DebugLog.WriteLine($"Data required pkg_len < 12 "
                        + " buflen=" + (buf_qty - pkg_start).ToString());
                    return new byte[] { };
                }
                UInt32 pkg_len = 12;
                UInt32 pkg_end = pkg_start + pkg_len;
                //byte[] pkg = buf.Slice((int)pkg_start, (int)pkg_len).ToArray();
                byte[] pkg = new byte[pkg_len];
                Array.Copy(mBuff, pkg_start, pkg, 0, (int)pkg_len);
                //buf.Slice((int)pkg_start, (int)pkg_len).CopyTo(pkg);
                DebugLog.WriteLine($"pkg  assembled");
                DoReduce(pkg_end);
                //Array.Copy(mBuff, 0, mBuff, pkg_end, mBuff.Length - pkg_end);
                //mBuffLength -= pkg_end;
                return pkg;
            }

            DoReduce(pkg_start + 2);
            return new byte[] { };
        }

    }//DataBuffer

}
