using SiamCross.Models.Connection.Protocol;
using SiamCross.Models.Scanners;
using SiamCross.Models.Sensors.Dua.Surveys;
using SiamCross.ViewModels.Dua;
using SiamCross.ViewModels.Dua.Survey;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiamCross.Models.Sensors.Dua
{
    public class DuaSensor : BaseSensor2
    {
        public readonly MemStruct _SurvayParam;//0x8000
        public readonly MemVarUInt16 Chdav;
        public readonly MemVarUInt16 Chpiezo;
        public readonly MemVarUInt16 Noldav;
        public readonly MemVarUInt16 Nolexo;
        public readonly MemVarUInt16 Revbit;//du end
        public readonly MemVarUInt8 Vissl;
        public readonly MemVarByteArray Kust;
        public readonly MemVarByteArray Skv;
        public readonly MemVarUInt16 Field;
        public readonly MemVarUInt16 Shop;
        public readonly MemVarUInt16 Operator;
        public readonly MemVarUInt16 Vzvuk;
        public readonly MemVarUInt16 Ntpop;
        public readonly MemVarUInt8 Perp;
        public readonly MemVarUInt8 Kolp;
        public readonly MemVarUInt8 Peru;
        public readonly MemVarUInt8 kolur;
        public readonly MemVarUInt16 Rterm;
        public readonly MemVarUInt16 Pterm;

        public readonly MemStruct _CurrentParam;//0x8400
        public readonly MemVarUInt16 BatteryVoltage;
        public readonly MemVarUInt16 ТempC;
        public readonly MemVarInt16 Pressure;
        public readonly MemVarByteArray Time;
        public readonly MemVarUInt16 Emak;
        public readonly MemVarUInt16 Rdav;
        public readonly MemVarByteArray TimeRequre;
        public readonly MemVarUInt8 Interv;
        public readonly MemVarUInt16 Kolt;
        public readonly MemVarUInt16 Timeawt;
        public readonly MemVarUInt16 Uksh;
        public readonly MemVarUInt16 Ukex;


        public readonly MemStruct _Operating;//0x8800
        public readonly MemVarUInt8 OpReg;
        public readonly MemVarUInt16 StatusReg;

        public readonly MemStruct _Report;//0x80000000 
        public readonly MemVarUInt16 Urov;
        public readonly MemVarUInt16 Otr;

        public readonly MemStruct _ReportArray;//0x82000000  




        public DuaSensor(IProtocolConnection conn, ScannedDeviceInfo dev_info)
            : base(conn, dev_info)
        {
            Connection.AdditioonalTimeout = 2000;

            _SurvayParam = new MemStruct(0x8000);
            Chdav = _SurvayParam.Add(new MemVarUInt16(nameof(Chdav)));
            Chpiezo = _SurvayParam.Add(new MemVarUInt16(nameof(Chpiezo)));
            Noldav = _SurvayParam.Add(new MemVarUInt16(nameof(Noldav)));
            Nolexo = _SurvayParam.Add(new MemVarUInt16(nameof(Nolexo)));
            Revbit = _SurvayParam.Add(new MemVarUInt16(nameof(Revbit)));


            _CurrentParam = new MemStruct(0x8400);
            BatteryVoltage = _CurrentParam.Add(new MemVarUInt16(nameof(BatteryVoltage)));
            ТempC = _CurrentParam.Add(new MemVarUInt16(nameof(ТempC)));
            Pressure = _CurrentParam.Add(new MemVarInt16(nameof(Pressure)));

            _Operating = new MemStruct(0x8800);
            OpReg = _Operating.Add(new MemVarUInt8(nameof(OpReg)));
            StatusReg = _Operating.Add(new MemVarUInt16(nameof(StatusReg)));

            _Report = new MemStruct(0x80000000);
            Urov = _Report.Add(new MemVarUInt16(nameof(Urov)));
            Otr = _Report.Add(new MemVarUInt16(nameof(Otr)));

            var levelModel = new Level(this, 1
                , "Статический уровень"
                , "long long description");
            Model.Surveys.Add(levelModel);
            var levelVM = new LevelVM(this, levelModel);
            SurveysVM.SurveysCollection.Add(levelVM);

            var dynamicModel = new Level(this, 2
                , "Динамический уровень"
                , "long long description");
            Model.Surveys.Add(dynamicModel);

            var dynamicLevelVM = new LevelVM(this, dynamicModel);
            SurveysVM.SurveysCollection.Add(dynamicLevelVM);



            /*
            Model.Surveys.Add(new SurveyVM(this, null
                , "Динамический уровень"
                , "long long description"));

            Model.Surveys.Add(new SurveyVM(this, null
                , "КВУ"
                , "кривая восстановления уровня"));

            Model.Surveys.Add(new SurveyVM(this, null
                , "КВД"
                , "кривая восстановления давления"));

            Model.Surveys.Add(new SurveyVM(this, null
                , "АРД"
                , "автоматическая регистрация давления"));
            */

            Model.Storage = new DuaStorage(this);
            StorageVM = new DuaStorageVM(this);


            //FactoryConfigVM = new FactoryConfigVM(this);
            //UserConfigVM = new UserConfigVM(this);
            //StateVM = new StateVM(this);



        }



        public override async Task<bool> QuickReport(CancellationToken cancelToken)
        {
            try
            {
                cancelToken.ThrowIfCancellationRequested();

                RespResult ret = await Connection.ReadAsync(_CurrentParam);

                Battery = (BatteryVoltage.Value / 10.0).ToString();
                Temperature = (ТempC.Value / 10.0).ToString();
                //_reportBuilder.Load = LoadChanel.Value.ToString();
                //_reportBuilder.Acceleration = AccelerationChanel.Value.ToString();
                //SensorData.Status = _reportBuilder.GetReport();

                Status = $"{Resource.Pressure}: "
                    + Pressure.Value / 10.0 + $" ({Resource.KGFCMUnits})";

                ChangeNotify(nameof(Battery));
                ChangeNotify(nameof(Temperature));
                ChangeNotify(nameof(Status));
                ScannedDeviceInfo.Device.DeviceData["Battery"] = Battery;
                ScannedDeviceInfo.Device.DeviceData["Temperature"] = Temperature;
                ScannedDeviceInfo.Device.DeviceData["Status"] = Status;
                return true;
            }
            catch (ProtocolException)
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in: "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return false;
        }


    }//DuaSensor
}
