using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class FactorIds : GeneratorBase
    {
        public void Generate()
        {
            var dic = new Dictionary<long, string>();
            foreach (var i in Data.TextData.Where(x => x.id == 147 && x.category == 147))
            {
                var stars = (int)char.GetNumericValue(i.index.ToString().Last());
                dic.Add(i.index, $"{i.text}{string.Join(string.Empty, Enumerable.Repeat("★", stars))}");
            }
            Save("factor_ids", dic);
        }
    }
}
