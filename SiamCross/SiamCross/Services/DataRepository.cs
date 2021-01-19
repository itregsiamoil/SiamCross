using Autofac;
using Dapper;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Services.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SiamCross.Services
{
    public class DataRepository
    {
        private static readonly Lazy<DataRepository> _instance =
            new Lazy<DataRepository>(() => new DataRepository());
        public static DataRepository Instance => _instance.Value;
        private readonly IDbConnection _database;
        private static readonly object _locker = new object();
        private readonly string _databasePart;

        private static readonly Logger _logger =
            AppContainer.Container.Resolve<ILogManager>().GetLog();

        private DataRepository()
        {
            try
            {
                _databasePart = AppContainer.Container.Resolve<ISQLite>()
                    .GetDatabasePath("Db.sqlite");

                if (!File.Exists(_databasePart))
                {
                    AppContainer.Container.Resolve<IDatabaseCreator>().CreateDatabase(_databasePart);
                }

                _database = AppContainer.Container
                    .Resolve<IDbConnection>(new TypedParameter(typeof(string),
                        (string.Format("Data Source={0};Version=3;", _databasePart))));
                _database.Open();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Database creation error! Database path: {_databasePart}" + "\n");
                throw;
            }

            try
            {
                CreateDdin2Table();
                CreateDuTable();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Table creation error!" + "\n");
                throw;
            }
        }

        private void NonQueryCheck()
        {
            // Ensure we have a connection
            if (_database == null)
            {
                _logger.Error($"NonQueryCheck database error!" + "\n");
                throw new NullReferenceException("Please provide a connection" + "\n");
            }

            // Ensure that the connection state is Open
            if (_database.State != ConnectionState.Open)
            {
                _database.Open();
            }
        }

        #region Table Creation

        private void CreateDdin2Table()
        {
            NonQueryCheck();

            _database.Execute(@"
            CREATE TABLE IF NOT EXISTS [Ddin2Measurement] (
                [Id] INTEGER NOT NULL PRIMARY KEY,
                [MaxWeight] REAL NOT NULL,
                [MinWeight] REAL NOT NULL,
                [Travel] INTEGER NOT NULL,
                [Period] INTEGER NOT NULL,
                [Step] INTEGER NOT NULL,
                [WeightDiscr] INTEGER NOT NULL,
                [TimeDiscr] INTEGER NOT NULL,
                [DynGraph] BLOB,
                [AccelerationGraph] BLOB,
                [Field] NVARCHAR(128) NOT NULL,
                [Well] NVARCHAR(128) NOT NULL,
                [Bush] NVARCHAR(128) NOT NULL,
                [Shop] NVARCHAR(128) NOT NULL,
                [BufferPressure] NVARCHAR(128) NOT NULL,
                [Comment] NVARCHAR(128) NOT NULL,
                [Name] NVARCHAR(128) NOT NULL,
                [DateTime] TEXT NOT NULL,
                [ErrorCode] NVARCHAR(128),
                [ApertNumber] REAL NOT NULL,
                [ModelPump] REAL NOT NULL,
                [Rod] REAL NOT NULL,
                [MaxBarbellWeight] REAL NOT NULL,
                [MinBarbellWeight] REAL NOT NULL,
                [TravelLength] REAL NOT NULL,
                [SwingCount] REAL NOT NULL,
                [BatteryVolt] NVARCHAR(128) NOT NULL,
                [Temperature] NVARCHAR(128) NOT NULL,
                [MainFirmware] NVARCHAR(128) NOT NULL,
                [RadioFirmware] NVARCHAR(128) NOT NULL)");
        }

        private void CreateDuTable()
        {
            NonQueryCheck();
            //_database.Execute("DROP TABLE IF EXISTS [DuMeasurement] ");
            _database.Execute(@"
                CREATE TABLE IF NOT EXISTS [DuMeasurement] (
                [Id] INTEGER NOT NULL PRIMARY KEY,
                [SrcFluidLevel] INTEGER NOT NULL,
                [SrcReflectionsCount] INTEGER NOT NULL,
                [AnnularPressure] REAL NOT NULL,
                [Echogram] BLOB,
                [SoundSpeed] NVARCHAR(128) NOT NULL,
                [MeasurementType] NVARCHAR(128) NOT NULL,
                [SoundSpeedCorrection] NVARCHAR(128) NOT NULL,
                [ReportTimestamp] TEXT NOT NULL,
                [Field] NVARCHAR(128) NOT NULL,
                [Well] NVARCHAR(128) NOT NULL,
                [Bush] NVARCHAR(128) NOT NULL,
                [Shop] NVARCHAR(128) NOT NULL,
                [BufferPressure] NVARCHAR(128) NOT NULL,
                [Comment] NVARCHAR(128) NOT NULL,
                [Name] NVARCHAR(128) NOT NULL,
                [BatteryVolt] NVARCHAR(128) NOT NULL,
                [Temperature] NVARCHAR(128) NOT NULL,
                [MainFirmware] NVARCHAR(128) NOT NULL,
                [RadioFirmware] NVARCHAR(128) NOT NULL)");
        }

        #endregion 

        public int SaveDuMeasurement(DuMeasurement duMeasurement)
        {
            NonQueryCheck();
            try
            {
                if (duMeasurement.Id == 0)
                {
                    IEnumerable<int> idsColumn = _database.Query<int>(string.Format(
                        "SELECT Id FROM DuMeasurement"));
                    if (idsColumn.Count() > 0)
                    {
                        duMeasurement.Id = idsColumn.Max() + 1;
                    }
                    else
                    {
                        duMeasurement.Id = 1;
                    }
                }

                IEnumerable<dynamic> rows = _database.Query(string.Format(
                    "SELECT COUNT(1) AS 'Count' FROM DuMeasurement WHERE Id = '{0}'", duMeasurement.Id));

                if (rows.First().Count > 0)
                {
                    UpdateDuMeasurement(duMeasurement);
                    return duMeasurement.Id;
                }

                string sql = "INSERT INTO DuMeasurement (" +
                        "  SrcFluidLevel, SrcReflectionsCount, AnnularPressure" +
                        ", Echogram, SoundSpeed, MeasurementType" +
                        ", SoundSpeedCorrection, ReportTimestamp, Field" +
                        ", Well, Bush, Shop" +
                        ", BufferPressure, Comment, Name" +
                        ", BatteryVolt, Temperature, MainFirmware, RadioFirmware" +
                        ") Values (" +
                        "  @SrcFluidLevel, @SrcReflectionsCount, @AnnularPressure" +
                        ", @Echogram, @SoundSpeed, @MeasurementType" +
                        ", @SoundSpeedCorrection, @ReportTimestamp,  @Field" +
                        ", @Well, @Bush, @Shop" +
                        ", @BufferPressure, @Comment, @Name " +
                        ", @BatteryVolt, @Temperature, @MainFirmware, @RadioFirmware  );";
                int affectedRows = _database.Execute(sql, new
                {
                    duMeasurement.SrcFluidLevel,
                    duMeasurement.SrcReflectionsCount,
                    duMeasurement.AnnularPressure,
                    duMeasurement.Echogram,
                    duMeasurement.SoundSpeed,
                    duMeasurement.MeasurementType,
                    duMeasurement.SoundSpeedCorrection,
                    duMeasurement.ReportTimestamp,
                    duMeasurement.Field,
                    duMeasurement.Well,
                    duMeasurement.Bush,
                    duMeasurement.Shop,
                    duMeasurement.BufferPressure,
                    duMeasurement.Comment,
                    duMeasurement.Name,
                    duMeasurement.BatteryVolt,
                    duMeasurement.Temperature,
                    duMeasurement.MainFirmware,
                    duMeasurement.RadioFirmware
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SaveDuMeasurement "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                throw;
            }
            return duMeasurement.Id;
        }
        public void UpdateDuMeasurement(DuMeasurement duMeasurement)
        {
            try
            {
                string sql = "UPDATE DuMeasurement SET " +
                    "SrcFluidLevel = @SrcFluidLevel, SrcReflectionsCount = @SrcReflectionsCount, " +
                    "AnnularPressure = @AnnularPressure, Echogram = @Echogram, SoundSpeed = @SoundSpeed, " +
                    "MeasurementType = @MeasurementType, SoundSpeedCorrection = @SoundSpeedCorrection, " +
                    "ReportTimestamp = @ReportTimestamp," +
                    " Field = @Field, Well = @Well, Bush = @Bush, Shop = @Shop, BufferPressure = @BufferPressure," +
                    " Comment = @Comment, Name = @Name " +
                    " ,BatteryVolt=@BatteryVolt, Temperature=@Temperature " +
                    " ,MainFirmware=@MainFirmware, RadioFirmware=@RadioFirmware " +
                    " WHERE Id = @Id;";
                int affectedRows = _database.Execute(sql, new
                {
                    duMeasurement.SrcFluidLevel,
                    duMeasurement.SrcReflectionsCount,
                    duMeasurement.AnnularPressure,
                    duMeasurement.Echogram,
                    duMeasurement.SoundSpeed,
                    duMeasurement.MeasurementType,
                    duMeasurement.SoundSpeedCorrection,
                    duMeasurement.ReportTimestamp,
                    duMeasurement.Field,
                    duMeasurement.Well,
                    duMeasurement.Bush,
                    duMeasurement.Shop,
                    duMeasurement.BufferPressure,
                    duMeasurement.Comment,
                    duMeasurement.Name,
                    duMeasurement.BatteryVolt,
                    duMeasurement.Temperature,
                    duMeasurement.MainFirmware,
                    duMeasurement.RadioFirmware,
                    duMeasurement.Id
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UpdateDuMeasurement" + "\n");
                throw;
            }
        }
        public void RemoveDuMeasurement(int removebleId)
        {
            NonQueryCheck();
            try
            {
                _database.Execute("DELETE FROM DuMeasurement WHERE Id =" + removebleId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RemoveDuMeasurement" + "\n");
                throw;
            }
        }
        public IEnumerable<DuMeasurement> GetDuMeasurements()
        {
            NonQueryCheck();
            try
            {
                IEnumerable<DuMeasurement> duItems = _database.Query<DuMeasurement>(
                    "SELECT * FROM DuMeasurement");
                return duItems;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetDuMeasurements " + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                throw;
            }
        }
        public DuMeasurement GetDuMeasurementById(int id)
        {
            NonQueryCheck();
            try
            {
                return _database.Query<DuMeasurement>(
                    "SELECT * FROM DuMeasurement WHERE Id =" + id).First();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetDuMeasurementById "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                throw;
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public int SaveDdin2Measurement(Ddin2Measurement ddin2Measurement)
        {
            NonQueryCheck();

            try
            {
                if (ddin2Measurement.Id == 0)
                {
                    IEnumerable<int> idsColumn = _database.Query<int>(string.Format(
                        "SELECT Id FROM Ddin2Measurement"));
                    if (idsColumn.Count() > 0)
                    {
                        ddin2Measurement.Id = idsColumn.Max() + 1;
                    }
                    else
                    {
                        ddin2Measurement.Id = 1;
                    }
                }

                IEnumerable<dynamic> rows = _database.Query(string.Format(
                    "SELECT COUNT(1) as 'Count' FROM Ddin2Measurement WHERE Id = '{0}'",
                    ddin2Measurement.Id));

                if (rows.First().Count > 0)
                {
                    UpdateDdin2Measurement(ddin2Measurement);
                    return ddin2Measurement.Id;
                }

                string sql = "INSERT INTO Ddin2Measurement" +
                        "(MaxWeight, MinWeight, Travel, Period, Step, WeightDiscr, TimeDiscr, DynGraph," +
                        " AccelerationGraph, Field, Well, Bush, Shop, BufferPressure, Comment, " +
                        "Name, DateTime, ErrorCode, ApertNumber, ModelPump, Rod, MaxBarbellWeight," +
                        " MinBarbellWeight, TravelLength, SwingCount" +
                        ", BatteryVolt, Temperature, MainFirmware, RadioFirmware " +
                        ") Values (" +
                        "@MaxWeight, @MinWeight," +
                        " @Travel, @Period, @Step, @WeightDiscr, @TimeDiscr, @DynGraph," +
                        " @AccelerationGraph, @Field, @Well, @Bush, @Shop, @BufferPressure, @Comment, " +
                        "@Name, @DateTime, @ErrorCode, @ApertNumber, @ModelPump, @Rod, @MaxBarbellWeight," +
                        " @MinBarbellWeight, @TravelLength, @SwingCount " +
                        ", @BatteryVolt, @Temperature, @MainFirmware, @RadioFirmware  );";

                int affectedRows = _database.Execute(sql, new
                {
                    MaxWeight = ddin2Measurement.MaxWeight,
                    MinWeight = ddin2Measurement.MinWeight,
                    Travel = ddin2Measurement.Travel,
                    Period = ddin2Measurement.Period,
                    Step = ddin2Measurement.Step,
                    WeightDiscr = ddin2Measurement.WeightDiscr,
                    TimeDiscr = ddin2Measurement.TimeDiscr,
                    DynGraph = ddin2Measurement.DynGraph,
                    AccelerationGraph = ddin2Measurement.AccelerationGraph,
                    Field = ddin2Measurement.Field,
                    Well = ddin2Measurement.Well,
                    Bush = ddin2Measurement.Bush,
                    Shop = ddin2Measurement.Shop,
                    BufferPressure = ddin2Measurement.BufferPressure,
                    Comment = ddin2Measurement.Comment,
                    Name = ddin2Measurement.Name,
                    DateTime = ddin2Measurement.DateTime,
                    ErrorCode = ddin2Measurement.ErrorCode,
                    ApertNumber = ddin2Measurement.ApertNumber,
                    ModelPump = ddin2Measurement.ModelPump,
                    Rod = ddin2Measurement.Rod,
                    MaxBarbellWeight = ddin2Measurement.MaxBarbellWeight,
                    MinBarbellWeight = ddin2Measurement.MinBarbellWeight,
                    TravelLength = ddin2Measurement.TravelLength,
                    SwingCount = ddin2Measurement.SwingCount,
                    BatteryVolt = ddin2Measurement.BatteryVolt,
                    Temperature = ddin2Measurement.Temperature,
                    MainFirmware = ddin2Measurement.MainFirmware,
                    RadioFirmware = ddin2Measurement.RadioFirmware
                });
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ddin2Save database error!" + "\n");
                throw;
            }

            return ddin2Measurement.Id;
        }
        private void UpdateDdin2Measurement(Ddin2Measurement ddin2Measurement)
        {
            try
            {
                string sql = "UPDATE Ddin2Measurement SET " +
                     "MaxWeight = @MaxWeight, MinWeight = @MinWeight, Travel = @Travel," +
                     " Period = @Period, Step = @Step, WeightDiscr = @WeightDiscr," +
                     " TimeDiscr = @TimeDiscr, DynGraph = @DynGraph," +
                     " AccelerationGraph = @AccelerationGraph, Field = @Field," +
                     " Well = @Well, Bush = @Bush, Shop = @Shop, BufferPressure = @BufferPressure," +
                     " Comment = @Comment, Name = @Name, DateTime = @DateTime," +
                     " ErrorCode = @ErrorCode, ApertNumber = @ApertNumber," +
                     " ModelPump = @ModelPump, Rod = @Rod, MaxBarbellWeight = @MaxBarbellWeight," +
                     " MinBarbellWeight = @MinBarbellWeight, TravelLength = @TravelLength, SwingCount = @SwingCount" +
                     " ,BatteryVolt=@BatteryVolt, Temperature=@Temperature " +
                     " ,MainFirmware=@MainFirmware, RadioFirmware=@RadioFirmware " +
                     " WHERE Id = @Id;";

                int affectedRows = _database.Execute(sql, new
                {
                    MaxWeight = ddin2Measurement.MaxWeight,
                    MinWeight = ddin2Measurement.MinWeight,
                    Travel = ddin2Measurement.Travel,
                    Period = ddin2Measurement.Period,
                    Step = ddin2Measurement.Step,
                    WeightDiscr = ddin2Measurement.WeightDiscr,
                    TimeDiscr = ddin2Measurement.TimeDiscr,
                    DynGraph = ddin2Measurement.DynGraph,
                    AccelerationGraph = ddin2Measurement.AccelerationGraph,
                    Field = ddin2Measurement.Field,
                    Well = ddin2Measurement.Well,
                    Bush = ddin2Measurement.Bush,
                    Shop = ddin2Measurement.Shop,
                    BufferPressure = ddin2Measurement.BufferPressure,
                    Comment = ddin2Measurement.Comment,
                    Name = ddin2Measurement.Name,
                    DateTime = ddin2Measurement.DateTime,
                    ErrorCode = ddin2Measurement.ErrorCode,
                    ApertNumber = ddin2Measurement.ApertNumber,
                    ModelPump = ddin2Measurement.ModelPump,
                    Rod = ddin2Measurement.Rod,
                    MaxBarbellWeight = ddin2Measurement.MaxBarbellWeight,
                    MinBarbellWeight = ddin2Measurement.MinBarbellWeight,
                    TravelLength = ddin2Measurement.TravelLength,
                    SwingCount = ddin2Measurement.SwingCount,
                    BatteryVolt = ddin2Measurement.BatteryVolt,
                    Temperature = ddin2Measurement.Temperature,
                    MainFirmware = ddin2Measurement.MainFirmware,
                    RadioFirmware = ddin2Measurement.RadioFirmware,
                    Id = ddin2Measurement.Id
                });
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ddin2Update database error!" + "\n");
                throw;
            }
        }
        public void RemoveDdin2Measurement(int removebleId)
        {
            NonQueryCheck();

            try
            {
                _database.Execute("DELETE FROM Ddin2Measurement WHERE Id =" + removebleId);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ddin2Remove database error!" + "\n");
                throw;
            }
        }
        public IEnumerable<Ddin2Measurement> GetDdin2Measurements()
        {
            NonQueryCheck();

            try
            {
                return _database.Query<Ddin2Measurement>(
                    "SELECT * FROM Ddin2Measurement");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ddin2Get database error!" + "\n");
                throw;
            }
        }
        public Ddin2Measurement GetDdin2MeasurementById(int id)
        {
            NonQueryCheck();
            try
            {
                return _database.Query<Ddin2Measurement>(
                    "SELECT * FROM Ddin2Measurement WHERE Id =" + id).First();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ddin2GetById database error!" + "\n");
                throw;
            }
        }

    }
}


