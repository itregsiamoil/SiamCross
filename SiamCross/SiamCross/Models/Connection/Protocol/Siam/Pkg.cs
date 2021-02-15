using SiamCross.Models.Tools;
using System;
using System.Diagnostics;

namespace SiamCross.Models.Connection.Protocol.Siam
{
    public static class Pkg
    {
        public enum Command
        {
            Read = 0x01
          , Write = 0x02
        }

        public const int MAX_PKG_SIZE = 256;
        public const int MIN_PKG_SIZE = 12;
        private static readonly bool CHECK_RESPONSE_CRC = false;
        private static readonly byte[] begin_marker = { 0x0D, 0x0A };
        public static int GetDataLen(ReadOnlySpan<byte> req)
        {
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(req.Slice(8, 2));
        }

        public static Command GetCommand(ReadOnlySpan<byte> req)
        {
            return (Command)req[3];
        }
        public static int GetRespLen(ReadOnlySpan<byte> req)
        {
            if (req.Length < 3)
                return -1;
            switch (GetCommand(req))
            {
                default: return -1;
                case Command.Read:
                    int data_len = GetDataLen(req);
                    if (255 < data_len)
                        return -1;
                    return 12 + data_len + 2;
                case Command.Write:
                    return 12;
            }
        }
        private static bool IsNormalPkgHeader(ReadOnlySpan<byte> res, ReadOnlySpan<byte> req)
        {
            ReadOnlySpan<byte> req_hdr_data_part = req.Slice(2, 8);
            ReadOnlySpan<byte> res_hdr_data_part = res.Slice(2, 8);
            return res_hdr_data_part.SequenceEqual(req_hdr_data_part);
        }
        private static bool IsErrorPkgHeader(ReadOnlySpan<byte> res, ReadOnlySpan<byte> req)
        {
            byte[] err_pkg = new byte[8];
            req.Slice(2, 8).CopyTo(err_pkg);
            err_pkg[1] |= 0x80;

            ReadOnlySpan<byte> res_hdr_data_part = res.Slice(2, 8);
            return res_hdr_data_part.SequenceEqual(err_pkg);
        }
        private static int ExtractPkgData(ReadOnlySpan<byte> res_data, ReadOnlySpan<byte> req)
        {
            int need = GetRespLen(req) - 12 - res_data.Length;
            if (0 < need)
                return need;

            ReadOnlySpan<byte> data_and_crc = res_data.Slice(0, GetDataLen(req) + 2);

            if (0 == CrcModbusCalculator.Calc(data_and_crc))
                return 0;
            else
                return -1;
        }
        private static int ExtractErrPkgData(ReadOnlySpan<byte> res_data)
        {
            int need = 14 - res_data.Length;
            if (0 < need)
                return need;

            ReadOnlySpan<byte> data_and_crc = res_data.Slice(2, 12);

            if (0 == CrcModbusCalculator.Calc(data_and_crc))
                return -2;
            else
                return -1;
        }


