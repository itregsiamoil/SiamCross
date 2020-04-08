﻿using SiamCross.Models.Sensors.Dynamographs.Shared;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors
{

    /// <summary>
    /// Определитель версии прошивки
    /// </summary>
    public class FirmWaveQualifier
    {

        private Func<byte[], Task> QueryParamerter;

        private byte[] _programmVersionAddressCommand;
        private byte[] _programmVersionSizeCommand;

        /// <summary>
        /// Адресс версии программы
        /// </summary>
        private byte[] _programmVersionAddress;

        /// <summary>
        /// Адресс версии программы
        /// </summary>
        public byte[] DeviceNameAddress
        {
            get => _programmVersionAddress;
            set
            {
                _programmVersionAddress = value;
                if (_programmVersionSize != null)
                {
                    IsFullFirmWaveInformation(_programmVersionAddress, _programmVersionSize);
                }
            }
        }

        private async Task GetFirmware(byte[] address, byte[] size)
        {
            var command = new MessageCreator().CreateReadMessage(address, size);
            await QueryParamerter(command);
            await Task.Delay(Constants.ShortDelay);
        }

        public async Task Qualify()
        {
            if (DeviceNameAddress == null)
            {
                await QueryParamerter(_programmVersionAddressCommand);
                await Task.Delay(Constants.ShortDelay);
;           }
            if (DeviceNameSize == null)
            {
                await QueryParamerter(_programmVersionSizeCommand);
                await Task.Delay(Constants.ShortDelay);
            }
            if(DeviceNameAddress != null && DeviceNameSize != null)
            {
                await GetFirmware(DeviceNameAddress, DeviceNameSize);
            }
        }

        /// <summary>
        /// Размер версии программы
        /// </summary>
        private byte[] _programmVersionSize;

        public FirmWaveQualifier(Func<byte[], Task> queryParamerters,
            byte[] programmVersionAddressCommand, byte[] programmVersionSizeCommand)
        {
            QueryParamerter = queryParamerters;
            _programmVersionAddressCommand = programmVersionAddressCommand;
            _programmVersionSizeCommand = programmVersionSizeCommand;

            IsFullFirmWaveInformation += GetFirmware;
        }

        /// <summary>
        /// Размер версии программы
        /// </summary>
        public byte[] DeviceNameSize
        {
            get
            {
                return _programmVersionSize;
            }
            set
            {
                _programmVersionSize = value;
                if (_programmVersionAddress != null)
                {
                    IsFullFirmWaveInformation(_programmVersionAddress, _programmVersionSize);
                }
            }
        }

        // Делагат полноты данных
        public delegate Task FirmWaveHandler(byte[] deviceNameAddress, byte[] deviceNameSize);

        /// <summary>
        /// Событие готовности всех данных
        /// </summary>
        public event FirmWaveHandler IsFullFirmWaveInformation;
    }
}
