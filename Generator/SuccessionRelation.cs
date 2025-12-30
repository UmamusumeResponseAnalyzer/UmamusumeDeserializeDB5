using System;
using System.Collections.Generic;
using System.Text;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class SuccessionRelation : GeneratorBase
    {
        public void Generate()
        {
            var bonus = new Dictionary<long, long>();
            foreach (var i in Data.JP.SuccessionRelationTable)
            {
                bonus.Add(i.relation_type, i.relation_point);
            }
            var dic = new Dictionary<long, List<long>>();
            foreach (var i in Data.JP.SuccessionRelationMemberTable)
            {
                dic.TryAdd(i.relation_type, []);
                dic[i.relation_type].Add(i.chara_id);
            }
            var data = new SuccessionRelationData()
            {
                PointDictionary = bonus,
                MemberDictionary = dic,
            };
            Save("succession_relation", data);
        }

        public class SuccessionRelationData
        {
            public Dictionary<long, long> PointDictionary { get; set; }
            public Dictionary<long, List<long>> MemberDictionary { get; set; }
        }
    }
}
