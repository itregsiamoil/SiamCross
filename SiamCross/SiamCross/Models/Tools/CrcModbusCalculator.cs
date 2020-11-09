using System;
using System.Collections.Generic;
using System.Text;

namespace SiamCross.Models.Tools
{
    public class CrcModbusCalculator
    {
        /// <summary>
        /// Расчет контрольной суммы
        /// </summary>
        /// <param name="buf"></param>
        /// <returns>Результат в виде реверсированного массива 2 байтов</returns>
        public byte[] ModbusCrc(byte[] buf, int start=0, int len= -1)
        {
            if(-1== len)
                len = buf.Length;
            UInt16 crc = 0xFFFF;

            for (int pos = start; pos < start+len; pos++)
            {
                crc ^= (UInt16)buf[pos];          // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }
            // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)

            return BitConverter.GetBytes(crc);
        }
    }
}
