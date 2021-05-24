using Dapper;
using SiamCross.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SiamCross.Services
{
    public class Attributes
    {
        public readonly Dictionary<uint, AttributeItem> ById = new Dictionary<uint, AttributeItem>();
        public readonly Dictionary<string, AttributeItem> ByTitle = new Dictionary<string, AttributeItem>();

        public Attributes()
        {

        }

        public async Task<IEnumerable<AttributeItem>> LoadAsync(IDbTransaction tr)
        {
            ById.Clear();
            ByTitle.Clear();
            var values = await tr.Connection.QueryAsync<AttributeItem>(
                "SELECT * FROM Attributes");
            return values;
        }
    }
}
