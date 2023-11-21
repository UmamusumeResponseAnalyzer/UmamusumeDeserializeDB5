using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class TalentSkillSet : GeneratorBase
    {
        List<AvailableSkillSetTable> AvailableSkillSetTableList = new();
        List<SkillUpgradeConditionTable> SkillUpgradeConditionTables = new();
        List<SkillUpgradeDescriptionTable> SkillUpgradeDescriptionTable = new();
        Dictionary<long, string> ConditionText = new();

        Regex Proper = new Regex("＜(.*?)＞のスキルを(.*?)個以上所持する");
        Regex Specific = new Regex("「(.*?)」");
        Regex Speed = new Regex("速度が上がるスキルを(.*?)個以上所持する");
        Regex Recovery = new Regex("持久力が回復するスキルを(.*?)個以上所持する");
        Regex Acceleration = new Regex("加速力が上がるスキルを(.*?)個以上所持する");
        Regex Lane = new Regex("コース取りがうまくなるスキルを(.*?)個以上所持する");
        Regex Stat = new Regex("能力を引き出すスキルを(.*?)個以上所持する");
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
            ConditionText = Data.TextData.Where(x => x.category == 290).ToDictionary(x => x.index, x => x.text);
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
                    var upgradeSkills = conditions[j.skill_id].Select(conditionId =>
                    {
                        var detail = new TalentSkill.UpgradeDetail
                        {
                            UpgradedSkillId = j.skill_id
                        };
                        var conditions = new List<TalentSkill.UpgradeDetail.UpgradeCondition>();
                        var conditionText = ConditionText[conditionId];
                        if (conditionText.Contains('＜') && conditionText.Contains('＞'))
                        {
                            var regex = Proper.Match(conditionText);
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = TalentSkill.UpgradeDetail.UpgradeCondition.ConditionType.Proper,
                                Requirement = regex.Groups[1].Value switch
                                {
                                    "逃げ" => 1,
                                    "先行" => 2,
                                    "差し" => 3,
                                    "追込" => 4,
                                    "短距離" => 5,
                                    "マイル" => 6,
                                    "中距離" => 7,
                                    "長距離" => 8,
                                    "ダート" => 9
                                },
                                AdditionalRequirement = long.Parse(regex.Groups[2].Value)
                            });
                        }
                        else if (conditionText.Contains("を所持する"))
                        {
                            var regex = Specific.Match(conditionText).Groups[1].Value;
                            var skillId = Data.TextData.First(x => x.category == 47 && x.text == regex).index;
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = TalentSkill.UpgradeDetail.UpgradeCondition.ConditionType.Specific,
                                Requirement = skillId
                            });
                        }
                        else if (conditionText.Contains("速度が上がるスキル"))
                        {
                            var regex = Speed.Match(conditionText).Groups[1].Value;
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = TalentSkill.UpgradeDetail.UpgradeCondition.ConditionType.Speed,
                                Requirement = long.Parse(regex)
                            });
                        }
                        else if (conditionText.Contains("持久力が回復する"))
                        {
                            var regex = Recovery.Match(conditionText).Groups[1].Value;
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = TalentSkill.UpgradeDetail.UpgradeCondition.ConditionType.Recovery,
                                Requirement = long.Parse(regex)
                            });
                        }
                        else if (conditionText.Contains("加速力が上がるスキル"))
                        {
                            var regex = Acceleration.Match(conditionText).Groups[1].Value;
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = TalentSkill.UpgradeDetail.UpgradeCondition.ConditionType.Acceleration,
                                Requirement = long.Parse(regex)
                            });
                        }
                        else if (conditionText.Contains("コース取りがうまくなる"))
                        {
                            var regex = Lane.Match(conditionText).Groups[1].Value;
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = TalentSkill.UpgradeDetail.UpgradeCondition.ConditionType.Lane,
                                Requirement = long.Parse(regex)
                            });
                        }
                        else if (conditionText.Contains("能力を引き出すスキル"))
                        {
                            var regex = Stat.Match(conditionText).Groups[1].Value;
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = TalentSkill.UpgradeDetail.UpgradeCondition.ConditionType.Stat,
                                Requirement = long.Parse(regex)
                            });
                        }
                        else
                        {
                            conditions.Add(new TalentSkill.UpgradeDetail.UpgradeCondition
                            {
                                ConditionId = conditionId,
                            });
                        }
                        detail.Conditions = conditions.ToArray();
                        return detail;
                    });
                    switch (j.rank)
                    {
                        case 3:
                            i.Value[i.Value.IndexOf(i.Value.First(x => x.Rank == 3))].UpgradeSkills.Add(j.skill_id, upgradeSkills.Where(x => x != null).ToArray());
                            break;
                        case 5:
                            i.Value[i.Value.IndexOf(i.Value.First(x => x.Rank == 5))].UpgradeSkills.Add(j.skill_id, upgradeSkills.Where(x => x != null).ToArray());
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
        public Dictionary<long, UpgradeDetail[]> UpgradeSkills = new();

        public class UpgradeDetail
        {
            public long UpgradedSkillId;
            public UpgradeCondition[] Conditions;

            public class UpgradeCondition
            {
                public long ConditionId;
                public ConditionType Type;
                public long Requirement;
                public long AdditionalRequirement;

                public enum ConditionType
                {
                    None,
                    Proper,
                    Specific,
                    Speed,
                    Acceleration,
                    Recovery,
                    Lane,
                    Stat
                }
            }
        }
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
