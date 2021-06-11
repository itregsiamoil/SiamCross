using Dapper;
using SiamCross.Models.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    internal class SoundSpeedItem
    {
        public uint Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
    }
    public class SoundSpeedDir
    {
        public readonly Dictionary<string, SoundSpeedModel> DictByTitle = new Dictionary<string, SoundSpeedModel>();
        public readonly Dictionary<uint, SoundSpeedModel> DictById = new Dictionary<uint, SoundSpeedModel>();
        public readonly ObservableCollection<SoundSpeedModel> Models = new ObservableCollection<SoundSpeedModel>();

        //readonly SoundSpeedTable SoundSpeedTable = new SoundSpeedTable(DbService.Instance.Db);
        public SoundSpeedDir()
        {

        }
        public async Task InitAsync()
        {
            DictByTitle.Clear();
            DictById.Clear();
            Models.Clear();

            using (var tr = BeginTransaction())
            {
                var values = await tr.Connection.QueryAsync<SoundSpeedItem>(select_all);
                foreach (var item in values)
                {
                    var modelData = SoundSpeedFileParcer.TryToParce(item.Value);
                    Add(new SoundSpeedModel((int)item.Id, item.Title, modelData));
                }
            }
            if (!DictById.TryGetValue(0, out SoundSpeedModel _))
                await AddLocalAsync(Resource.Langepas, 0, "SiamCross.DefaultSoundSpeedResources.langepas");
            if (!DictById.TryGetValue(1, out SoundSpeedModel _))
                await AddLocalAsync(Resource.Tataria, 1, "SiamCross.DefaultSoundSpeedResources.tataria");

        }
        private async Task AddLocalAsync(string title, uint id, string file)
        {
            Assembly assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            Stream stream = assembly.GetManifestResourceStream(file);
            using (StreamReader reader = new StreamReader(stream))
            {
                string text = await reader.ReadToEndAsync();
                var modelData = SoundSpeedFileParcer.TryToParce(text);
                Add(new SoundSpeedModel((int)id, title, modelData));
            }
        }
        void Add(SoundSpeedModel model)
        {
            if (null == model)
                return;

            DictByTitle.Add(model.Name, model);
            DictById.Add((uint)model.Code, model);
            Models.Add(model);
        }
        public async Task SaveAsync(SoundSpeedModel model)
        {
            await DeleteAsync((uint)model.Code);
            await DeleteAsync(model.Name);

            using (var tr = BeginTransaction())
            {
                var item = new SoundSpeedItem()
                {
                    Id = (uint)model.Code,
                    Title = model.Name,
                    Value = model.LevelSpeedTable.ToString()
                };
                int affectedrow = await tr.Connection.ExecuteAsync(insert_with_user_id, item);
                tr.Commit();
                if (0 < affectedrow)
                    Add(model);
            }
        }
        public async Task SaveAsync(string title, uint id, string text)
        {
            var data = SoundSpeedFileParcer.TryToParce(text);
            if (null == data)
                return;
            var model = new SoundSpeedModel((int)id, title, data);
            await SaveAsync(model);
        }
        public async Task<SoundSpeedModel> DeleteAsync(string title)
        {
            if (!DictByTitle.TryGetValue(title, out SoundSpeedModel val))
                return null;
            using (var tr = BeginTransaction())
                await tr.Connection.ExecuteAsync(delete_by_id, new { Id = val.Code });
            DictByTitle.Remove(title);
            DictById.Remove((uint)val.Code);
            Models.Remove(val);
            return val;
        }
        public async Task<SoundSpeedModel> DeleteAsync(uint id)
        {
            if (!DictById.TryGetValue(id, out SoundSpeedModel val))
                return null;
            using (var tr = BeginTransaction())
                await tr.Connection.ExecuteAsync(delete_by_id, new { Id = id });
            DictByTitle.Remove(val.Name);
            DictById.Remove(id);
            Models.Remove(val);
            return val;
        }


        const string table = "SoundSpeedDictionary";
        private readonly string insert_with_default_id
            = $"INSERT OR REPLACE INTO {table}(Title, Value) VALUES(@Title, @Value)";
        private readonly string insert_with_user_id
            = $"INSERT OR REPLACE INTO {table}(Title, Id, Value) VALUES(@Title,@Id, @Value)";
        private readonly string select_by_id
            = $"SELECT Id, Title, Value FROM {table} WHERE Id=@Id";
        private readonly string select_all
            = $"SELECT Id, Title, Value  FROM {table}";
        private readonly string delete_by_id
            = $"DELETE FROM {table} WHERE Id=@Id";
        private IDbTransaction BeginTransaction()
        {
            return DbService.Instance.Db.BeginTransaction(IsolationLevel.Serializable);
        }
    }
}
