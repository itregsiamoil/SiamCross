using SiamCross.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class AttrDir
    {
        public readonly Dictionary<uint, AttributeItem> ById = new Dictionary<uint, AttributeItem>();
        public readonly Dictionary<string, AttributeItem> ByTitle = new Dictionary<string, AttributeItem>();

        public async Task InitAsync()
        {
            ById.Clear();
            ByTitle.Clear();
            var values = await DbService.Instance.LoadAttributesAsync();
            foreach (var v in values)
            {
                ById.Add(v.Id, v);
                ByTitle.Add(v.Title, v);
            }
        }
    }
}
