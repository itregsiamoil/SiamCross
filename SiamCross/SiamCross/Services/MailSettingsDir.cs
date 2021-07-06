using SiamCross.Models;
using SiamCross.Models.Tools;
using SiamCross.Services.RepositoryTables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace SiamCross.Services
{
    [Preserve(AllMembers = true)]
    internal class MailSettingsItem
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
    }
    public class MailSettingsDir
    {
        private MailSettingsData _MailSettingsData = new MailSettingsData();

        public async Task InitAsync()
        {
            var dataInt = new Dictionary<AttributeItem, long>();
            var dataFloat = new Dictionary<AttributeItem, double>();
            var dataString = new Dictionary<AttributeItem, string>();
            using (var tr = BeginTransaction())
            {
                dataInt = await DbService.Instance.DataInt.Load(tr, EntityKind.MailConfig);
                dataFloat = await DbService.Instance.DataFloat.Load(tr, EntityKind.MailConfig);
                dataString = await DbService.Instance.DataString.Load(tr, EntityKind.MailConfig);
            }
            MailSettings ss = new MailSettings(_MailSettingsData);
            var propArray = ClassPropertiesConverter.GetProperties(ss);

            foreach (var prop in propArray)
            {
                Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out AttributeItem attItem);
                if (null == attItem)
                    continue;
                var typeCode = Type.GetTypeCode(prop.PropertyType);
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        prop.SetValue(ss, 0 != (int)dataInt[attItem]); break;
                    case TypeCode.Int32:
                        prop.SetValue(ss, (int)dataInt[attItem]); break;
                    case TypeCode.Int64:
                        prop.SetValue(ss, (long)dataInt[attItem]); break;
                    case TypeCode.UInt32:
                        prop.SetValue(ss, (uint)dataInt[attItem]); break;
                    case TypeCode.UInt64:
                        prop.SetValue(ss, (ulong)dataInt[attItem]); break;
                    case TypeCode.Double:
                        prop.SetValue(ss, (double)dataFloat[attItem]); break;
                    case TypeCode.Single:
                        prop.SetValue(ss, (float)dataFloat[attItem]); break;
                    case TypeCode.String:
                        prop.SetValue(ss, dataString[attItem]); break;
                    default:
                        break;
                }
            }
            _MailSettingsData = ss.GetData();
        }
        public async Task SaveAsync(MailSettingsData settings)
        {
            var strDir = new Dictionary<AttributeItem, string>();
            var intDir = new Dictionary<AttributeItem, long>();
            var floatDir = new Dictionary<AttributeItem, double>();

            MailSettings ss = new MailSettings(settings);
            var propArray = ClassPropertiesConverter.GetProperties(ss);

            foreach (var prop in propArray)
            {
                var typeCode = Type.GetTypeCode(prop.PropertyType);
                AttributeItem attItem = null;
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        if (!Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out attItem))
                            attItem = await Repo.AttrDir.SaveAsync(prop.Name, AttributeType.Int);
                        intDir.Add(attItem, Convert.ToBoolean(prop.GetValue(ss)) ? 1 : 0);
                        break;
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        if (!Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out attItem))
                            attItem = await Repo.AttrDir.SaveAsync(prop.Name, AttributeType.Int);
                        intDir.Add(attItem, Convert.ToInt64(prop.GetValue(ss)));
                        break;
                    case TypeCode.Double:
                    case TypeCode.Single:
                        if (!Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out attItem))
                            attItem = await Repo.AttrDir.SaveAsync(prop.Name, AttributeType.Float);
                        floatDir.Add(attItem, Convert.ToDouble(prop.GetValue(ss)));
                        break;
                    case TypeCode.String:
                        if (!Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out attItem))
                            attItem = await Repo.AttrDir.SaveAsync(prop.Name, AttributeType.String);
                        strDir.Add(attItem, Convert.ToString(prop.GetValue(ss)));
                        break;
                    default:
                        break;
                }
            }
            using (var tr = BeginTransaction())
            {
                await DbService.Instance.DataInt.Save(tr, EntityKind.MailConfig, 0, intDir);
                await DbService.Instance.DataFloat.Save(tr, EntityKind.MailConfig, 0, floatDir);
                await DbService.Instance.DataString.Save(tr, EntityKind.MailConfig, 0, strDir);
                tr.Commit();
            }
            _MailSettingsData = settings;
        }
        public Task<MailSettingsData> GetData()
        {
            return Task.FromResult(_MailSettingsData);
        }
        private IDbTransaction BeginTransaction()
        {
            return DbService.Instance.Db.BeginTransaction(IsolationLevel.Serializable);
        }


    }
}
