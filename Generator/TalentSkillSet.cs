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
        List<AvailableSkillSetTable> AvailableSkillSetTableList = new();
        List<SkillUpgradeConditionTable> SkillUpgradeConditionTables = new();
        List<SkillUpgradeDescriptionTable> SkillUpgradeDescriptionTable = new();
        void PrepareDB()
        {
            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from available_skill_set";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    AvailableSkillSetTableList.Add(new AvailableSkillSetTable
                    {
                        available_skill_set_id = (long)reader["available_skill_set_id"],
                        skill_id = (long)reader["skill_id"],
                        need_rank = (long)reader["need_rank"]
                    });
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from skill_upgrade_condition";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SkillUpgradeConditionTables.Add(new SkillUpgradeConditionTable
                    {
                        id = (long)reader["id"],
                        description_id = (long)reader["description_id"],
                        num = (long)reader["num"],
                        sub_num = (long)reader["sub_num"],
                        timing_type = (long)reader["timing_type"],
                        count_type = (long)reader["count_type"],
                    });
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from skill_upgrade_description";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SkillUpgradeDescriptionTable.Add(new SkillUpgradeDescriptionTable
                    {
                        id = (long)reader["id"],
                        card_id = (long)reader["card_id"],
                        rank = (long)reader["rank"],
                        skill_id = (long)reader["skill_id"]
                    });
                }
            }
        }
        public void Generate()
        {
            PrepareDB();
            var conditions = SkillUpgradeConditionTables.GroupBy(x => x.description_id).ToDictionary(x => x.Key, x => x.Select(y => y.id).ToArray());
            var result = AvailableSkillSetTableList.GroupBy(x => x.available_skill_set_id).ToDictionary(x => x.Key, x => x.ToArray().Select(y => new TalentSkill
            {
                SkillId = y.skill_id,
                Rank = y.need_rank
            }).ToList());
            foreach (var i in result)
            {
                var upgraded = SkillUpgradeDescriptionTable.Where(x => x.card_id == i.Key);
                foreach (var j in upgraded)
                {
                    switch (j.rank)
                    {
                        case 3:
                            i.Value[i.Value.IndexOf(i.Value.First(x => x.Rank == 3))].UpgradeSkills.Add(j.skill_id, conditions[j.skill_id].ToArray());
                            break;
                        case 5:
                            i.Value[i.Value.IndexOf(i.Value.First(x => x.Rank == 5))].UpgradeSkills.Add(j.skill_id, conditions[j.skill_id].ToArray());
                            break;
                        default:
                            throw new Exception("出现了白技能的进化技能？");
                    }
                }
            }
            Save("talent_skill_sets", result);
        }
    }
    public class TalentSkill
    {
        public long SkillId;
        public long Rank;
        public Dictionary<long, long[]> UpgradeSkills = new();
    }
    public class AvailableSkillSetTable
    {
        public long available_skill_set_id;
        public long skill_id;
        public long need_rank;
    }
    public class SkillUpgradeConditionTable
    {
        public long id;
        public long description_id;
        public long num;
        public long sub_num;
        public long timing_type;
        public long count_type;
    }
    public class SkillUpgradeDescriptionTable
    {
        public long id;
        public long card_id;
        public long rank;
        public long skill_id;
    }
}