        private static int Extract(ReadOnlySpan<byte> req, ReadOnlySpan<byte> res_span, ref int begin)
        {
            begin = 0;
            int marker_pos = 0;
            int rest = res_span.Length - begin;

            while (MIN_PKG_SIZE <= rest)
            {
                ReadOnlySpan<byte> pkg_hdr = res_span.Slice(begin, 12);
                if (IsNormalPkgHeader(pkg_hdr, req))
                {
                    switch (GetCommand(req))
                    {
                        case Command.Read:
                            if (CHECK_RESPONSE_CRC)
                                if (0 != CrcModbusCalculator.Calc(pkg_hdr.Slice(2)))
                                    return -1;
                            return ExtractPkgData(res_span.Slice(begin + 12), req);
                        case Command.Write:
                            if (0 != CrcModbusCalculator.Calc(pkg_hdr.Slice(2)))
                                return -1;
                            return 0;
                    }
                }
                if (IsErrorPkgHeader(pkg_hdr, req))
                {
                    return ExtractErrPkgData(res_span.Slice(begin, 14));
                }

                begin++;
                marker_pos = res_span.Slice(begin).IndexOf(begin_marker);
                if (-1 == marker_pos)
                {
                    begin = res_span.Length - 1;
                    return MIN_PKG_SIZE - 1;
                }
                begin += marker_pos;
                rest = res_span.Length - begin;
            }
            return MIN_PKG_SIZE - rest;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="req">массив запроса</param>
        /// <param name="res">массив ответа</param>
        /// <param name="begin">индекс откуда начинаются данные в массиве res
        /// при выполненни функции перемещается на начало пакета
        /// </param>
        /// <param name="end">индекс следующего за последним значащим в массиве res</param>
        /// <param name="need">количество необходибых байт для загрузки
        /// при выполненни функции устанавливается количество необходимых байт
        /// или -1 в случае ошибки CRC
        /// или -2 в случае обнаружения ошибкой
        /// </param>
        public static void Extract(ReadOnlySpan<byte> req, byte[] res, ref int begin, int end, ref int need)
        {
            int pos = 0;
            need = Extract(req, res.AsSpan(begin, end - begin), ref pos);
            begin += pos;
        }


        private static readonly byte[] rq = { 0x0D, 0x0A, 0x01, 0x01, 0x00, 0x10, 0x00, 0x00, 0x04, 0x00, 0x52, 0x04 };
        private static readonly byte[] rs = { 0x0D, 0x0A, 0x01, 0x01, 0x00, 0x10, 0x00, 0x00, 0x04, 0x00, 0x52, 0x04
                , 0x00, 0x05, 0x00, 0x00, 0x10, 0x25 };
        private static readonly byte[] rx_buf = new byte[512];

        public static bool Test()
        {
            try
            {
                Debug.Assert(Test_TrashPkg());
                Debug.Assert(Test_TrashPkg2());
                Debug.Assert(Test_TrashPkg3());
                Debug.Assert(Test_ErrorPkg_no_CRC());
                Debug.Assert(Test_ErrorPkg_with_wrong_CRC());
                Debug.Assert(Test_ErrorPkg());
                Debug.Assert(Test_Pkg_wrong_len());
                Debug.Assert(Test_Pkg_no_hdr_CRC());
                Debug.Assert(Test_Pkg_wrong_hdr_CRC());
                Debug.Assert(Test_Pkg_no_CRC_in_data());
                Debug.Assert(Test_Pkg_wrong_CRC_in_data());
                Debug.Assert(Test_Pkg());
                Debug.Assert(Test_Pkg1());
            }
            catch (Exception ex)
            {
                DebugLog.WriteLine("Exchange ERROR"
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                return false;
            }
            return true;

        }

        //! проверка на пакет c мусором
        private static bool Test_TrashPkg()
        {
            byte[] trash_rs = { 0xFD, 0xFA, 0xF1, 0xF1, 0xFD, 0xFA, 0x00, 0x00, 0x04, 0x00, 0x52, 0x04 };
            int begin = 0;
            int need = MIN_PKG_SIZE;
            trash_rs.CopyTo(rx_buf, 0);
            Extract(rq, rx_buf, ref begin, trash_rs.Length, ref need);
            return MIN_PKG_SIZE - 1 == need && trash_rs.Length - 1 == begin;
        }
        private static bool Test_TrashPkg2()
        {
            byte[] trash_rs = { 0x0D, 0x0A, 0xF1, 0xF1, 0xFD, 0xFA, 0x0D, 0x0A, 0x04, 0x00, 0x52, 0x0D };
            int begin = 0;
            int need = MIN_PKG_SIZE;
            trash_rs.CopyTo(rx_buf, 0);
            Extract(rq, rx_buf, ref begin, trash_rs.Length, ref need);
            return 1 == need && 1 == begin;
        }
        private static bool Test_TrashPkg3()
        {
            byte[] trash_rs = { 0x0D, 0x0A, 0xF1, 0xF1, 0xFD, 0xFA, 0xDD, 0xDA, 0x04, 0x00, 0x52, 0x01, 0x0D };
            int begin = 0;
            int need = MIN_PKG_SIZE;
            trash_rs.CopyTo(rx_buf, 0);
            Extract(rq, rx_buf, ref begin, trash_rs.Length, ref need);
            return 11 == need && 12 == begin;
        }
        //! проверка на пакет ошибку с мусором в начале и без CRC
        private static bool Test_ErrorPkg_no_CRC()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;
            byte[] err_rs = { 0x0D, 0x0A, 0x0D, 0x0A, 0x01, 0x81, 0x00, 0x10, 0x00, 0x00, 0x04, 0x00 };
            err_rs.CopyTo(rx_buf, 0);
            Extract(rq, rx_buf, ref begin, err_rs.Length, ref need);
            return 1 == need && 1 == begin;
        }
        //! проверка на пакет ошибку с кривым  CRC
        private static bool Test_ErrorPkg_with_wrong_CRC()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;
            byte[] err_rs = { 0x0D, 0x0A, 0x01, 0x81, 0x00, 0x10, 0x00, 0x00, 0x04, 0x00, 0xFF, 0xFF };
            err_rs.CopyTo(rx_buf, 0);
            Extract(rq, rx_buf, ref begin, err_rs.Length, ref need);
            return -1 == need && 0 == begin;
        }
        //! проверка на пакет ошибку с мусором в начале
        private static bool Test_ErrorPkg()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;
            byte[] err_rs = { 0x0D, 0x0A, 0x0D, 0x0A, 0x01, 0x81, 0x00, 0x10, 0x00, 0x00, 0x04, 0x00, 0xD3, 0xCC };
            err_rs.CopyTo(rx_buf, 0);
            Extract(rq, rx_buf, ref begin, err_rs.Length, ref need);
            return -2 == need && 2 == begin;
        }
        //! проверка на длину пакета 
        private static bool Test_Pkg_wrong_len()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;
            rs.AsSpan(0, 5).CopyTo(rx_buf.AsSpan());
            Extract(rq, rx_buf, ref begin, 5, ref need);
            return 12 - 5 == need && 0 == begin;
        }
        //! проверка нормальный пакет с мусором в начале и без CRC
        private static bool Test_Pkg_no_hdr_CRC()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;

            int trash_qty = 2;
            rs.AsSpan(0, 10).CopyTo(rx_buf.AsSpan(trash_qty));

            Extract(rq, rx_buf, ref begin, 12, ref need);
            return 1 == need && 1 == begin;

        }
        //! проверка нормальный пакет с мусором в начале и кривым CRC в запросе
        private static bool Test_Pkg_wrong_hdr_CRC()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;

            int trash_qty = 2;
            rs.AsSpan(0, 12).CopyTo(rx_buf.AsSpan(trash_qty));
            rx_buf[trash_qty + 10] = 0xFF;
            rx_buf[trash_qty + 11] = 0xFF;

            Extract(rq, rx_buf, ref begin, trash_qty + rs.Length, ref need);
            return -1 == need && trash_qty == begin;
        }
        //! проверка нормальный пакет с мусором в начале и без CRC в данных
        private static bool Test_Pkg_no_CRC_in_data()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;

            int trash_qty = 2;
            rs.AsSpan().CopyTo(rx_buf.AsSpan(trash_qty));

            Extract(rq, rx_buf, ref begin, rs.Length, ref need);
            return 2 == need && trash_qty == begin;
        }
        //! проверка нормальный пакет с мусором в начале и кривым CRC в данных
        private static bool Test_Pkg_wrong_CRC_in_data()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;
            int trash_qty = 2;
            rs.AsSpan().CopyTo(rx_buf.AsSpan(trash_qty));
            rx_buf[trash_qty + rs.Length - 2] = 0xFF;
            rx_buf[trash_qty + rs.Length - 1] = 0xFF;
            Extract(rq, rx_buf, ref begin, rs.Length + trash_qty, ref need);
            return -1 == need && trash_qty == begin;
        }

        private static bool Test_Pkg()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;
            int trash_qty = 2;
            rs.CopyTo(rx_buf, trash_qty);
            Extract(rq, rx_buf, ref begin, rs.Length + trash_qty, ref need);
            return 0 == need && trash_qty == begin;

        }

        private static bool Test_Pkg1()
        {
            int begin = 0;
            int need = MIN_PKG_SIZE;
            byte[] req = { 0x0D, 0x0A, 0x01, 0x01, 0x00, 0x10, 0x00, 0x00, 0x04, 0x00, 0x52, 0x04 };
            byte[] res = { 0x6E, 0x61, 0x6D, 0x65, 0x73, 0x65, 0x74, 0x20, 0x53, 0x49, 0x44, 0x44, 0x0D, 0x0A, 0x01, 0x01, 0x00, 0x10, 0x00, 0x00, 0x04, 0x00, 0x52 };

            res.CopyTo(rx_buf, 0);
            Extract(req, rx_buf, ref begin, res.Length, ref need);
            return 1 == need && 12 == begin;

        }
    }
}
