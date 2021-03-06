﻿using Dapper;
using SiamCross.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Services.RepositoryTables
{
    [Preserve(AllMembers = true)]
    public class MeasureTableItem
    {
        public MeasureData MeasureData { get; private set; }

        public MeasureTableItem()
        {
            MeasureData = new MeasureData(
                new Position()
                , new Models.DeviceInfo()
                , new CommonInfo()
                , new MeasurementInfo()
                , new DistributionInfo()
                , new DistributionInfo()
                );
        }
        public MeasureTableItem(MeasureData mi)
        {
            MeasureData = mi;
        }

        public long Id { get => MeasureData.Id; set => MeasureData.Id = value; }

        public uint Field { get => MeasureData.Position.Field; set => MeasureData.Position.Field = value; }
        public string Well { get => MeasureData.Position.Well; set => MeasureData.Position.Well = value; }
        public string Bush { get => MeasureData.Position.Bush; set => MeasureData.Position.Bush = value; }
        public uint Shop { get => MeasureData.Position.Shop; set => MeasureData.Position.Shop = value; }

        public uint DeviceKind { get => MeasureData.Device.Kind; set => MeasureData.Device.Kind = value; }
        public uint DeviceNumber { get => MeasureData.Device.Number; set => MeasureData.Device.Number = value; }
        public string DeviceName { get => MeasureData.Device.Name; set => MeasureData.Device.Name = value; }
        public uint DeviceProtocolId { get => MeasureData.Device.ProtocolId; set => MeasureData.Device.ProtocolId = value; }
        public uint DevicePhyId { get => MeasureData.Device.PhyId; set => MeasureData.Device.PhyId = value; }

        public uint MeasureKind { get => MeasureData.Measure.Kind; set => MeasureData.Measure.Kind = value; }
        public DateTime MeasureBeginTimestamp { get => MeasureData.Measure.BeginTimestamp; set => MeasureData.Measure.BeginTimestamp = value; }
        public DateTime MeasureEndTimestamp { get => MeasureData.Measure.EndTimestamp; set => MeasureData.Measure.EndTimestamp = value; }
        public string MeasureComment { get => MeasureData.Measure.Comment; set => MeasureData.Measure.Comment = value; }

        public DateTime MailDistributionDateTime { get => MeasureData.MailDistribution.Timestamp; set => MeasureData.MailDistribution.Timestamp = value; }
        public string MailDistributioDestination { get => MeasureData.MailDistribution.Destination; set => MeasureData.MailDistribution.Destination = value; }

        public DateTime FileDistributionDateTime { get => MeasureData.FileDistribution.Timestamp; set => MeasureData.FileDistribution.Timestamp = value; }
        public string FileDistributionDestination { get => MeasureData.FileDistribution.Destination; set => MeasureData.FileDistribution.Destination = value; }

    }


    public class MeasureTable
    {
        public MeasureTable()
        {
        }
        public Task Delete(IDbTransaction tr, long measureId)
        {
            return tr.Connection.ExecuteAsync("DELETE FROM Measurement WHERE Id=" + measureId.ToString());
        }
        public async Task<long> Save(IDbTransaction tr, MeasureTableItem item)
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
            int affectedRows = await tr.Connection.ExecuteAsync(sql, item);

            var rec_id = await tr.Connection.QueryAsync<long?>("SELECT last_insert_rowid()");
            if (affectedRows < 1 || rec_id.Count() == 0 || null == rec_id.First())
                throw new Exception("can`t get last_insert_rowid ");

            return rec_id.First().Value;

        }

        public Task<IEnumerable<MeasureTableItem>> GetMeasurements(IDbTransaction tr)
        {
            return tr.Connection.QueryAsync<MeasureTableItem>("SELECT * FROM Measurement");
        }


    }

}

