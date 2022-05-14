using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class TalentSkillSet : GeneratorBase
    {
        public void Generate()
        {
            var list = new List<AvailableSkillSetTable>();

            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from available_skill_set";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new AvailableSkillSetTable
                    {
                        available_skill_set_id = (long)reader["available_skill_set_id"],
                        skill_id = (long)reader["skill_id"],
                        need_rank = (long)reader["need_rank"]
                    });
                }
            }

            var result = list.GroupBy(x => x.available_skill_set_id).ToDictionary(x => x.Key, x => x.ToArray().Select(y => new
            {
                SkillId = y.skill_id,
                Rank = y.need_rank
            }));

            Save("talentskillsets", result);
        }
    }
    public class AvailableSkillSetTable
    {
        public long available_skill_set_id;
        public long skill_id;
        public long need_rank;
    }
}
