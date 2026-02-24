namespace UmamusumeDeserializeDB5.Generator
{
    internal class SkillDataMgr : GeneratorBase
    {
        List<SkillData> list = new List<SkillData>();

        public void Generate()
        {
            foreach (var i in Data.JP.SkillDataTable)
            {
                var skill = new SkillData
                {
                    Id = (int)i.id,
                    GroupId = (int)i.group_id,
                    Rarity = (int)i.rarity,
                    Rate = (int)i.group_rate,
                    Grade = (int)i.grade_value,
                    Name = Data.JP.IdToName[i.id],
                    Cost = Data.JP.SkillNeedPointTable.ContainsKey(i.id) ? (int)Data.JP.SkillNeedPointTable[i.id] : 0,
                    DisplayOrder = (int)i.disp_order,
                    Category = i.icon_id switch
                    {
                        // 10011 or 10012 or 10014 or 10016 白 金 紫 进化
                        > 10000 and < 20000 => SkillData.SkillCategory.Stat,
                        > 20010 and < 20020 => SkillData.SkillCategory.Speed,
                        > 20020 and < 20030 => SkillData.SkillCategory.Recovery,
                        > 20040 and < 20050 => SkillData.SkillCategory.Acceleration,
                        > 20050 and < 20060 => SkillData.SkillCategory.Lane,
                        > 20060 and < 20070 => SkillData.SkillCategory.Reaction,
                        > 20090 and < 20100 => SkillData.SkillCategory.Observation,
                        20101 or 20102 => SkillData.SkillCategory.Speed, //点火速
                        20111 or 20112 => SkillData.SkillCategory.Recovery, //点火体
                        20121 or 20122 => SkillData.SkillCategory.Acceleration, //点火力
                        20131 or 20132 => SkillData.SkillCategory.Lane, //点火智
                        20141 or 20142 => SkillData.SkillCategory.Speed, //綺羅星
                        20151 or 20152 => SkillData.SkillCategory.Speed, //夢の途中
                        20161 or 20162 => SkillData.SkillCategory.Speed, //限界の先へ
                        20171 => SkillData.SkillCategory.Acceleration, //レースの真髄・根
                        20181 => SkillData.SkillCategory.Stat, //レースの真髄・心
                        20191 or 20192 => SkillData.SkillCategory.Speed, //陽の加護
                        20201 or 20202 => SkillData.SkillCategory.Acceleration, //海の加護
                        20211 or 20212 => SkillData.SkillCategory.Speed, //想いを背負って
                        20221 or 20222 or 20231 or 20226 => SkillData.SkillCategory.Speed, // 一堆UAF进化速度技能
                        > 30000 and < 40000 => SkillData.SkillCategory.Debuff,
                        > 40000 and < 50000 => SkillData.SkillCategory.Special, //40012大逃
                        > 1000000 and < 2000000 => SkillData.SkillCategory.Special, //嘉年华bonus LoH技能
                        2010010 or 2010016 => SkillData.SkillCategory.Speed, //日本一のウマ娘
                        20241 or 20242 => SkillData.SkillCategory.Speed, // 私たちの走る道程
                        20251 or 20252 or 20246 => SkillData.SkillCategory.Speed, // 食の極意，和其他进化粉
                        20256 => SkillData.SkillCategory.Acceleration, // 曲線のグランシェフ, お待ちどおさま！,耕せ！開墾スプリント
                        20261 or 20262 => SkillData.SkillCategory.Speed, // もう少しだけ、いい景色
                        20276 => SkillData.SkillCategory.Acceleration, // モンスターマシン
                        20286 => SkillData.SkillCategory.Recovery, // リカバリーシーケンス
                        20266 => SkillData.SkillCategory.Speed, // システムオールグリーン
                        20291 or 20292 or 20296 => SkillData.SkillCategory.Speed, // 時代を変える者, 疾風より先へ, 革命の岐路, 雲上飛翔
                        20306 => SkillData.SkillCategory.Acceleration,  // 幾星霜が導く一手
                        20311 or 20312 or 20316 => SkillData.SkillCategory.Speed, // 本能の懸け橋
                        20321 or 20322 or 20326 or 20336 => SkillData.SkillCategory.Speed, // 保養が導く奇跡
                        20351 => SkillData.SkillCategory.Special, // もう一踏ん張り
#warning 未知的
                        20331 or 20332 or 20346 => SkillData.SkillCategory.Special, // 情熱と挑戦の先の栄光 スターダムを目指して！
                        _ => throw new Exception("出现了未知的icon_id: " + i.icon_id)
                        // select (select id from skill_data where icon_id=20266) as a,text from text_data where "index"=a and category=47;
                    }
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
                || x.Distance != SkillData.SkillProper.DistanceType.None)
                        .Distinct()
                        .ToArray();
                }
                list.Add(skill);
            }

            Save("skill_data", list);

            SkillData.SkillProper ProcessCondition(string conds)
            {
                var skill = new SkillData.SkillProper();

                foreach (var cond in conds.Split('&'))
                {
                    // 场地适性
                    if (cond == "ground_type==1")
                    {
                        skill.Ground = SkillData.SkillProper.GroundType.Turf;
                    }
                    else if (cond == "ground_type==2")
                    {
                        skill.Ground = SkillData.SkillProper.GroundType.Dirt;
                    }

                    // 跑法适性
                    if (cond == "running_style==1")
                    {
                        skill.Style = SkillData.SkillProper.StyleType.Nige;
                    }
                    else if (cond == "running_style==2")
                    {
                        skill.Style = SkillData.SkillProper.StyleType.Senko;
                    }
                    else if (cond == "running_style==3")
                    {
                        skill.Style = SkillData.SkillProper.StyleType.Sashi;
                    }
                    else if (cond == "running_style==4")
                    {
                        skill.Style = SkillData.SkillProper.StyleType.Oikomi;
                    }

                    // 距离适性
                    if (cond == "distance_type==1")
                    {
                        skill.Distance = SkillData.SkillProper.DistanceType.Short;
                    }
                    else if (cond == "distance_type==2")
                    {
                        skill.Distance = SkillData.SkillProper.DistanceType.Mile;
                    }
                    else if (cond == "distance_type==3")
                    {
                        skill.Distance = SkillData.SkillProper.DistanceType.Middle;
                    }
                    else if (cond == "distance_type==4")
                    {
                        skill.Distance = SkillData.SkillProper.DistanceType.Long;
                    }
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
        public long icon_id { get; set; }
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
        public SkillCategory Category;

        public class SkillProper
        {
            public GroundType Ground = GroundType.None;
            public DistanceType Distance = DistanceType.None;
            public StyleType Style = StyleType.None;

            public override bool Equals(object? obj)
            {
                if (obj is null || obj is not SkillProper proper) return false;
                return Ground == proper.Ground && Distance == proper.Distance && Style == proper.Style;
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(Ground, Distance, Style);
            }

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
        public enum SkillCategory
        {
            /// <summary>
            /// 绿
            /// </summary>
            Stat,
            /// <summary>
            /// 蓝
            /// </summary>
            Recovery,
            /// <summary>
            /// 速度
            /// </summary>
            Speed,
            /// <summary>
            /// 加速度
            /// </summary>
            Acceleration,
            /// <summary>
            /// 跑道
            /// </summary>
            Lane,
            /// <summary>
            /// 出闸
            /// </summary>
            Reaction,
            /// <summary>
            /// 视野
            /// </summary>
            Observation,
            /// <summary>
            /// 红
            /// </summary>
            Debuff,
            /// <summary>
            /// 特殊(大逃)
            /// </summary>
            Special,
            Unknown = int.MaxValue
        }
    }
    public class UpgradedSkillData : SkillData
    {
        public int Owner;
        public int Rank;
        public long[] Conditions;
    }
}
