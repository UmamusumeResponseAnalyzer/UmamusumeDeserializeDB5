using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class SkillDataMgr : GeneratorBase
    {
        List<SkillDataTable> SkillList = new();
        Dictionary<long, long> SkillNeedPointTable = new();
        Dictionary<long, string> IdToName = new();
        List<SkillData> list = new List<SkillData>();

        private void PrepareDB()
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
                        precondition_1 = (string)reader["precondition_1"],
                        condition_1 = (string)reader["condition_1"],
                        precondition_2 = (string)reader["precondition_2"],
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
        }
        public void Generate()
        {
            PrepareDB();
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

                var propers = new List<SkillData.SkillProper>();
                propers.AddRange(i.precondition_1.Split("@").Select(x => ProcessCondition(x)));
                propers.AddRange(i.condition_1.Split("@").Select(x => ProcessCondition(x)));
                propers.AddRange(i.precondition_2.Split("@").Select(x => ProcessCondition(x)));
                propers.AddRange(i.condition_2.Split("@").Select(x => ProcessCondition(x)));
                if (propers.All(x =>
                   x.Ground == SkillData.SkillProper.GroundType.None
                && x.Style == SkillData.SkillProper.StyleType.None
                && x.Distance == SkillData.SkillProper.DistanceType.None
                ))
                {
                    skill.Propers = Array.Empty<SkillData.SkillProper>();
                }
                else
                {
                    skill.Propers = propers.Where(x =>
                   x.Ground != SkillData.SkillProper.GroundType.None
                || x.Style != SkillData.SkillProper.StyleType.None
                || x.Distance != SkillData.SkillProper.DistanceType.None).ToArray();
                }
                list.Add(skill);
            }

            Save("skilldata", list);

            SkillData.SkillProper ProcessCondition(string cond)
            {
                var skill = new SkillData.SkillProper();

                // 场地适性
                if (cond.Contains("ground_type==1"))
                {
                    skill.Ground = SkillData.SkillProper.GroundType.Turf;
                }
                else if (cond.Contains("ground_type==2"))
                {
                    skill.Ground = SkillData.SkillProper.GroundType.Dirt;
                }
                else
                {
                    skill.Ground = SkillData.SkillProper.GroundType.None;
                }

                // 跑法适性
                if (cond.Contains("running_style==1"))
                {
                    skill.Style = SkillData.SkillProper.StyleType.Nige;
                }
                else if (cond.Contains("running_style==2"))
                {
                    skill.Style = SkillData.SkillProper.StyleType.Senko;
                }
                else if (cond.Contains("running_style==3"))
                {
                    skill.Style = SkillData.SkillProper.StyleType.Sashi;
                }
                else if (cond.Contains("running_style==4"))
                {
                    skill.Style = SkillData.SkillProper.StyleType.Oikomi;
                }
                else
                {
                    skill.Style = SkillData.SkillProper.StyleType.None;
                }

                // 距离适性
                if (cond.Contains("distance_type==1"))
                {
                    skill.Distance = SkillData.SkillProper.DistanceType.Short;
                }
                else if (cond.Contains("distance_type==2"))
                {
                    skill.Distance = SkillData.SkillProper.DistanceType.Mile;
                }
                else if (cond.Contains("distance_type==3"))
                {
                    skill.Distance = SkillData.SkillProper.DistanceType.Middle;
                }
                else if (cond.Contains("distance_type==4"))
                {
                    skill.Distance = SkillData.SkillProper.DistanceType.Long;
                }
                else
                {
                    skill.Distance = SkillData.SkillProper.DistanceType.None;
                }

                return skill;
            }
        }
    }
    public class SkillDataTable
    {
        public long id { get; set; }
        public long rarity { get; set; }
        public long group_id { get; set; }
        public long group_rate { get; set; }
        public long grade_value { get; set; }
        public string precondition_1 { get; set; }
        public string condition_1 { get; set; }
        public string precondition_2 { get; set; }
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
        public UpgradedSkillData[] Upgraded;
        public SkillProper[] Propers;

        public class SkillProper
        {
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
    public class UpgradedSkillData : SkillData
    {
        public int Owner;
        public int Rank;
        public long[] Conditions;
    }
}
