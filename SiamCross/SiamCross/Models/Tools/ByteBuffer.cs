using SiamCross.AppObjects;
using System;
using System.Collections.Generic;
using Autofac;
using SiamCross.Services.Logging;
using System.Threading;
using System.Text;
using System.Linq;

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

        private CrcModbusCalculator _crcCalulator = new CrcModbusCalculator();

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
                byte[] calcedCrc;
                switch (_expectedLength)
                {
                    case 12:
                        calcedCrc = _crcCalulator.ModbusCrc(
                            new byte[] 
                            {
                                _byffer[2],
                                _byffer[3],
                                _byffer[4],
                                _byffer[5],
                                _byffer[6],
                                _byffer[7],
                                _byffer[8],
                                _byffer[9]
                            });
                        if(!calcedCrc.SequenceEqual(new byte[] { _byffer[10], _byffer[11]}))
                        {
                            _dataSize = -1;
                            _expectedLength = -1;
                            _byffer.Clear();
                            return new byte[] { };
                        }
                        break;
                    case 14:
                        calcedCrc = _crcCalulator.ModbusCrc(
                            new byte[]
                            {
                                _byffer[2],
                                _byffer[3],
                                _byffer[4],
                                _byffer[5],
                                _byffer[6],
                                _byffer[7],
                                _byffer[8],
                                _byffer[9],
                                _byffer[10],
                                _byffer[11],
                            });
                        if (!calcedCrc.SequenceEqual(new byte[] { _byffer[12], _byffer[13] }))
                        {
                            _dataSize = -1;
                            _expectedLength = -1;
                            _byffer.Clear();
                            return new byte[] { };
                        }
                        break;
                    default:
                        calcedCrc = _crcCalulator.ModbusCrc(
                           new byte[]
                           {
                                _byffer[2],
                                _byffer[3],
                                _byffer[4],
                                _byffer[5],
                                _byffer[6],
                                _byffer[7],
                                _byffer[8],
                                _byffer[9]
                           });
                        if (!calcedCrc.SequenceEqual(new byte[] { _byffer[10], _byffer[11] }))
                        {
                            _dataSize = -1;
                            _expectedLength = -1;
                            _byffer.Clear();
                            return new byte[] { };
                        }
                        var startDataIndex = _expectedLength - _dataSize - 2;
                        List<byte> payloadDataList = new List<byte>();
                        for(int i = startDataIndex; i < _expectedLength - 2; i++)
                        {
                            payloadDataList.Add(_byffer[i]);
                        }
                        calcedCrc = _crcCalulator.ModbusCrc(payloadDataList.ToArray());
                        if (!calcedCrc.SequenceEqual(new byte[] { _byffer[_expectedLength - 2],
                            _byffer[_expectedLength - 1] }))
                        {
                            _dataSize = -1;
                            _expectedLength = -1;
                            _byffer.Clear();
                            return new byte[] { };
                        }
                        break;
                }

                _dataSize = -1;
                _expectedLength = -1;
                var result = _byffer.ToArray();
                _byffer.Clear();               
                return result;
            }
            else
            {
                return new byte[] { };
            }
        }
    }
}
