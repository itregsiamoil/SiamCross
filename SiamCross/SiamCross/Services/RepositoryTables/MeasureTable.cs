using SiamCross.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Linq;

namespace SiamCross.Services.RepositoryTables
{
	public class MeasureTableItem
	{
		public MeasureData MeasureInfo { get; private set;}

		public MeasureTableItem()
		{
			MeasureInfo = new MeasureData(
				new PositionInfo()
				, new DeviceInfo()
				, new CommonInfo()
				, new MeasurementInfo()
				, new DistributionInfo()
				, new DistributionInfo()
				);
		}
		public MeasureTableItem(MeasureData mi)
        {
			MeasureInfo = mi;
		}

		public long Id { get => MeasureInfo.Id; set => MeasureInfo.Id = value; }

		public uint Field { get => MeasureInfo.Position.Field; set => MeasureInfo.Position.Field = value; }
		public uint Well { get => MeasureInfo.Position.Well; set => MeasureInfo.Position.Well = value; }
		public uint Bush { get => MeasureInfo.Position.Bush; set => MeasureInfo.Position.Bush = value; }
		public uint Shop { get => MeasureInfo.Position.Shop; set => MeasureInfo.Position.Shop = value; }

		public uint DeviceKind { get => MeasureInfo.Device.Kind; set => MeasureInfo.Device.Kind = value; }
		public uint DeviceNumber { get => MeasureInfo.Device.Number; set => MeasureInfo.Device.Number = value; }
		public string DeviceName { get => MeasureInfo.Device.Name; set => MeasureInfo.Device.Name = value; }
		public uint DeviceProtocolId { get => MeasureInfo.Device.ProtocolId; set => MeasureInfo.Device.ProtocolId = value; }
		public uint DevicePhyId { get => MeasureInfo.Device.PhyId; set => MeasureInfo.Device.PhyId = value; }

		public uint MeasureKind { get => MeasureInfo.Measure.Kind; set => MeasureInfo.Measure.Kind = value; }
		public DateTime MeasureBeginTimestamp { get => MeasureInfo.Measure.BeginTimestamp; set => MeasureInfo.Measure.BeginTimestamp = value; }
		public DateTime MeasureEndTimestamp { get => MeasureInfo.Measure.EndTimestamp; set => MeasureInfo.Measure.EndTimestamp = value; }
		public string MeasureComment { get => MeasureInfo.Measure.Comment; set => MeasureInfo.Measure.Comment = value; }

		public DateTime MailDistributionDateTime { get => MeasureInfo.MailDistribution.Timestamp; set => MeasureInfo.MailDistribution.Timestamp = value; }
		public string MailDistributioDestination { get => MeasureInfo.MailDistribution.Destination; set => MeasureInfo.MailDistribution.Destination = value; }

		public DateTime FileDistributionDateTime { get => MeasureInfo.FileDistribution.Timestamp; set => MeasureInfo.FileDistribution.Timestamp = value; }
		public string FileDistributionDestination { get => MeasureInfo.FileDistribution.Destination; set => MeasureInfo.FileDistribution.Destination = value; }

	}


    public class MeasureTable
    {
		private readonly IDbConnection _db;
		public MeasureTable(IDbConnection db)
		{
			_db = db;
		}


		public async Task<long> Save(MeasureTableItem item)
		{
			const string sql = "INSERT INTO Measurement" +
				"( Field, Well, Bush, Shop" +
				", DeviceKind, DeviceNumber, DeviceName" +
				", DeviceProtocolId, DevicePhyId" +
				", MeasureKind, MeasureBeginTimestamp, MeasureEndTimestamp, MeasureComment" +
				") VALUES (" +
				"  @Field, @Well, @Bush, @Shop" +
				", @DeviceKind, @DeviceNumber, @DeviceName" +
				", @DeviceProtocolId, @DevicePhyId" +
				", @MeasureKind, @MeasureBeginTimestamp, @MeasureEndTimestamp, @MeasureComment" +
				")";
			/*
			var param = new
			{
				Field = item.Field, 
				Well = item.Well,
				Bush = item.Bush, 
				Shop = item.Shop,

				DeviceKind = item.DeviceKind,
				DeviceNumber = item.DeviceNumber,
				DeviceName = item.DeviceName,
				DeviceProtocolId = item.DeviceProtocolId,
				DevicePhyId = item.DevicePhyId,

				MeasureKind = item.MeasureKind,
				MeasureBeginTimestamp = item.MeasureBeginTimestamp,
				MeasureEndTimestamp = item.MeasureEndTimestamp,
				MeasureComment = item.MeasureComment 
			};
			*/
			int affectedRows = await _db.ExecuteAsync(sql, item);

			var rec_id = await _db.QueryAsync<long?>("SELECT last_insert_rowid()");
			if (rec_id.Count() == 0 || null == rec_id.First())
				throw new Exception("can`t get last_insert_rowid ");

			return rec_id.First().Value;

		}

		public Task<IEnumerable<MeasureTableItem>> GetMeasurements()
		{
			return _db.QueryAsync<MeasureTableItem>("SELECT * FROM Measurement");
		}


	}

}

