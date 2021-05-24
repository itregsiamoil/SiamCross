using Dapper;
using SiamCross.Models;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace SiamCross.Services.RepositoryTables
{
    public enum EntityKind
    {
        Survey = 0,
        SurveyAdditional = 1,
        DeviceProtocol = 10,
        DeviceConnection = 11
    };
    public enum AttributeType
    {
        Int = 1,
        Float = 5,
        String = 2,
        Blob = 3
    };

    public class DataItem<T>
    {
        public uint AttrId { get; set; }
        public T Value { get; set; }
    }

    public class ValueTable<T>
    {
        private readonly string insert_sql;
        private readonly string select_by_id_sql;
        private readonly string delete_by_entity;
        public ValueTable(AttributeType attrTypeId)
        {
            string table = string.Empty;
            switch (attrTypeId)
            {
                case AttributeType.Int: table = "ValInt"; break;
                case AttributeType.Float: table = "ValFloat"; break;
                case AttributeType.String: table = "ValString"; break;
                case AttributeType.Blob: table = "ValBlob"; break;
            }
            insert_sql = $"INSERT INTO {table}(EntityKindId, EntityId, AttrId, Value) VALUES(@EntityKindId, @EntityId, @AttrId, @Value)";
            select_by_id_sql = $"SELECT AttrId, Value FROM {table} WHERE EntityId=@EntityId AND EntityKindId=@EntityKindId";
            delete_by_entity = $"DELETE FROM {table} WHERE EntityId=@EntityId AND EntityKindId=@EntityKindId";
        }

        public virtual async Task Save(IDbTransaction tr, EntityKind entityKind, long entityId, Dictionary<AttributeItem, T> values)
        {
            foreach (var v in values)
            {
                var param = new
                {
                    EntityKindId = (int)entityKind,
                    EntityId = entityId,
                    AttrId = v.Key.Id,
                    Value = v.Value,
                };
                await tr.Connection.ExecuteAsync(insert_sql, param);
            }
        }
        public virtual async Task<Dictionary<AttributeItem, T>> Load(IDbTransaction tr, EntityKind entityKind, long entityId)
        {
            //const string sql = "SELECT * FROM  WHERE MeasureId=@MeasureId";
            var param = new
            {
                EntityKindId = entityKind,
                EntityId = entityId,
            };
            var values = await tr.Connection.QueryAsync<DataItem<T>>(select_by_id_sql, param, tr);
            var dict = new Dictionary<AttributeItem, T>();
            foreach (var v in values)
            {
                var attr = Repo.AttrDir.ById[v.AttrId];
                dict.Add(attr, v.Value);
            }
            return dict;
        }
        public virtual async Task Delete(IDbTransaction tr, EntityKind entityKind, long entityId)
        {
            var param = new
            {
                EntityKindId = entityKind,
                EntityId = entityId,
            };
            await tr.Connection.QueryAsync(delete_by_entity, param, tr);
        }
    }

    public class DataInt : ValueTable<long> { public DataInt() : base(AttributeType.Int) { } }
    public class DataFloat : ValueTable<double> { public DataFloat() : base(AttributeType.Float) { } }
    public class DataString : ValueTable<string> { public DataString() : base(AttributeType.String) { } }
    public class DataBlob : ValueTable<string>
    {
        public DataBlob()
            : base(AttributeType.Blob)
        { }
        public override async Task Save(IDbTransaction tr, EntityKind entityKind, long entityId, Dictionary<AttributeItem, string> values)
        {
            var path = Path.Combine(
                    System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal), "bin");

            var new_blobs = new Dictionary<AttributeItem, string>();
            foreach (var item in values)
            {
                string filename = $"{entityKind}_{entityId}_{item.Key}";
                var old_path = Path.Combine(path, values[item.Key]);
                var new_path = Path.Combine(path, filename);
                File.Delete(new_path);
                File.Move(old_path, new_path);
                new_blobs.Add(item.Key, filename);
            }
            values = new_blobs;
            await base.Save(tr, entityKind, entityId, values);
        }
        public override async Task Delete(IDbTransaction tr, EntityKind entityKind, long entityId)
        {
            var path = Path.Combine(
                System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal), "bin");

            var files = await Load(tr, entityKind, entityId);
            foreach (var f in files)
            {
                File.Delete(Path.Combine(path, f.Value));
            }
            await base.Delete(tr, entityKind, entityId);

        }
    }
}
