using System.Data.SQLite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UmamusumeDeserializeDB5.Generator;

namespace UmamusumeDeserializeDB5
{
    public class DataConfig
    {
        public const string DEFAULT_PATH = "appsettings.default.json";
        public const string LOCAL_PATH = "appsettings.json";

        public string JpDbPath { get; set; } = string.Empty;
        public string TwDbPath { get; set; } = string.Empty;
        public bool UseTw { get; set; } = false;
        public string StoryDataPath { get; set; } = string.Empty;

        public static DataConfig Load()
        {
            if (!File.Exists(DEFAULT_PATH))
                throw new FileNotFoundException($"未找到默认配置文件: {DEFAULT_PATH}", DEFAULT_PATH);

            // 默认配置作为基础，本地配置按字段覆盖（深度合并）
            var defaultJson = JObject.Parse(File.ReadAllText(DEFAULT_PATH));
            if (File.Exists(LOCAL_PATH))
            {
                var localJson = JObject.Parse(File.ReadAllText(LOCAL_PATH));
                defaultJson.Merge(localJson, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    PropertyNameComparison = StringComparison.OrdinalIgnoreCase
                });
            }
            var config = defaultJson.ToObject<DataConfig>()
                ?? throw new InvalidOperationException("配置文件解析失败");

            // 校验配置中引用的文件路径（一次收集所有错误，避免用户多次试错）
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(config.JpDbPath))
                missing.Add("JpDbPath 未配置");
            else if (!File.Exists(config.JpDbPath))
                missing.Add($"JpDbPath 指定的文件不存在: {config.JpDbPath}");

            if (config.UseTw)
            {
                if (string.IsNullOrWhiteSpace(config.TwDbPath))
                    missing.Add("TwDbPath 未配置 (UseTw = true)");
                else if (!File.Exists(config.TwDbPath))
                    missing.Add($"TwDbPath 指定的文件不存在: {config.TwDbPath}");
            }

            if (string.IsNullOrWhiteSpace(config.StoryDataPath))
                missing.Add("StoryDataPath 未配置");
            else if (!Directory.Exists(config.StoryDataPath))
                missing.Add($"StoryDataPath 指定的目录不存在: {config.StoryDataPath}");

            if (missing.Count > 0)
                throw new InvalidOperationException(
                    "配置文件校验失败：\n  - " + string.Join("\n  - ", missing) +
                    $"\n请修改 {LOCAL_PATH} (字段会被合并到 {DEFAULT_PATH} 之上)");

