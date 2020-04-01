﻿using SiamCross.AppObjects;
using System;
using System.Collections.Generic;
using Autofac;
using SiamCross.Services.Logging;
using System.Threading;
using System.Text;

namespace SiamCross.Models.Tools
{
    /// <summary>
    /// Буффер сообщений
    /// </summary>
    public class ByteBuffer
    {
        private static readonly NLog.Logger _logger = AppContainer.Container
            .Resolve<ILogManager>().GetLog();

        /// <summary>
        /// Буффер
        /// </summary>
        private List<byte> _byffer = new List<byte>();

        public List<byte> Buffer { get => _byffer; }

        /// <summary>
        /// Ожидаемый размер данных
        /// </summary>
        private int _dataSize = -1;

        /// <summary>
        /// Ожидаемый размер сообщения
        /// </summary>
        private int _expectedLength = -1;

        /// <summary>
        /// Добавить байты в буффер
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        public byte[] AddBytes(byte[] inputBytes)
        {
            if (_byffer.Count > 34)
            {
                _logger.Warn($"Buffer of parser is overflow! Buffer has been cleared! " +
                    $"Thread ID: {Thread.CurrentThread.Name}. Content before cleaning: " +
                    BitConverter.ToString(_byffer.ToArray()) + "\n");

                _byffer.Clear();
            }
            if (_byffer.Count == 0)                                       // Буффер пуст
            {
                if (inputBytes.Length >= 2)
                {
                    if (inputBytes[0] == 0x0D && inputBytes[1] == 0x0A)     // Пришло начало сообщения
                    {
                        _byffer.AddRange(inputBytes);
                    }
                }
                else if (inputBytes.Length == 1)
                {
                    if (inputBytes[0] == 0x0D)     // Пришло начало сообщения
                    {
                        _byffer.AddRange(inputBytes);
                    }
                }
            }
            else if (_byffer.Count == 1)
            {
                if (inputBytes[0] == 0x0A)
                {
                    _byffer.AddRange(inputBytes);
                }
            }
            else                                                       // Буффер не пуст
            {
                _byffer.AddRange(inputBytes);
            }

            // // //
            return ConditionalBufferReturn();
        }

        /// <summary>
        /// Метод возврата содержимого буффера, если сообщение полностью закончено
        /// </summary>
        /// <returns></returns>
        private byte[] ConditionalBufferReturn()
        {
            if (_byffer.Count >= 10)
            {
                _dataSize = _byffer[8] + _byffer[9] * 16;
                switch (_byffer[3])
                {
                    case 0x01:
                        _expectedLength = 14 + _dataSize;
                        break;
                    case 0x02:
                        _expectedLength = 12;
                        break;
                    case 0x81:
                        _expectedLength = 14;
                        break;
                }
            }

            if (_byffer.Count >= _expectedLength && _expectedLength != -1)
            {
                _dataSize = -1;
                _expectedLength = -1;
                var result = _byffer.ToArray();
                _byffer.Clear();
                //Console.WriteLine($"Answer message: {BitConverter.ToString(result)}");
                return result;
            }
            else
            {
                return new byte[] { };
            }
        }
    }
}
