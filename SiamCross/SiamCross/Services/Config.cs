using SiamCross.Models;
using SiamCross.Services.RepositoryTables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
namespace SiamCross.Services
{
    public class Config
    {
        public Task InitAsync()
        {
            return Task.CompletedTask;
        }
        IDbTransaction BeginTransaction()
        {
            return DbService.Instance.Db.BeginTransaction(IsolationLevel.Serializable);
        }

        public async Task<Dictionary<AttributeItem, object>> GetAsync(EntityKind entyty, long entityId)
        {
            var dataInt = new Dictionary<AttributeItem, long>();
            var dataFloat = new Dictionary<AttributeItem, double>();
            var dataString = new Dictionary<AttributeItem, string>();
            using (var tr = BeginTransaction())
            {
                dataInt = await DbService.Instance.DataInt.Load(tr, entyty, entityId);
                dataFloat = await DbService.Instance.DataFloat.Load(tr, entyty, entityId);
                dataString = await DbService.Instance.DataString.Load(tr, entyty, entityId);
            }
            var ret = new Dictionary<AttributeItem, object>();
            foreach (var item in dataInt)
                ret.Add(item.Key, item.Value);
            foreach (var item in dataFloat)
                ret.Add(item.Key, item.Value);
            foreach (var item in dataString)
                ret.Add(item.Key, item.Value);
            return ret;
        }


        static void ComposeObject<T>(Dictionary<long, Dictionary<string, T>> srcDict
            , Dictionary<long, Dictionary<string, object>> dstDict)
        {
            foreach (var src in srcDict)
            {
                Dictionary<string, object> atts = null;
                if (!dstDict.TryGetValue(src.Key, out atts))
                {
                    atts = new Dictionary<string, object>();
                    dstDict.Add(src.Key, atts);
                }
                foreach (var val in src.Value)
                    atts.Add(val.Key, val.Value);
            }
        }

        public async Task<Dictionary<long, Dictionary<string, object>>> GetAsync(EntityKind entyty)
        {
            var dataInt = new Dictionary<long, Dictionary<string, long>>();
            var dataFloat = new Dictionary<long, Dictionary<string, double>>();
            var dataString = new Dictionary<long, Dictionary<string, string>>();
            using (var tr = BeginTransaction())
            {
                dataInt = await DbService.Instance.DataInt.Load(tr, entyty);
                dataFloat = await DbService.Instance.DataFloat.Load(tr, entyty);
                dataString = await DbService.Instance.DataString.Load(tr, entyty);
            }
            var ret = new Dictionary<long, Dictionary<string, object>>();

            ComposeObject(dataInt, ret);
            ComposeObject(dataFloat, ret);
            ComposeObject(dataString, ret);

            return ret;
        }
        public async Task SetAsync(EntityKind entityKind, long entityId, Dictionary<string, object> keyValues)
        {
            var strDir = new Dictionary<AttributeItem, string>();
            var intDir = new Dictionary<AttributeItem, long>();
            var floatDir = new Dictionary<AttributeItem, double>();
            //AttributeItem attItem = null;
            foreach (var item in keyValues)
            {
                var typeCode = Type.GetTypeCode(item.Value.GetType());
                Repo.AttrDir.ByTitle.TryGetValue(item.Key, out AttributeItem attItem);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        if (null == attItem)
                            attItem = await Repo.AttrDir.SaveAsync(item.Key, AttributeType.Int);
                        intDir.Add(attItem, Convert.ToBoolean(item.Value) ? 1 : 0);
                        break;
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        if (null == attItem)
                            attItem = await Repo.AttrDir.SaveAsync(item.Key, AttributeType.Int);
                        intDir.Add(attItem, Convert.ToInt64(item.Value));
                        break;
                    case TypeCode.Double:
                    case TypeCode.Single:
                        if (null == attItem)
                            attItem = await Repo.AttrDir.SaveAsync(item.Key, AttributeType.Float);
                        floatDir.Add(attItem, Convert.ToDouble(item.Value));
                        break;
                    case TypeCode.String:
                        if (null == attItem)
                            attItem = await Repo.AttrDir.SaveAsync(item.Key, AttributeType.String);
                        strDir.Add(attItem, Convert.ToString(item.Value));
                        break;
                    default:
                        break;
                }
                using (var tr = BeginTransaction())
                {
                    await DbService.Instance.DataInt.Save(tr, entityKind, entityId, intDir);
                    await DbService.Instance.DataFloat.Save(tr, entityKind, entityId, floatDir);
                    await DbService.Instance.DataString.Save(tr, entityKind, entityId, strDir);
                    tr.Commit();
                }

            }
        }

    }
}
