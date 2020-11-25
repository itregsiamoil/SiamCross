﻿using Autofac;
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
        public static DataRepository Instance { get => _instance.Value; }
        private IDbConnection _database;
        private static object _locker = new object();
        private string _databasePart;

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
            catch(Exception e)
            {
                _logger.Error(e, $"Database creation error! Database path: {_databasePart}" + "\n");
                throw;
            }

            try
            {
                CreateDdim2Table();
                CreateDdin2Table();
                CreateSiddosA3MTable();
                CreateDuTable();
            }
            catch(Exception e)
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

        private void CreateDdim2Table()
        {
            NonQueryCheck();

            _database.Execute(@"
            CREATE TABLE IF NOT EXISTS [Ddim2Measurement] (
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
                [MaxBarbellWeight] REAL NOT NULL,
                [MinBarbellWeight] REAL NOT NULL,
                [TravelLength] REAL NOT NULL,
                [SwingCount] REAL NOT NULL,
                [BatteryVolt] NVARCHAR(128) NOT NULL,
                [Temperature] NVARCHAR(128) NOT NULL,
                [MainFirmware] NVARCHAR(128) NOT NULL,
                [RadioFirmware] NVARCHAR(128) NOT NULL)");
        }

        private void CreateSiddosA3MTable()
        {
            NonQueryCheck();

            _database.Execute(@"
            CREATE TABLE IF NOT EXISTS [SiddosA3MMeasurement] (
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
                [MaxBarbellWeight] REAL NOT NULL,
                [MinBarbellWeight] REAL NOT NULL,
                [TravelLength] REAL NOT NULL,
                [SwingCount] REAL NOT NULL,
                [BatteryVolt] NVARCHAR(128) NOT NULL,
                [Temperature] NVARCHAR(128) NOT NULL,
                [MainFirmware] NVARCHAR(128) NOT NULL,
                [RadioFirmware] NVARCHAR(128) NOT NULL)");
        }

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

            _database.Execute(@"
            CREATE TABLE IF NOT EXISTS [DuMeasurement] (
                [Id] INTEGER NOT NULL PRIMARY KEY,
                [FluidLevel] INTEGER NOT NULL,
                [NumberOfReflections] INTEGER NOT NULL,
                [AnnularPressure] REAL NOT NULL,
                [Echogram] BLOB,
                [SoundSpeed] NVARCHAR(128) NOT NULL,
                [MeasurementType] NVARCHAR(128) NOT NULL,
                [SoundSpeedCorrection] NVARCHAR(128) NOT NULL,
                [DateTime] TEXT NOT NULL,
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
                    var idsColumn = _database.Query<int>(string.Format(
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

                var rows = _database.Query(string.Format(
                    "SELECT COUNT(1) AS 'Count' FROM DuMeasurement WHERE Id = '{0}'", duMeasurement.Id));

                if (rows.First().Count > 0)
                {
                    UpdateDuMeasurement(duMeasurement);
                    return duMeasurement.Id;
                }

                string sql = "INSERT INTO DuMeasurement " +
                        "(FluidLevel, " +
                        "NumberOfReflections, " +
                        "AnnularPressure, " +
                        "Echogram, " +
                        "SoundSpeed, " +
                        "MeasurementType, " +
                        "SoundSpeedCorrection, " +
                        "DateTime, Field, Well, Bush, Shop, BufferPressure, Comment, Name" +
                        ", BatteryVolt, Temperature, MainFirmware, RadioFirmware "+
                        ") Values (" +
                        "@FluidLevel, " +
                        "@NumberOfReflections, " +
                        "@AnnularPressure, " +
                        "@Echogram, " +
                        "@SoundSpeed, " +
                        "@MeasurementType, " +
                        "@SoundSpeedCorrection, " +
                        "@DateTime,  @Field, @Well, @Bush, @Shop, " +
                        "@BufferPressure, @Comment, @Name "+
                        ", @BatteryVolt, @Temperature, @MainFirmware, @RadioFirmware  );";
                var affectedRows = _database.Execute(sql, new
                {
                    duMeasurement.FluidLevel,
                    duMeasurement.NumberOfReflections,
                    duMeasurement.AnnularPressure,
                    duMeasurement.Echogram,
                    duMeasurement.SoundSpeed,
                    duMeasurement.MeasurementType,
                    duMeasurement.SoundSpeedCorrection,
                    duMeasurement.DateTime,
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
                _logger.Error(ex, "SaveDuMeasurement" + "\n");
                throw;
            }
            return duMeasurement.Id;
        }

        
        public void UpdateDuMeasurement(DuMeasurement duMeasurement)
        {
            try
            {
                string sql = "UPDATE DuMeasurement SET " +
                    "FluidLevel = @FluidLevel, NumberOfReflections = @NumberOfReflections, " +
                    "AnnularPressure = @AnnularPressure, Echogram = @Echogram, SoundSpeed = @SoundSpeed, " +
                    "MeasurementType = @MeasurementType, SoundSpeedCorrection = @SoundSpeedCorrection, " +
                    "DateTime = @DateTime," +
                    " Field = @Field, Well = @Well, Bush = @Bush, Shop = @Shop, BufferPressure = @BufferPressure," +
                    " Comment = @Comment, Name = @Name "+
                    " ,BatteryVolt=@BatteryVolt, Temperature=@Temperature "+
                    " ,MainFirmware=@MainFirmware, RadioFirmware=@RadioFirmware "+
                    " WHERE Id = @Id;";
                var affectedRows = _database.Execute(sql, new
                {
                    duMeasurement.FluidLevel,
                    duMeasurement.NumberOfReflections,
                    duMeasurement.AnnularPressure,
                    duMeasurement.Echogram,
                    duMeasurement.SoundSpeed,
                    duMeasurement.MeasurementType,
                    duMeasurement.SoundSpeedCorrection,
                    duMeasurement.DateTime,
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

        public int SaveDdim2Measurement(Ddim2Measurement ddim2Measurement)
        {
            NonQueryCheck();
            try
            {
                if (ddim2Measurement.Id == 0)
                {
                    var idsColumn = _database.Query<int>(string.Format(
                        "SELECT Id FROM Ddim2Measurement"));
                    if (idsColumn.Count() > 0)
                    {
                        ddim2Measurement.Id = idsColumn.Max() + 1;
                    }
                    else
                    {
                        ddim2Measurement.Id = 1;
                    }
                }

                var rows = _database.Query(string.Format(
                "SELECT COUNT(1) as 'Count' FROM Ddim2Measurement WHERE Id = '{0}'",
                ddim2Measurement.Id));

                if (rows.First().Count > 0)
                {
                    UpdateDdim2Measurement(ddim2Measurement);
                    return ddim2Measurement.Id;
                }

                string sql = "INSERT INTO Ddim2Measurement" +
                        "(MaxWeight, MinWeight, Travel, Period, Step, WeightDiscr, TimeDiscr, DynGraph," +
                        " AccelerationGraph, Field, Well, Bush, Shop, BufferPressure, Comment, " +
                        "Name, DateTime, ErrorCode, ApertNumber, ModelPump, MaxBarbellWeight," +
                        " MinBarbellWeight, TravelLength, SwingCount" +
                        ", BatteryVolt, Temperature, MainFirmware, RadioFirmware "+
                        ") Values (@MaxWeight, @MinWeight," +
                        " @Travel, @Period, @Step, @WeightDiscr, @TimeDiscr, @DynGraph," +
                        " @AccelerationGraph, @Field, @Well, @Bush, @Shop, @BufferPressure, @Comment, " +
                        "@Name, @DateTime, @ErrorCode, @ApertNumber, @ModelPump, @MaxBarbellWeight," +
                        " @MinBarbellWeight, @TravelLength, @SwingCount "+
                        ", @BatteryVolt, @Temperature, @MainFirmware, @RadioFirmware  );";


                var affectedRows = _database.Execute(sql, new
                {
                    MaxWeight = ddim2Measurement.MaxWeight,
                    MinWeight = ddim2Measurement.MinWeight,
                    DynGraph = ddim2Measurement.DynGraph,
                    Travel = ddim2Measurement.Travel,
                    Period = ddim2Measurement.Period,
                    Step = ddim2Measurement.Step,
                    WeightDiscr = ddim2Measurement.WeightDiscr,
                    TimeDiscr = ddim2Measurement.TimeDiscr,
                    AccelerationGraph = ddim2Measurement.AccelerationGraph,
                    Field = ddim2Measurement.Field,
                    Well = ddim2Measurement.Well,
                    Bush = ddim2Measurement.Bush,
                    Shop = ddim2Measurement.Shop,
                    BufferPressure = ddim2Measurement.BufferPressure,
                    Comment = ddim2Measurement.Comment,
                    Name = ddim2Measurement.Name,
                    DateTime = ddim2Measurement.DateTime,
                    ErrorCode = ddim2Measurement.ErrorCode,
                    ApertNumber = ddim2Measurement.ApertNumber,
                    ModelPump = ddim2Measurement.ModelPump,
                    MaxBarbellWeight = ddim2Measurement.MaxBarbellWeight,
                    MinBarbellWeight = ddim2Measurement.MinBarbellWeight,
                    TravelLength = ddim2Measurement.TravelLength,
                    SwingCount = ddim2Measurement.SwingCount,
                    BatteryVolt = ddim2Measurement.BatteryVolt,
                    Temperature = ddim2Measurement.Temperature,
                    MainFirmware = ddim2Measurement.MainFirmware,
                    RadioFirmware = ddim2Measurement.RadioFirmware
                });
            }
            catch(Exception e)
            {
                _logger.Error(e, $"Ddim2Save database error!" + "\n");
                throw;
            }

            return ddim2Measurement.Id;
        }

        
        private void UpdateDdim2Measurement(Ddim2Measurement ddim2Measurement)
        {
            try
            {
                string sql = "UPDATE Ddim2Measurement SET " +
                    "MaxWeight = @MaxWeight, MinWeight = @MinWeight, Travel = @Travel," +
                    " Period = @Period, Step = @Step, WeightDiscr = @WeightDiscr," +
                    " TimeDiscr = @TimeDiscr, DynGraph = @DynGraph," +
                    " AccelerationGraph = @AccelerationGraph, Field = @Field," +
                    " Well = @Well, Bush = @Bush, Shop = @Shop, BufferPressure = @BufferPressure," +
                    " Comment = @Comment, Name = @Name, DateTime = @DateTime," +
                    " ErrorCode = @ErrorCode, ApertNumber = @ApertNumber," +
                    " ModelPump = @ModelPump, MaxBarbellWeight = @MaxBarbellWeight," +
                    " MinBarbellWeight = @MinBarbellWeight, TravelLength = @TravelLength, SwingCount = @SwingCount" +
                    " ,BatteryVolt=@BatteryVolt, Temperature=@Temperature " +
                    " ,MainFirmware=@MainFirmware, RadioFirmware=@RadioFirmware " +
                    " WHERE Id = @Id;";

                var affectedRows = _database.Execute(sql, new
                {
                    MaxWeight = ddim2Measurement.MaxWeight,
                    MinWeight = ddim2Measurement.MinWeight,
                    Travel = ddim2Measurement.Travel,
                    Period = ddim2Measurement.Period,
                    Step = ddim2Measurement.Step,
                    WeightDiscr = ddim2Measurement.WeightDiscr,
                    TimeDiscr = ddim2Measurement.TimeDiscr,
                    DynGraph = ddim2Measurement.DynGraph,
                    AccelerationGraph = ddim2Measurement.AccelerationGraph,
                    Field = ddim2Measurement.Field,
                    Well = ddim2Measurement.Well,
                    Bush = ddim2Measurement.Bush,
                    Shop = ddim2Measurement.Shop,
                    BufferPressure = ddim2Measurement.BufferPressure,
                    Comment = ddim2Measurement.Comment,
                    Name = ddim2Measurement.Name,
                    DateTime = ddim2Measurement.DateTime,
                    ErrorCode = ddim2Measurement.ErrorCode,
                    ApertNumber = ddim2Measurement.ApertNumber,
                    ModelPump = ddim2Measurement.ModelPump,
                    MaxBarbellWeight = ddim2Measurement.MaxBarbellWeight,
                    MinBarbellWeight = ddim2Measurement.MinBarbellWeight,
                    TravelLength = ddim2Measurement.TravelLength,
                    SwingCount = ddim2Measurement.SwingCount,
                    BatteryVolt = ddim2Measurement.BatteryVolt,
                    Temperature = ddim2Measurement.Temperature,
                    MainFirmware = ddim2Measurement.MainFirmware,
                    RadioFirmware = ddim2Measurement.RadioFirmware,
                    Id = ddim2Measurement.Id
                });
            }
            catch(Exception e)
            {
                _logger.Error(e, $"Ddim2Update database error!" + "\n");
                throw;
            }
        }

        public void RemoveDdim2Measurement(int removebleId)
        {
            NonQueryCheck();
            try
            {
                _database.Execute("DELETE FROM Ddim2Measurement WHERE Id =" + removebleId);
            }
            catch(Exception e)
            {
                _logger.Error(e, $"Ddim2Delete database error!" + "\n");
                throw;
            }
        }

        public IEnumerable<DuMeasurement> GetDuMeasurements()
        {
            NonQueryCheck();
            try
            {
                var duItems = _database.Query<DuMeasurement>(
                    "SELECT * FROM DuMeasurement");
                return duItems;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetDuMeasurements" + "\n");
                throw;
            }
        }

        public IEnumerable<Ddim2Measurement> GetDdim2Measurements()
        {
            NonQueryCheck();
            try
            {
                var ddim2Items = _database.Query<Ddim2Measurement>(
                    "SELECT * FROM Ddim2Measurement");
           
            return ddim2Items;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Ddim2Get database error!" + "\n");
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
                _logger.Error(ex, "GetDuMeasurementById" + "\n");
                throw;
            }
        }

        public Ddim2Measurement GetDdim2MeasurementById(int id)
        {
            NonQueryCheck();
            try
            {
                return _database.Query<Ddim2Measurement>(
                    "SELECT * FROM Ddim2Measurement WHERE Id =" + id).First();
            }
            catch(Exception e)
            {
                _logger.Error(e, $"Ddim2GetById database error!" + "\n");
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
                    var idsColumn = _database.Query<int>(string.Format(
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

                var rows = _database.Query(string.Format(
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
                        " @MinBarbellWeight, @TravelLength, @SwingCount "+
                        ", @BatteryVolt, @Temperature, @MainFirmware, @RadioFirmware  );";

                var affectedRows = _database.Execute(sql, new
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
            catch(Exception e)
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

                var affectedRows = _database.Execute(sql, new
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
            catch(Exception e)
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
            catch(Exception e)
            {
                _logger.Error(e, $"Ddim2Remove database error!" + "\n");
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
            catch(Exception e)
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
            catch(Exception e)
            {
                _logger.Error(e, $"Ddin2GetById database error!" + "\n");
                throw;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public int SaveSiddosA3MMeasurement(SiddosA3MMeasurement siddosA3MMeasurement)
        {
            NonQueryCheck();

            try
            {
                if (siddosA3MMeasurement.Id == 0)
                {
                    var idsColumn =_database.Query<int>(string.Format(
                        "SELECT Id FROM SiddosA3MMeasurement"));
                    if(idsColumn.Count() > 0)
                    {
                        siddosA3MMeasurement.Id = idsColumn.Max() + 1;
                    }
                    else
                    {
                        siddosA3MMeasurement.Id = 1;
                    }
                }

                var rows = _database.Query(string.Format(
                "SELECT COUNT(1) as 'Count' FROM SiddosA3MMeasurement WHERE Id = '{0}'",
                siddosA3MMeasurement.Id));

                if (rows.First().Count > 0)
                {
                    UpdateSiddosA3MMeasurement(siddosA3MMeasurement);
                    return siddosA3MMeasurement.Id;
                }

                string sql = "INSERT INTO SiddosA3MMeasurement" +
                        "(MaxWeight, MinWeight, Travel, Period, Step, WeightDiscr, TimeDiscr, DynGraph," +
                        " AccelerationGraph, Field, Well, Bush, Shop, BufferPressure, Comment, " +
                        "Name, DateTime, ErrorCode, ApertNumber, ModelPump, MaxBarbellWeight," +
                        " MinBarbellWeight, TravelLength, SwingCount" +
                        ", BatteryVolt, Temperature, MainFirmware, RadioFirmware " +
                        ") Values (" +
                        "@MaxWeight, @MinWeight," +
                        " @Travel, @Period, @Step, @WeightDiscr, @TimeDiscr, @DynGraph," +
                        " @AccelerationGraph, @Field, @Well, @Bush, @Shop, @BufferPressure, @Comment, " +
                        "@Name, @DateTime, @ErrorCode, @ApertNumber, @ModelPump, @MaxBarbellWeight," +
                        " @MinBarbellWeight, @TravelLength, @SwingCount "+
                        ", @BatteryVolt, @Temperature, @MainFirmware, @RadioFirmware  );";

                var affectedRows = _database.Execute(sql, new
                {
                    MaxWeight = siddosA3MMeasurement.MaxWeight,
                    MinWeight = siddosA3MMeasurement.MinWeight,
                    Travel = siddosA3MMeasurement.Travel,
                    Period = siddosA3MMeasurement.Period,
                    Step = siddosA3MMeasurement.Step,
                    WeightDiscr = siddosA3MMeasurement.WeightDiscr,
                    TimeDiscr = siddosA3MMeasurement.TimeDiscr,
                    DynGraph = siddosA3MMeasurement.DynGraph,
                    AccelerationGraph = siddosA3MMeasurement.AccelerationGraph,
                    Field = siddosA3MMeasurement.Field,
                    Well = siddosA3MMeasurement.Well,
                    Bush = siddosA3MMeasurement.Bush,
                    Shop = siddosA3MMeasurement.Shop,
                    BufferPressure = siddosA3MMeasurement.BufferPressure,
                    Comment = siddosA3MMeasurement.Comment,
                    Name = siddosA3MMeasurement.Name,
                    DateTime = siddosA3MMeasurement.DateTime,
                    ErrorCode = siddosA3MMeasurement.ErrorCode,
                    ApertNumber = siddosA3MMeasurement.ApertNumber,
                    ModelPump = siddosA3MMeasurement.ModelPump,
                    MaxBarbellWeight = siddosA3MMeasurement.MaxBarbellWeight,
                    MinBarbellWeight = siddosA3MMeasurement.MinBarbellWeight,
                    TravelLength = siddosA3MMeasurement.TravelLength,
                    SwingCount = siddosA3MMeasurement.SwingCount,
                    BatteryVolt = siddosA3MMeasurement.BatteryVolt,
                    Temperature = siddosA3MMeasurement.Temperature,
                    MainFirmware = siddosA3MMeasurement.MainFirmware,
                    RadioFirmware = siddosA3MMeasurement.RadioFirmware
                });
            }
            catch(Exception e)
            {
                _logger.Error(e, $"SiddosSave database error!" + "\n");
                throw;
            }

            return siddosA3MMeasurement.Id;
        }

        private void UpdateSiddosA3MMeasurement(SiddosA3MMeasurement siddosA3MMeasurement)
        {
            try
            {
                string sql = "UPDATE SiddosA3MMeasurement SET " +
                    "MaxWeight = @MaxWeight, MinWeight = @MinWeight, Travel = @Travel," +
                    " Period = @Period, Step = @Step, WeightDiscr = @WeightDiscr," +
                    " TimeDiscr = @TimeDiscr, DynGraph = @DynGraph," +
                    " AccelerationGraph = @AccelerationGraph, Field = @Field," +
                    " Well = @Well, Bush = @Bush, Shop = @Shop, BufferPressure = @BufferPressure," +
                    " Comment = @Comment, Name = @Name, DateTime = @DateTime," +
                    " ErrorCode = @ErrorCode, ApertNumber = @ApertNumber," +
                    " ModelPump = @ModelPump, MaxBarbellWeight = @MaxBarbellWeight," +
                    " MinBarbellWeight = @MinBarbellWeight, TravelLength = @TravelLength, SwingCount = @SwingCount" +
                    " ,BatteryVolt=@BatteryVolt, Temperature=@Temperature " +
                    " ,MainFirmware=@MainFirmware, RadioFirmware=@RadioFirmware " +
                    " WHERE Id = @Id;";

                var affectedRows = _database.Execute(sql, new
                {
                    MaxWeight = siddosA3MMeasurement.MaxWeight,
                    MinWeight = siddosA3MMeasurement.MinWeight,
                    Travel = siddosA3MMeasurement.Travel,
                    Period = siddosA3MMeasurement.Period,
                    Step = siddosA3MMeasurement.Step,
                    WeightDiscr = siddosA3MMeasurement.WeightDiscr,
                    TimeDiscr = siddosA3MMeasurement.TimeDiscr,
                    DynGraph = siddosA3MMeasurement.DynGraph,
                    AccelerationGraph = siddosA3MMeasurement.AccelerationGraph,
                    Field = siddosA3MMeasurement.Field,
                    Well = siddosA3MMeasurement.Well,
                    Bush = siddosA3MMeasurement.Bush,
                    Shop = siddosA3MMeasurement.Shop,
                    BufferPressure = siddosA3MMeasurement.BufferPressure,
                    Comment = siddosA3MMeasurement.Comment,
                    Name = siddosA3MMeasurement.Name,
                    DateTime = siddosA3MMeasurement.DateTime,
                    ErrorCode = siddosA3MMeasurement.ErrorCode,
                    ApertNumber = siddosA3MMeasurement.ApertNumber,
                    ModelPump = siddosA3MMeasurement.ModelPump,
                    MaxBarbellWeight = siddosA3MMeasurement.MaxBarbellWeight,
                    MinBarbellWeight = siddosA3MMeasurement.MinBarbellWeight,
                    TravelLength = siddosA3MMeasurement.TravelLength,
                    SwingCount = siddosA3MMeasurement.SwingCount,
                    BatteryVolt = siddosA3MMeasurement.BatteryVolt,
                    Temperature = siddosA3MMeasurement.Temperature,
                    MainFirmware = siddosA3MMeasurement.MainFirmware,
                    RadioFirmware = siddosA3MMeasurement.RadioFirmware,
                    Id = siddosA3MMeasurement.Id
                });
            }
            catch(Exception e)
            {
                _logger.Error(e, $"SiddosUpdate database error!" + "\n");
                throw;
            }
        }

        public void RemoveSiddosA3MMeasurement(int removebleId)
        {
            NonQueryCheck();

            try
            {
                _database.Execute("DELETE FROM SiddosA3MMeasurement WHERE Id =" + removebleId);
            }
            catch(Exception e)
            {
                _logger.Error(e, $"SiddosRemove database error!" + "\n");
                throw;
            }
        }

        public IEnumerable<SiddosA3MMeasurement> GetSiddosA3MMeasurements()
        {
            NonQueryCheck();

            try
            {
                var siddosMeasutemenrs = _database.Query<SiddosA3MMeasurement>(
                    "SELECT * FROM SiddosA3MMeasurement");
                return siddosMeasutemenrs;
            }
            catch(Exception e)
            {
                _logger.Error(e, $"SiddosGet database error!" + "\n");
                throw;
            }
        }

        public SiddosA3MMeasurement GetSiddosA3MMeasurementById(int id)
        {
            NonQueryCheck();

            try
            {
                var siddosMeasurement = _database.Query<SiddosA3MMeasurement>(
                    "SELECT * FROM SiddosA3MMeasurement WHERE Id =" + id);
                return siddosMeasurement.First();
            }
            catch(Exception e)
            {
                _logger.Error(e, $"SiddosGetById database error!" + "\n");
                throw;
            }
        }
    }
}


