using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5
{
    public class SkillDataMgr
    {
        List<SkillDataTable> SkillList = new();
        Dictionary<long, long> SkillNeedPointTable = new();
        Dictionary<long, string> IdToName = new();
        List<SkillData> list = new List<SkillData>();

        public void Generate()
        {
            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from skill_data where tag_id!='0'";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SkillList.Add(new SkillDataTable
                    {
                        id = (long)reader["id"],
                        rarity = (long)reader["rarity"],
                        group_id = (long)reader["group_id"],
                        group_rate = (long)reader["group_rate"],
                        grade_value = (long)reader["grade_value"],
                        condition_1 = (string)reader["condition_1"],
                        condition_2 = (string)reader["condition_2"],
                        disp_order = (long)reader["disp_order"]
                    });
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from single_mode_skill_need_point";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SkillNeedPointTable.Add((long)reader["id"], (long)reader["need_skill_point"]);
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from text_data where id=47";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IdToName.Add((long)reader["index"], (string)reader["text"]);
                }
            }
            foreach (var i in SkillList)
            {
                var skill = new SkillData
                {
                    Id = (int)i.id,
                    GroupId = (int)i.group_id,
                    Rarity = (int)i.rarity,
                    Rate = (int)i.group_rate,
                    Grade = (int)i.grade_value,
                    Name = IdToName[i.id],
                    Cost = SkillNeedPointTable.ContainsKey(i.id) ? (int)SkillNeedPointTable[i.id] : 0,
                    DisplayOrder = (int)i.disp_order
                };
                if (i.condition_1.Contains("ground_type==1") || i.condition_2.Contains("ground_type==1"))
                {
                    skill.Ground = SkillData.GroundType.Turf;
                }
                else if (i.condition_1.Contains("ground_type==2") || i.condition_2.Contains("ground_type==2"))
                {
                    skill.Ground = SkillData.GroundType.Dirt;
                }
                else
                {
                    skill.Ground = SkillData.GroundType.None;
                }

                if (i.condition_1.Contains("running_style==1") || i.condition_2.Contains("running_style==1"))
                {
                    skill.Style = SkillData.StyleType.Nige;
                }
                else if (i.condition_1.Contains("running_style==2") || i.condition_2.Contains("running_style==2"))
                {
                    skill.Style = SkillData.StyleType.Senko;
                }
                else if (i.condition_1.Contains("running_style==3") || i.condition_2.Contains("running_style==3"))
                {
                    skill.Style = SkillData.StyleType.Sashi;
                }
                else if (i.condition_1.Contains("running_style==4") || i.condition_2.Contains("running_style==4"))
                {
                    skill.Style = SkillData.StyleType.Oikomi;
                }
                else
                {
                    skill.Style = SkillData.StyleType.None;
                }

                if (i.condition_1.Contains("distance_type==1") || i.condition_2.Contains("distance_type==1"))
                {
                    skill.Distance = SkillData.DistanceType.Short;
                }
                else if (i.condition_1.Contains("distance_type==2") || i.condition_2.Contains("distance_type==2"))
                {
                    skill.Distance = SkillData.DistanceType.Mile;
                }
                else if (i.condition_1.Contains("distance_type==3") || i.condition_2.Contains("distance_type==3"))
                {
                    skill.Distance = SkillData.DistanceType.Middle;
                }
                else if (i.condition_1.Contains("distance_type==4") || i.condition_2.Contains("distance_type==4"))
                {
                    skill.Distance = SkillData.DistanceType.Long;
                }
                else
                {
                    skill.Distance = SkillData.DistanceType.None;
                }
                list.Add(skill);
            }
            File.WriteAllText(@"output/skilldata.json", JsonConvert.SerializeObject(list, Formatting.Indented));
        }
    }
    public class SkillDataTable
    {
        public long id { get; set; }
        public long rarity { get; set; }
        public long group_id { get; set; }
        public long group_rate { get; set; }
        public long grade_value { get; set; }
        public string condition_1 { get; set; }
        public string condition_2 { get; set; }
        public long disp_order { get; set; }
    }
    public class SkillNeedPointTable
    {
        public long id { get; set; }
        public long need_skill_point { get; set; }
    }
    public class SkillData
    {
        public string Name;
        public int Id;
        public int GroupId;
        public int Rarity;
        public int Rate;
        public int Grade;
        public int Cost;
        public int DisplayOrder;
        public GroundType Ground;
        public DistanceType Distance;
        public StyleType Style;

        public enum GroundType
        {
            None,
            Turf,
            Dirt
        }
        public enum DistanceType
        {
            None,
            Short,
            Mile,
            Middle,
            Long
        }
        public enum StyleType
        {
            None,
            Nige,
            Senko,
            Sashi,
            Oikomi
        }
    }
}
