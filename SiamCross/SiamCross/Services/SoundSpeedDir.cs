using SiamCross.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Helpers;

namespace SiamCross.Services
{
    public class SoundSpeedDir
    {
        readonly WeakEventManager<NotifyCollectionChangedEventArgs> _EventManager
            = new WeakEventManager<NotifyCollectionChangedEventArgs>();

        public event EventHandler<NotifyCollectionChangedEventArgs> GetNotifyCollectionChanged
        {
            add => _EventManager.AddEventHandler(value);
            remove => _EventManager.RemoveEventHandler(value);
        }

        public SoundSpeedDir()
        {
            Models.CollectionChanged += Models_CollectionChanged;
        }

        private void Models_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _EventManager.RaiseEvent(sender, e, "");
        }


        public readonly Dictionary<string, SoundSpeedModel> DictByTitle = new Dictionary<string, SoundSpeedModel>();
        public readonly Dictionary<uint, SoundSpeedModel> DictById = new Dictionary<uint, SoundSpeedModel>();
        public readonly ObservableCollection<SoundSpeedModel> Models = new ObservableCollection<SoundSpeedModel>();


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
        public async Task InitAsync()
        {
            DictByTitle.Clear();
            DictById.Clear();
            Models.Clear();
            var values = await DbService.Instance.LoadSoundSpeedAsync();
            foreach (var item in values)
            {
                var modelData = SoundSpeedFileParcer.TryToParce(item.Value);
                Add(new SoundSpeedModel((int)item.Id, item.Title, modelData));
            }

            if (!DictById.TryGetValue(0, out SoundSpeedModel _))
                await AddLocalAsync(Resource.Langepas, 0, "SiamCross.DefaultSoundSpeedResources.langepas");
            if (!DictById.TryGetValue(1, out SoundSpeedModel _))
                await AddLocalAsync(Resource.Tataria, 1, "SiamCross.DefaultSoundSpeedResources.tataria");

        }
        public async Task SaveAsync(SoundSpeedModel model)
        {
            await DeleteAsync((uint)model.Code);
            await DeleteAsync(model.Name);

            var item = await DbService.Instance
                .SaveSoundSpeedAsync(model.Name, (uint)model.Code, model.LevelSpeedTable.ToString());
            if (null == item)
                return;
            Add(model);
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
            await DbService.Instance.DelSoundSpeedAsync(val.Code);
            DictByTitle.Remove(title);
            DictById.Remove((uint)val.Code);
            Models.Remove(val);
            return val;
        }
        public async Task<SoundSpeedModel> DeleteAsync(uint id)
        {
            if (!DictById.TryGetValue(id, out SoundSpeedModel val))
                return null;
            await DbService.Instance.DelSoundSpeedAsync(id);
            DictByTitle.Remove(val.Name);
            DictById.Remove(id);
            Models.Remove(val);
            return val;
        }
    }
}
