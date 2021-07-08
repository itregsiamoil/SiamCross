using SiamCross.Models.Connection.Protocol;
using SiamCross.Services;
using SiamCross.Services.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Umt
{
    class TaskStorageRead : BaseSensorTask
    {
        readonly Storage _Storage;

        readonly MemStruct _CurrMemInfo = new MemStruct(0x0E);
        readonly MemVarUInt16 Adrtek = new MemVarUInt16(0x0E);
        readonly MemVarUInt16 Ukstr = new MemVarUInt16(0x10);
        readonly MemVarUInt16 Ukbl = new MemVarUInt16(0x12);

        readonly MemStruct _MemInfo = new MemStruct(0x14);
        readonly MemVarUInt16 page = new MemVarUInt16(0x14);
        readonly MemVarUInt16 kolstr = new MemVarUInt16(0x16);
        readonly MemVarUInt16 kolbl = new MemVarUInt16(0x18);

        readonly MemStruct _CurrInfo = new MemStruct(0x841A);
        readonly MemVarUInt16 Emem = new MemVarUInt16();
        readonly MemVarUInt16 Kolisl = new MemVarUInt16();
        readonly MemVarUInt16 Schstr = new MemVarUInt16();
        readonly MemVarUInt16 Schbl = new MemVarUInt16();
        readonly MemVarUInt16 Koliz = new MemVarUInt16();

        readonly MemStruct _Rep = new MemStruct(0x00);
        readonly MemVarUInt8 IdRep = new MemVarUInt8();
        readonly MemVarUInt16 KSum = new MemVarUInt16();
        readonly MemVarUInt16 AdrPr = new MemVarUInt16();
        readonly MemVarUInt16 KolToch = new MemVarUInt16();
        readonly MemVarUInt16 KolStrt = new MemVarUInt16();
        readonly MemVarUInt16 KolBlt = new MemVarUInt16();
        readonly MemVarUInt16 NomIslt = new MemVarUInt16();
        readonly MemVarUInt8 KolPar = new MemVarUInt8();
        readonly MemVarUInt8 VisslRep = new MemVarUInt8();
        readonly MemVarByteArray KustRep = new MemVarByteArray(0, new MemValueByteArray(5));
        readonly MemVarByteArray SkvRep = new MemVarByteArray(0, new MemValueByteArray(6));
        readonly MemVarUInt16 FieldRep = new MemVarUInt16();
        readonly MemVarUInt16 ShopRep = new MemVarUInt16();
        readonly MemVarUInt16 OperatorRep = new MemVarUInt16();
        readonly MemVarByteArray StartTimestamp = new MemVarByteArray(0, new MemValueByteArray(6));
        readonly MemVarUInt32 IntervalRep = new MemVarUInt32();
        //readonly MemVarFloat DavRep = new MemVarFloat();
        //readonly MemVarFloat TempRep = new MemVarFloat();
        //readonly MemVarFloat ExTempRep = new MemVarFloat();

        const UInt32 FramOffset = 0x70000000;
        const UInt32 FlashOffset = 0x80000000;
        const int _MtSize = 4096;
        const int _MaxFileSize = 1024 * 1024 * 10;

        readonly MemVarByteArray DataMt;
        MeasureData _CurrSurvey = null;

        MemoryStream _DataPress;
        MemoryStream _DataTemp;
        MemoryStream _DataTempExt;

        float MinPress;
        float MaxPress;
        float MinIntTemp;
        float MaxIntTemp;
        float MinExtTemp;
        float MaxExtTemp;

        ulong _BytesTotal;
        ulong _BytesReaded;


        UInt16 PageSize => page.Value;
        UInt16 PageQty => kolstr.Value;
        UInt16 BlockQty => kolbl.Value;

        UInt16 CurrentPage
        {
            get => Ukstr.Value;
            set => Ukstr.Value = value;
        }
        UInt16 CurrentBlock
        {
            get => Ukbl.Value;
            set => Ukbl.Value = value;
        }

        public TaskStorageRead(Storage model, SensorModel sensor)
            : base(sensor, Resource.ReadingSurvey)
        {
            if (model is Storage storage)
                _Storage = storage;

            _CurrMemInfo.Add(Adrtek);
            _CurrMemInfo.Add(Ukstr);
            _CurrMemInfo.Add(Ukbl);

            _MemInfo.Add(page);
            _MemInfo.Add(kolstr);
            _MemInfo.Add(kolbl);

            _CurrInfo.Add(Emem);
            _CurrInfo.Add(Kolisl);
            _CurrInfo.Add(Schstr);
            _CurrInfo.Add(Schbl);
            _CurrInfo.Add(Koliz);

            _Rep.Add(IdRep);
            _Rep.Add(KSum);
            _Rep.Add(AdrPr);
            _Rep.Add(KolToch);
            _Rep.Add(KolStrt);
            _Rep.Add(KolBlt);
            _Rep.Add(NomIslt);
            _Rep.Add(KolPar);
            _Rep.Add(VisslRep);
            _Rep.Add(KustRep);
            _Rep.Add(SkvRep);
            _Rep.Add(FieldRep);
            _Rep.Add(ShopRep);
            _Rep.Add(OperatorRep);
            _Rep.Add(StartTimestamp);
            _Rep.Add(IntervalRep);

            DataMt = new MemVarByteArray(0, new MemValueByteArray(_MtSize));
        }
        public override async Task<bool> DoExecuteAsync(CancellationToken ct)
        {
            if (null == _Storage || null == Connection)
                return false;
            if (!await CheckConnectionAsync(ct))
                return false;
            InfoEx = Resource.ReadingMemoryInformation;
            await Connection.ReadAsync(_MemInfo, null, ct);
            InfoEx = Resource.ReadingMemoryStatus;
            await Connection.ReadAsync(_CurrMemInfo, null, ct);
            InfoEx = Resource.ReadingNumberSurvey;
            await Connection.ReadAsync(_CurrInfo, null, ct);


            if (0xFFFF == Adrtek.Value || 0 == Kolisl.Value)
            {
                InfoEx = Resource.Empty;
                return true;
            }

            var totalSpace = (ulong)(kolbl.Value) * kolstr.Value * page.Value;
            var emptySpaceRatio = Math.Round(0.1f * Emem.Value, 1);

            _BytesTotal = (uint)(totalSpace * ((double)1.0f - emptySpaceRatio / 100.0f));
            _BytesReaded = 0;


            UInt16 currAddrInPage = Adrtek.Value;
            /*
            var sc = Connection as Connection.Protocol.Siam.SiamConnection;
            sc.Address = 127;
            MemVarUInt16 devBtRequest = new MemVarUInt16(0x5E);
            MemVarUInt16 devUartResponse = new MemVarUInt16(0x60);
            var devTest = new MemStruct(0x5E);
            devTest.Add(devBtRequest);
            devTest.Add(devUartResponse);
            await Connection.ReadAsync(devTest);
            sc.PrintExchangeCounters();
            System.Diagnostics.Debug.WriteLine($"dev: BtRequest={devBtRequest.Value} UartResponse={devUartResponse.Value}");
            sc.Address = 1;
            */
            NomIslt.Value = Kolisl.Value;
            UInt16 CurrentSurveyId = 0xFFFF;
            HashSet<UInt16> ReadedSurveys = new HashSet<UInt16>();
            ReadedSurveys.Add(CurrentSurveyId);

            if (IsFram(currAddrInPage))
            {
                currAddrInPage = (UInt16)(currAddrInPage & 0x7FFF);

                while (0x7FFF != currAddrInPage)
                {
                    _Rep.Address = FramOffset + currAddrInPage;
                    await Connection.ReadAsync(_Rep, SetProgressBytes, ct);

                    if (CurrentSurveyId != NomIslt.Value)
                    {
                        await CloseSurvey(ct);
                        if (UInt16.MaxValue == NomIslt.Value || !ReadedSurveys.Add(NomIslt.Value))
                            break;
                        CurrentSurveyId = NomIslt.Value;
                        OpenSurvey();
                    }
                    await AppendSurveyData(ct);

                    if (0 == currAddrInPage)
                    {
                        currAddrInPage = AdrPr.Value;
                        break;
                    }
                    currAddrInPage = AdrPr.Value;
                }
            }
            // read from flash
            while (0x7FFF != currAddrInPage)
            {
                _Rep.Address = FlashOffset + GetCurrentPageAddress() + currAddrInPage;
                await Connection.ReadAsync(_Rep, SetProgressBytes, ct);

                if (CurrentSurveyId != NomIslt.Value)
                {
                    await CloseSurvey(ct);
                    if (UInt16.MaxValue == NomIslt.Value || !ReadedSurveys.Add(NomIslt.Value))
                        break;
                    CurrentSurveyId = NomIslt.Value;
                    OpenSurvey();
                }

                uint qty = (uint)4 * KolPar.Value * (uint)(((0 == KolToch.Value) ? 1 : KolToch.Value));
                if (_DataPress.Position < qty)
                {
                    await CloseSurvey(ct);
                    OpenSurvey();
                }
                await AppendSurveyData(ct);

                if (0 == currAddrInPage)
                    DecrementPage();
                currAddrInPage = AdrPr.Value;
            }

            await CloseSurvey(ct);
            /*
            sc.Address = 127;
            await Connection.ReadAsync(devTest);
            sc.PrintExchangeCounters();
            System.Diagnostics.Debug.WriteLine($"dev: BtRequest={devBtRequest.Value} UartResponse={devUartResponse.Value}");
            sc.Address = 1;
            */

            return true;
        }

        async Task CloseSurvey(CancellationToken ct)
        {
            if (null == _CurrSurvey)
                return;
            try
            {
                _CurrSurvey.Measure.Set("MinPressure", (double)MinPress);
                _CurrSurvey.Measure.Set("MaxPressure", (double)MaxPress);
                _CurrSurvey.Measure.Set("MinIntTemperature", (double)MinIntTemp);
                _CurrSurvey.Measure.Set("MaxIntTemperature", (double)MaxIntTemp);
                _CurrSurvey.Measure.Set("MinExtTemperature", (double)MinExtTemp);
                _CurrSurvey.Measure.Set("MaxExtTemperature", (double)MaxExtTemp);

                using (var file = EnvironmentService.CreateTempFileSurvey())
                {
                    await _DataPress.CopyToAsync(file, _MtSize, ct);
                    _CurrSurvey.Measure.SetBlob("mtpressure", Path.GetFileName(file.Name));
                    file.Close();
                }
                if (null != _DataTemp)
                    using (var file = EnvironmentService.CreateTempFileSurvey())
                    {
                        await _DataTemp.CopyToAsync(file, _MtSize, ct);
                        _CurrSurvey.Measure.SetBlob("mttemperature", Path.GetFileName(file.Name));
                        file.Close();
                    }
                if (null != _DataTempExt)
                    using (var file = EnvironmentService.CreateTempFileSurvey())
                    {
                        await _DataTempExt.CopyToAsync(file, _MtSize, ct);
                        _CurrSurvey.Measure.SetBlob("umttemperatureex", Path.GetFileName(file.Name));
                        file.Close();
                    }
                await DbService.Instance.SaveSurveyAsync(_CurrSurvey);
            }
            finally
            {
                _DataPress.Close();
                _DataTemp?.Close();
                _DataTempExt?.Close();
                _DataPress.Dispose();
                _DataTemp?.Dispose();
                _DataTempExt?.Dispose();
                _DataPress = null;
                _DataTemp = null;
                _DataTempExt = null;
            }
        }
        async Task AppendSurveyData(CancellationToken ct)
        {
            if (null == _CurrSurvey)
                return;
            DataMt.Address = _Rep.Address + _Rep.Size;
            uint qty = (uint)4 * KolPar.Value * (uint)(((0 == KolToch.Value) ? 1 : KolToch.Value));
            await Connection.ReadMemAsync(DataMt.Address, (uint)qty, DataMt.Value, 0, SetProgressBytes, ct);

            _DataPress.Seek(-qty / KolPar.Value, SeekOrigin.Current);
            _DataTemp?.Seek(-qty / KolPar.Value, SeekOrigin.Current);
            _DataTempExt?.Seek(-qty / KolPar.Value, SeekOrigin.Current);

            int curr = 0;
            while (curr < qty)
            {
                switch (KolPar.Value)
                {
                    default: break;
                    case 3:
                        await _DataTempExt?.WriteAsync(DataMt.Value, curr + 2 * 4, 4, ct);
                        UpdateMinMax(BitConverter.ToSingle(DataMt.Value, curr + 2 * 4), ref MinExtTemp, ref MaxExtTemp);
                        goto case 2;
                    case 2:
                        await _DataTemp?.WriteAsync(DataMt.Value, curr + 1 * 4, 4, ct);
                        UpdateMinMax(BitConverter.ToSingle(DataMt.Value, curr + 1 * 4), ref MinIntTemp, ref MaxIntTemp);
                        goto case 1;
                    case 1:
                        await _DataPress.WriteAsync(DataMt.Value, curr + 0 * 4, 4, ct);
                        UpdateMinMax(BitConverter.ToSingle(DataMt.Value, curr + 0 * 4), ref MinPress, ref MaxPress);
                        break;
                }
                curr += (KolPar.Value) * 4;
            }
            _DataPress.Seek(-qty / KolPar.Value, SeekOrigin.Current);
            _DataTemp?.Seek(-qty / KolPar.Value, SeekOrigin.Current);
            _DataTempExt?.Seek(-qty / KolPar.Value, SeekOrigin.Current);

            long interval = IntervalRep.Value / 10000;
            long count = (_MaxFileSize - _DataPress.Position) / 4;
            _CurrSurvey.Measure.Set("MeasurementsCount", (long)count);

            _CurrSurvey.Measure.BeginTimestamp = GetTimestamp(StartTimestamp.Value);
            var ts = TimeSpan.FromSeconds(interval * (count - 1));
            _CurrSurvey.Measure.EndTimestamp = _CurrSurvey.Measure.BeginTimestamp + ts;

        }
        void OpenSurvey()
        {

            MeasureData survey = new MeasureData(
                new Position()
                , Sensor.Device
                , Sensor.Info
                , new MeasurementInfo());

            InfoEx = $"{Resource.Reading} {NomIslt.Value} {Resource.DopSurvey}";

            MinPress = float.MaxValue;
            MaxPress = float.MinValue;
            MinIntTemp = float.MaxValue;
            MaxIntTemp = float.MinValue;
            MinExtTemp = float.MaxValue;
            MaxExtTemp = float.MinValue;

            survey.Measure.Kind = 2;
            survey.Position.Field = FieldRep.Value;
            survey.Position.Well = Encoding.UTF8.GetString(SkvRep.Value);
            survey.Position.Bush = Encoding.UTF8.GetString(KustRep.Value);
            survey.Position.Shop = ShopRep.Value;
            //survey.Measure.BeginTimestamp = GetTimestamp(StartTimestamp.Value);
            //survey.Measure.EndTimestamp = GetTimestamp(StartTimestamp.Value)
            //    + TimeSpan.FromSeconds((KolToch.Value-1) * IntervalRep.Value/10000);

            survey.Measure.Set("PeriodSec", (long)IntervalRep.Value / 10000);
            survey.Measure.Set("umttype", (long)VisslRep.Value);
            survey.Measure.Set("mtinterval", TimeSpan.FromSeconds(IntervalRep.Value / 10000).ToString());
            survey.Measure.Set("MeasurementsCount", (long)0);
            switch (KolPar.Value)
            {
                default: break;
                case 3:
                    _DataTempExt = new MemoryStream(_MaxFileSize);
                    _DataTempExt.Seek(_MaxFileSize, SeekOrigin.Begin);
                    goto case 2;
                case 2:
                    _DataTemp = new MemoryStream(_MaxFileSize);
                    _DataTemp.Seek(_MaxFileSize, SeekOrigin.Begin);
                    goto case 1;
                case 1:
                    _DataPress = new MemoryStream(_MaxFileSize);
                    _DataPress.Seek(_MaxFileSize, SeekOrigin.Begin);
                    break;
            }
            _CurrSurvey = survey;
        }
        DateTime GetTimestamp(byte[] data)
        {
            try
            {
                var dt = DateTime.Now;
                var epoh = dt.Year - dt.Year % 100;
                var year = (100 > data[5]) ? epoh + data[5] : data[5];

                return new DateTime(year, data[4], data[3], data[0], data[1], data[2]);
            }
            catch (Exception)
            {

            }
            return DateTime.Now;
        }
        uint GetCurrentPageAddress()
        {
            return PageSize * (CurrentPage + (uint)CurrentBlock * PageQty);
        }
        void DecrementPage()
        {
            if (0 < CurrentPage)
                CurrentPage--;
            else
            {
                if (0 == CurrentBlock)
                {
                    CurrentBlock = (UInt16)(BlockQty - 1);
                }
                else
                {
                    CurrentBlock--;
                }
                CurrentPage = (UInt16)(PageQty - 1);
            }
        }

        bool IsFram(UInt16 pageAddr)
        {
            return !IsFlash(pageAddr);
        }
        bool IsFlash(UInt16 pageAddr)
        {
            return 0 < (pageAddr & (1 << 15));
        }
        void SetProgressBytes(uint bytes)
        {
            _BytesReaded += bytes;
            Progress = ((float)_BytesReaded / _BytesTotal);
        }
        static void UpdateMinMax<T>(T val, ref T min, ref T max) where T : IComparable
        {
            if (0 > val.CompareTo(min))
                min = val;
            if (0 < val.CompareTo(max))
                max = val;
        }
    }
}
