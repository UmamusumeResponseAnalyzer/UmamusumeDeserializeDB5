using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmamusumeDeserializeDB5.Generator;

namespace UmamusumeDeserializeDB5
{
    public static class Data
    {
        public static List<TextData> TextData;
        public static Dictionary<string, long> NameToId;
        public static List<SupportCardData> SupportCardData;
        public static List<AvailableSkillSetTable> AvailableSkillSetTableList = new();
        public static List<SkillUpgradeConditionTable> SkillUpgradeConditionTables = new();
        public static List<SkillUpgradeDescriptionTable> SkillUpgradeDescriptionTable = new();
        public static List<SkillUpgradeSpecialityTable> SkillUpgradeSpecialityTable = new();
        static Data()
        {
            TextData = new List<TextData>();
            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from text_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    TextData.Add(new TextData
                    {
                        category = (long)reader["category"],
                        id = (long)reader["id"],
                        index = (long)reader["index"],
                        text = (string)reader["text"]
                    });
                }
            }
            NameToId = TextData.Where(x => x.index != 9100101 && x.index != 9101101).Where(x => (x.id == 4 && x.category == 4) || (x.id == 6 && x.category == 6) || (x.id == 75 && x.category == 75)).ToDictionary(x => x.text, x => x.index);
            NameToId.Add("系统", 1000);

            SupportCardData = new();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from support_card_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var scd = new SupportCardData
                    {
                        id = (long)reader["id"],
                        chara_id = (long)reader["chara_id"],
                        rarity = (long)reader["rarity"],
                        effect_table_id = (long)reader["effect_table_id"],
                        unique_effect_id = (long)reader["unique_effect_id"],
                        command_type = (long)reader["command_type"],
                        command_id = (long)reader["command_id"],
                        support_card_type = (long)reader["support_card_type"]
                    };
                    SupportCardData.Add(scd);
                }
            }


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
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from skill_upgrade_speciality";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SkillUpgradeSpecialityTable.Add(new SkillUpgradeSpecialityTable
                    {
                        id = (long)reader["id"],
                        scenario_id = (long)reader["scenario_id"],
                        base_skill_id = (long)reader["base_skill_id"],
                        skill_id = (long)reader["skill_id"]
                    });
                }
            }
        }
    }
    public struct SupportCardData
    {
        public long id;
        public long chara_id;
        public long rarity;
        public long effect_table_id;
        public long unique_effect_id;
        public long command_type;
        public long command_id;
        public long support_card_type;
    }
    public class TalentSkill
    {
        public long SkillId;
        public long Rank;
        public Dictionary<long, UpgradeCondition[]> UpgradeSkills = new();

    }
    public class UpgradeCondition
    {
        /// <summary>
        /// 条件ID
        /// </summary>
        public long ConditionId;
        /// <summary>
        /// 条件类型
        /// </summary>
        public ConditionType Type;
        /// <summary>
        /// 条件所需内容，Type为Specific时为指定技能ID，为Proper时为指定技能适性类型，否则为所需技能数量
        /// </summary>
        public long Requirement;
        /// <summary>
        /// 条件所需额外内容，仅Type为Proper时需要，为对应适性技能的需求数量
        /// </summary>
        public long AdditionalRequirement;

        public enum ConditionType
        {
            None,
            /// <summary>
            /// 需要学习指定适性(距离、场地、跑法)的技能
            /// </summary>
            Proper,
            /// <summary>
            /// 需要学习指定技能
            /// </summary>
            Specific,
            /// <summary>
            /// 需要学习速度技能
            /// </summary>
            Speed,
            /// <summary>
            /// 需要学习加速度技能
            /// </summary>
            Acceleration,
            /// <summary>
            /// 需要学习恢复技能
            /// </summary>
            Recovery,
            /// <summary>
            /// 需要学习走位技能
            /// </summary>
            Lane,
            /// <summary>
            /// 需要学习绿技能
            /// </summary>
            Stat
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
    public class SkillUpgradeSpecialityTable
    {
        public long id;
        public long scenario_id;
        public long base_skill_id;
        public long skill_id;
    }
}