            return config;
        }
    }

    public class Data
    {
        public static DataConfig Config = null!;

        public static string MDB_JP_FILEPATH => Config.JpDbPath;
        public static string MDB_TW_FILEPATH => Config.TwDbPath;

        public static Data JP = null!;
        public static Data TW = null!;

        public static bool IsTw => Config.UseTw;

        public static void Initialize()
        {
            Config = DataConfig.Load();
            JP = new(MDB_JP_FILEPATH);
            TW = Config.UseTw ? new(MDB_TW_FILEPATH) : null!;
        }

        public List<TextData> TextData;
        public Dictionary<long, string> IdToName = [];
        public Dictionary<string, long> NameToId;
        public List<SupportCardData> SupportCardData;
        public List<AvailableSkillSetTable> AvailableSkillSetTableList = new();
        public List<SkillUpgradeConditionTable> SkillUpgradeConditionTables = new();
        public List<SkillUpgradeDescriptionTable> SkillUpgradeDescriptionTable = new();
        public List<SkillUpgradeSpecialityTable> SkillUpgradeSpecialityTable = new();
        public List<SkillDataTable> SkillDataTable = [];
        public Dictionary<long, long> SkillNeedPointTable = [];
        public List<SingleModeStoryData> SingleModeStoryData = new();
        public List<SuccessionRelationTable> SuccessionRelationTable = [];
        public List<SuccessionRelationMemberTable> SuccessionRelationMemberTable = [];
        public List<SingleModeWinsSaddleTable> SingleModeWinsSaddleTable = [];

        public Data(string mdbPath)
        {
            if (!File.Exists(mdbPath))
                throw new FileNotFoundException($"未找到 master.mdb: {mdbPath}", mdbPath);

            TextData = new List<TextData>();
            SQLiteConnection conn;
            try
            {
                conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = mdbPath }.ToString());
                conn.Open();
            }
            catch (Exception e)
            {
                var detail = e.InnerException != null ? $" -> {e.InnerException.GetType().Name}: {e.InnerException.Message}" : "";
                throw new InvalidOperationException($"打开数据库失败: {mdbPath} ({e.GetType().Name}: {e.Message}{detail})", e);
            }
            using (conn)
            {
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
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"select * from text_data where id=47";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        IdToName.Add((long)reader["index"], (string)reader["text"]);
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
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"select * from skill_data where tag_id!='0'";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SkillDataTable.Add(new SkillDataTable
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
                            disp_order = (long)reader["disp_order"],
                            icon_id = (long)reader["icon_id"]
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
                    var StoryTextData = TextData.Where(x => x.id == 181 && x.category == 181).ToDictionary(x => x.index, x => x);
                    cmd.CommandText = $"select * from single_mode_story_data";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var data = new SingleModeStoryData
                        {
                            id = (long)reader["id"],
                            card_chara_id = (long)reader["card_chara_id"],
                            card_id = (long)reader["card_id"],
                            ending_type = (long)reader["ending_type"],
                            event_title_chara_icon = (long)reader["event_title_chara_icon"],
                            event_title_dress_icon = (long)reader["event_title_dress_icon"],
                            event_title_style = (long)reader["event_title_style"],
                            gallery_flag = (long)reader["gallery_flag"],
                            gallery_list_id = (long)reader["gallery_list_id"],
                            gallery_main_scenario = (long)reader["gallery_main_scenario"],
                            mini_game_result = (long)reader["mini_game_result"],
                            past_race_id = (long)reader["past_race_id"],
                            race_event_flag = (long)reader["race_event_flag"],
                            se_change = (long)reader["se_change"],
                            short_story_id = (long)reader["short_story_id"],
                            show_clear = (long)reader["show_clear"],
                            show_progress_1 = (long)reader["show_progress_1"],
                            show_progress_2 = (long)reader["show_progress_2"],
                            show_succession = (long)reader["show_succession"],
                            story_id = (long)reader["story_id"],
                            support_card_id = (long)reader["support_card_id"],
                            support_chara_id = (long)reader["support_chara_id"],
                        };
                        data.Name = StoryTextData.ContainsKey(data.story_id) ? StoryTextData[data.story_id].text : "成長のヒント";
                        SingleModeStoryData.Add(data);
                        if (data.short_story_id != 0)
                        {
                            var shorted = new SingleModeStoryData
                            {
                                id = (long)reader["id"],
                                card_chara_id = (long)reader["card_chara_id"],
                                card_id = (long)reader["card_id"],
                                ending_type = (long)reader["ending_type"],
                                event_title_chara_icon = (long)reader["event_title_chara_icon"],
                                event_title_dress_icon = (long)reader["event_title_dress_icon"],
                                event_title_style = (long)reader["event_title_style"],
                                gallery_flag = (long)reader["gallery_flag"],
                                gallery_list_id = (long)reader["gallery_list_id"],
                                gallery_main_scenario = (long)reader["gallery_main_scenario"],
                                mini_game_result = (long)reader["mini_game_result"],
                                past_race_id = (long)reader["past_race_id"],
                                race_event_flag = (long)reader["race_event_flag"],
                                se_change = (long)reader["se_change"],
                                short_story_id = (long)reader["short_story_id"],
                                show_clear = (long)reader["show_clear"],
                                show_progress_1 = (long)reader["show_progress_1"],
                                show_progress_2 = (long)reader["show_progress_2"],
                                show_succession = (long)reader["show_succession"],
                                story_id = (long)reader["story_id"],
                                support_card_id = (long)reader["support_card_id"],
                                support_chara_id = (long)reader["support_chara_id"],
                            };
                            SingleModeStoryData.Add(shorted);
                        }
                    }
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"select * from succession_relation";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SuccessionRelationTable.Add(new SuccessionRelationTable { relation_type = (long)reader["relation_type"], relation_point = (long)reader["relation_point"] });
                    }
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"select * from succession_relation_member";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SuccessionRelationMemberTable.Add(new SuccessionRelationMemberTable { id = (long)reader["id"], relation_type = (long)reader["relation_type"], chara_id = (long)reader["chara_id"] });
                    }
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"select * from single_mode_wins_saddle";
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SingleModeWinsSaddleTable.Add(new SingleModeWinsSaddleTable { id = (long)reader["id"], win_saddle_type = (long)reader["win_saddle_type"] });
                    }
                }
            }
        }
        public static string TranslateScenarioId(int scenario_id) => scenario_id switch
        {
            0 => "通用",
            1 => "U.R.A",
            2 => "青春杯",
            3 => "偶像杯",
            4 => "巅峰杯",
            5 => "女神杯",
            6 => "L.Arc",
            7 => "U.A.F",
            8 => "田园杯",
            9 => "机甲杯",
            10 => "传奇杯",
            11 => "野人杯",
            12 => "温泉杯",
            _ => "新剧本"
        };
        public static void UseTw()
        {
            /*IsTw = true;
            foreach (var textData in TW.TextData.Where(x => x.category != 290 && x.category != 47)) // 不需要改技能进化的条件
            {
                var jp = JP.TextData.FirstOrDefault(x => x.index == textData.index && x.category == textData.category);
                jp?.text = textData.text;
            }

            foreach (var idToName in TW.IdToName)
            {
                JP.IdToName[idToName.Key] = idToName.Value;
            }
            */
            JP.NameToId = JP.TextData.Where(x => x.index != 9100101 && x.index != 9101101).Where(x => (x.id == 4 && x.category == 4) || (x.id == 6 && x.category == 6) || (x.id == 75 && x.category == 75)).ToDictionary(x => x.text, x => x.index);
            JP.NameToId.Add("系统", 1000);

           // NewEvents.STORY_DATA_PATH = @"F:\UmaAI\UMA-Unpack\story\story\data\";
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
        /// 数据库中的num，同组条件取或，不同组取且
        /// </summary>
        public long Group;
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
            /// <summary>
            /// 仅由服务器判定，不做本地预测
            /// </summary>
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
    public class SuccessionRelationTable
    {
        public long relation_type;
        public long relation_point;
    }
    public class SuccessionRelationMemberTable
    {
        public long id;
        public long relation_type;
        public long chara_id;
    }
    public class SingleModeWinsSaddleTable
    {
        public long id;
        public long win_saddle_type;
    }
}
