using Autofac;
using Dapper;
using NLog;
using SiamCross.AppObjects;
using SiamCross.DataBase;
using SiamCross.DataBase.DataBaseModels;
using SiamCross.Models;
using SiamCross.Services.Environment;
using SiamCross.Services.Logging;
using SiamCross.Services.RepositoryTables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class DbService
    {
        private static readonly Logger _logger =
            AppContainer.Container.Resolve<ILogManager>().GetLog();

        private static readonly Lazy<DbService> _instance =
            new Lazy<DbService>(() => new DbService());
        public static DbService Instance => _instance.Value;

        private readonly IDbConnection _database;
        private IDbConnection _Db;

        public int UserDbVersion = 17;

        readonly Attributes Attributes = new Attributes();
        readonly FieldDictionaryTable FieldDictionary = new FieldDictionaryTable();
        readonly DataInt DataInt = new DataInt();
        readonly DataFloat DataFloat = new DataFloat();
        readonly DataString DataString = new DataString();
        readonly DataBlob DataBlob = new DataBlob();
        readonly MeasureTable MeasureTable = new MeasureTable();
        IDbTransaction BeginTransaction()
        {
            return _Db.BeginTransaction(IsolationLevel.Serializable);
        }
        static string GetDbPath(string dbFileName)
        {
#if DEBUG
            return Path.Combine(
                EnvironmentService.Instance.GetDir_Downloads(), dbFileName);
#else
                return Path.Combine(
                    EnvironmentService.Instance.GetDir_LocalApplicationData(), dbFileName);
#endif        
        }
        static IDbConnection OpenDb(string dbPath)
        {
            IDbConnection db = AppContainer.Container
                                .Resolve<IDbConnection>(new TypedParameter(typeof(string)
                                , $"Data Source={dbPath};Version=3;"));
            db.Open();
            db.Execute("PRAGMA foreign_keys = ON");
            var sql_version = db.ExecuteScalar<string>("select sqlite_version()");
            var sqlFk = db.Query<int?>("PRAGMA foreign_keys").AsList();
            if (1 > sqlFk.Count || !sqlFk[0].HasValue || 0 == sqlFk[0].Value)
                throw new Exception($"Database Sqlite version:{sql_version} does not support foreign keys");
            return db;
        }
        static async Task<IDbConnection> CreateDb(string dbPath, int userDbVersion)
        {
            AppContainer.Container.Resolve<IDatabaseCreator>().CreateDatabase(dbPath);
            var db = OpenDb(dbPath);
            using (var tr = db.BeginTransaction(IsolationLevel.Serializable))
            {
                await db.ExecuteAsync(GetCreateDataBaseScript("CreateDataBase.sql"), tr);
                await db.ExecuteAsync(GetCreateDataBaseScript("AppendData.sql"), tr);
                await db.ExecuteAsync($"PRAGMA user_version = {userDbVersion}", tr);
                tr.Commit();
            }
            return db;
        }
        public async Task Init()
        {
            string dbPath = GetDbPath("SiamServiceDB.sqlite");
            IDbConnection db = null;
            try
            {
                if (File.Exists(dbPath))
                {
                    db = OpenDb(dbPath);
                    var userVersionRec = (await db.QueryAsync<int?>("PRAGMA user_version")).AsList();
                    if (1 > userVersionRec.Count || !userVersionRec[0].HasValue || UserDbVersion != userVersionRec[0].Value)
                    {
                        db.Close();
                        File.Delete(dbPath);
                        db = await CreateDb(dbPath, UserDbVersion);
                    }
                }
                else
                    db = await CreateDb(dbPath, UserDbVersion);

                _Db = db;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
                _logger.Error(ex, $"Database creation error! Database path: {dbPath}" + "\n");
                throw;
            }
        }
        private DbService()
        {
            try
            {
                string dbPath = GetDbPath("Db.sqlite");
                if (!File.Exists(dbPath))
                    AppContainer.Container.Resolve<IDatabaseCreator>().CreateDatabase(dbPath);
                _database = OpenDb(dbPath);
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
                [BufferPressure] NUMERIC NOT NULL,
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
                [BufferPressure] NUMERIC NOT NULL,
                [Comment] NVARCHAR(128) NOT NULL,
                [Name] NVARCHAR(128) NOT NULL,
                [BatteryVolt] NVARCHAR(128) NOT NULL,
                [Temperature] NVARCHAR(128) NOT NULL,
                [MainFirmware] NVARCHAR(128) NOT NULL,
                [RadioFirmware] NVARCHAR(128) NOT NULL,
                [PumpDepth] NUMERIC NOT NULL)");
        }

        #endregion 

        public int SaveDuMeasurement(DuMeasurement duMeasurement)
        {
            NonQueryCheck();
            try
            {
                if (duMeasurement.Id == 0)
                {
                    IEnumerable<int?> idsColumn = _database.Query<Nullable<int>>(string.Format(
                        "SELECT max(Id)+1 as 'Id' FROM DuMeasurement"));
                    if (idsColumn.Count() > 0 && null != idsColumn.First())
                        duMeasurement.Id = idsColumn.First().Value;
                    else
                        duMeasurement.Id = 1;
                }
                {
                    IEnumerable<int?> rows1 = _database.Query<int?>(string.Format(
                        "SELECT COUNT(1) AS 'Count' FROM DuMeasurement WHERE Id = '{0}'", duMeasurement.Id));
                    if (rows1.Count() > 0 && null != rows1.First() && 0 < rows1.First().Value)
                    {
                        UpdateDuMeasurement(duMeasurement);
                        return duMeasurement.Id;
                    }
                }

                string sql = "INSERT INTO DuMeasurement (" +
                        "  SrcFluidLevel, SrcReflectionsCount, AnnularPressure" +
                        ", Echogram, SoundSpeed, MeasurementType" +
                        ", SoundSpeedCorrection, ReportTimestamp, Field" +
                        ", Well, Bush, Shop" +
                        ", BufferPressure, Comment, Name" +
                        ", BatteryVolt, Temperature, MainFirmware, RadioFirmware" +
                        ", PumpDepth " +
                        ") Values (" +
                        "  @SrcFluidLevel, @SrcReflectionsCount, @AnnularPressure" +
                        ", @Echogram, @SoundSpeed, @MeasurementType" +
                        ", @SoundSpeedCorrection, @ReportTimestamp,  @Field" +
                        ", @Well, @Bush, @Shop" +
                        ", @BufferPressure, @Comment, @Name " +
                        ", @BatteryVolt, @Temperature, @MainFirmware, @RadioFirmware " +
                        ", @PumpDepth" +
                        "  );";

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
                    duMeasurement.PumpDepth
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
                    " ,PumpDepth=@PumpDepth " +
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
                    duMeasurement.PumpDepth,
                    duMeasurement.Id
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UpdateDuMeasurement" + "\n");
                throw;
            }
        }
        public async Task RemoveDuMeasurementAsync(long removebleId)
        {
            NonQueryCheck();
            try
            {
                await _database.ExecuteAsync("DELETE FROM DuMeasurement WHERE Id =" + removebleId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RemoveDuMeasurement" + "\n");
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
        public async Task<DuMeasurement> GetDuMeasurementByIdAsync(long id)
        {
            NonQueryCheck();
            try
            {
                var resilt = await _database.QueryAsync<DuMeasurement>(
                    "SELECT * FROM DuMeasurement WHERE Id =" + id);
                return resilt.First();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetDuMeasurementById "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return null;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public int SaveDdin2Measurement(Ddin2Measurement ddin2Measurement)
        {
            NonQueryCheck();

            try
            {
                if (ddin2Measurement.Id == 0)
                {
                    IEnumerable<int?> idsColumn = _database.Query<int?>(string.Format(
                        "SELECT max(Id)+1 as 'Id' FROM Ddin2Measurement"));
                    if (idsColumn.Count() > 0 && null != idsColumn.First())
                        ddin2Measurement.Id = idsColumn.First().Value;
                    else
                        ddin2Measurement.Id = 1;
                }
                {
                    IEnumerable<int?> rows2 = _database.Query<int?>(string.Format(
                        "SELECT COUNT(1) as 'Count' FROM Ddin2Measurement WHERE Id = '{0}'",
                        ddin2Measurement.Id));
                    if (rows2.Count() > 0 && null != rows2.First() && 0 < rows2.First().Value)
                    {
                        UpdateDdin2Measurement(ddin2Measurement);
                        return ddin2Measurement.Id;
                    }
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
        public void RemoveDdin2Measurement(long removebleId)
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
        public Ddin2Measurement GetDdin2MeasurementById(long id)
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
            }
            return null;
        }

        static string GetCreateDataBaseScript(string resourceName)
        {
            //Получаем текущую сборку.
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            string fullResourceName = $"{myAssembly.GetName().Name}.{resourceName.Replace('\\', '.')}";
            bool isExistsResourceName = myAssembly.GetManifestResourceNames()
                .Contains(fullResourceName); //уточняем существует этот ресурс в данной сборке.
            string ret = string.Empty;
            if (isExistsResourceName)
            {
                Stream stream = myAssembly.GetManifestResourceStream(fullResourceName);
                StreamReader reader = new StreamReader(stream);
                ret = reader.ReadToEnd();
            }
            return ret;
        }

        public async Task GetValuesAsync(MeasureData m)
        {
            using (var tr = BeginTransaction())
            {
                m.Measure.DataInt = await DataInt.Load(tr, EntityKind.Survey, m.Id);
                m.Measure.DataFloat = await DataFloat.Load(tr, EntityKind.Survey, m.Id);
                m.Measure.DataString = await DataString.Load(tr, EntityKind.Survey, m.Id);
                m.Measure.DataBlob = await DataBlob.Load(tr, EntityKind.Survey, m.Id);
                tr.Commit();
            }
        }
        public async Task<IEnumerable<MeasureTableItem>> GetSurveysAsync()
        {
            NonQueryCheck();
            try
            {
                using (var tr = BeginTransaction())
                {
                    var v = await MeasureTable.GetMeasurements(tr);
                    tr.Commit();
                    return v;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "EXCEPTION "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            //return Task.FromResult<IEnumerable<MeasureTableItem>>(new List<MeasureTableItem>());
            return new List<MeasureTableItem>();
        }
        public async Task<long> SaveSurveyAsync(MeasureData survey)
        {
            NonQueryCheck();
            IDbTransaction tr = null; ;
            try
            {
                using (tr = BeginTransaction())
                {
                    long measureId = await MeasureTable.Save(tr, new MeasureTableItem(survey));
                    await DataInt.Save(tr, EntityKind.Survey, measureId, survey.Measure.DataInt);
                    await DataFloat.Save(tr, EntityKind.Survey, measureId, survey.Measure.DataFloat);
                    await DataString.Save(tr, EntityKind.Survey, measureId, survey.Measure.DataString);
                    await DataBlob.Save(tr, EntityKind.Survey, measureId, survey.Measure.DataBlob);
                    tr.Commit();
                    return measureId;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
            return -1;
        }
        public async Task DelSurveyAsync(long measureId)
        {
            NonQueryCheck();
            IDbTransaction tr = null; ;
            try
            {
                using (tr = BeginTransaction())
                {
                    await MeasureTable.Delete(tr, measureId);
                    await DataInt.Delete(tr, EntityKind.Survey, measureId);
                    await DataFloat.Delete(tr, EntityKind.Survey, measureId);
                    await DataString.Delete(tr, EntityKind.Survey, measureId);
                    await DataBlob.Delete(tr, EntityKind.Survey, measureId);
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                tr?.Rollback();
                Debug.WriteLine("EXCEPTION "
                    + System.Reflection.MethodBase.GetCurrentMethod().Name
                    + "\n msg=" + ex.Message
                    + "\n type=" + ex.GetType()
                    + "\n stack=" + ex.StackTrace + "\n");
            }
        }

        public async Task DelFieldAsync(long id)
        {
            using (var tr = BeginTransaction())
            {
                await FieldDictionary.DeleteAsync(tr, id);
                tr.Commit();
            }
        }
        public async Task<FieldItem> SaveFieldAsync(string title, uint id = 0)
        {
            using (var tr = BeginTransaction())
            {
                var val = await FieldDictionary.SaveAsync(tr, title, id);
                tr.Commit();
                return val;
            }
        }
        public async Task<List<FieldItem>> LoadFieldAsync(long id)
        {
            using (var tr = BeginTransaction())
                return await FieldDictionary.LoadAsync(tr, id);
        }
        public async Task<List<FieldItem>> LoadFieldAsync()
        {
            using (var tr = BeginTransaction())
                return await FieldDictionary.LoadAsync(tr);
        }
        public async Task<IEnumerable<AttributeItem>> LoadAttributesAsync()
        {
            using (var tr = BeginTransaction())
                return await Attributes.LoadAsync(tr);

        }
    }
}


