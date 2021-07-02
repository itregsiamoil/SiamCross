using Dapper;
using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.ObjectModel;
using SiamCross.Services.RepositoryTables;
using SiamCross.Models;

namespace SiamCross.Services
{
    internal class MailSettingsItem
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
    }
    public class MailSettingsDir
    {
        private MailSettingsData _MailSettingsData;

        readonly Dictionary<string, MailSettingsItem> DictByTitle = new Dictionary<string, MailSettingsItem>();
        readonly Dictionary<uint, MailSettingsItem> DictById = new Dictionary<uint, MailSettingsItem>();
        readonly ObservableRangeCollection<MailSettingsItem> Models = new ObservableRangeCollection<MailSettingsItem>();

        public async Task InitAsync()
        {
            /*
            
            DataInt DataInt = new DataInt();
            DataFloat DataFloat = new DataFloat();
            DataString DataString = new DataString();

            Dictionary<AttributeItem, long> intDict;

            
            using (var tr = BeginTransaction())
            {
                intDict = await DbService.Instance.DataInt.Load(tr, EntityKind.MailConfig);
            }

            





                        MailSettings ss = new MailSettings(_MailSettingsData);
                        var settingsArray = ClassPropertiesConverter.GetProperties(ss);

                        var strDir = new Dictionary<string, string>();
                        var intDir = new Dictionary<string, long>();
                        var floatDir = new Dictionary<string, double>();


                        foreach (var prop in settingsArray)
                        {
                            if (prop.GetValue(ss) is int || prop.GetValue(ss) is uint)
                                intDir.Add(prop.Name, Convert.ToInt64(prop.GetValue(ss)));
                            else if (prop.GetValue(ss) is float || prop.GetValue(ss) is double)
                                floatDir.Add(prop.Name, Convert.ToDouble(prop.GetValue(ss)));
                            else if (prop.GetValue(ss) is string || prop.GetValue(ss) is string)
                                strDir.Add(prop.Name, Convert.ToString(prop.GetValue(ss)));
                        }



                        DictByTitle.Clear();
                        DictById.Clear();
                        Models.Clear();

                        using (var tr = BeginTransaction())
                        {
                            var values = await tr.Connection.QueryAsync<SoundSpeedItem>(select_all);
                            foreach (var item in values)
                            {
                                var modelData = SoundSpeedParser.ToList(item.Value);
                                Add(new SoundSpeedModel(item.Id, item.Title, modelData));
                            }
                        }
                        */
        }


        public async Task SaveSettings(MailSettingsData settings)
        {
            var strDir = new Dictionary<AttributeItem, string>();
            var intDir = new Dictionary<AttributeItem, long>();
            var floatDir = new Dictionary<AttributeItem, double>();

            MailSettings ss = new MailSettings(settings);

            Dictionary<string, object> v1 = ClassPropertiesConverter.GetProperties2(ss);
            var propArray = ClassPropertiesConverter.GetProperties(ss);

            foreach (var prop in propArray)
            {
                if (prop.GetValue(ss) is int || prop.GetValue(ss) is uint)
                {
                    if (Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out AttributeItem attItem))
                        intDir.Add(attItem, Convert.ToInt64(prop.GetValue(ss)));
                }
                else if (prop.GetValue(ss) is float || prop.GetValue(ss) is double)
                {
                    if (Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out AttributeItem attItem))
                        floatDir.Add(attItem, Convert.ToDouble(prop.GetValue(ss)));
                }
                else if (prop.GetValue(ss) is string)
                {
                    if (Repo.AttrDir.ByTitle.TryGetValue(prop.Name, out AttributeItem attItem))
                        strDir.Add(attItem, Convert.ToString(prop.GetValue(ss)));
                }
            }
            using (var tr = BeginTransaction())
            {
                await DbService.Instance.DataInt.Save(tr, EntityKind.MailConfig, 0, intDir);
                await DbService.Instance.DataFloat.Save(tr, EntityKind.MailConfig, 0, floatDir);
                await DbService.Instance.DataString.Save(tr, EntityKind.MailConfig, 0, strDir);
            }
        }

        public Task<MailSettingsData> ReadSettings()
        {
            return Task.FromResult(_MailSettingsData);
        }

        const string table = "SoundSpeedDictionary";
        private readonly string select_all
            = $"SELECT * FROM {table}";
        private readonly string insert_with_default_id
            = $"INSERT OR REPLACE INTO {table}(Title, Value) VALUES(@Title, @Value)";

        private IDbTransaction BeginTransaction()
        {
            return DbService.Instance.Db.BeginTransaction(IsolationLevel.Serializable);
        }



    }
}
