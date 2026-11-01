using System.Data.SQLite;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace UmamusumeDeserializeDB5.Generator;

internal static class SkillPlanningDataGenerator
{
    public static void Generate(string masterPath, string outputDirectory)
    {
        using var connection = new SQLiteConnection(new SQLiteConnectionStringBuilder
        { DataSource = Path.GetFullPath(masterPath), ReadOnly = true, FailIfMissing = true }.ToString());
        connection.Open();
        List<Dictionary<string, object>> Read(string query)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            using var reader = command.ExecuteReader();
            var rows = new List<Dictionary<string, object>>();
            while (reader.Read())
                rows.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
            return rows;
        }
        static int N(Dictionary<string, object> row, string name) => Convert.ToInt32(row[name]);
        static string S(Dictionary<string, object> row, string name) => Convert.ToString(row[name])!;
        var names = Read("select * from text_data where category=47").ToDictionary(x => N(x, "index"), x => S(x, "text"));
        var descriptions = Read("select * from text_data where category=290").ToDictionary(x => N(x, "index"), x => S(x, "text"));
        var costs = Read("select * from single_mode_skill_need_point").ToDictionary(x => N(x, "id"));
        var skillRows = Read("select * from skill_data");
        var conditionSlots = skillRows[0].Keys.Where(x => Regex.IsMatch(x, "^precondition_[0-9]+$"))
            .Select(x => int.Parse(x[13..])).Order().ToArray();
        var skills = skillRows.Select(row =>
        {
            var id = N(row, "id");
            var cost = costs.GetValueOrDefault(id);
            return new
            {
                Id = id, Name = names[id], GroupId = N(row, "group_id"),
                Rarity = N(row, "rarity"), Rate = N(row, "group_rate"), Grade = N(row, "grade_value"),
                Cost = cost is null ? (int?)null : N(cost, "need_skill_point"),
                StatusType = cost is null ? 0 : N(cost, "status_type"), StatusValue = cost is null ? 0 : N(cost, "status_value"),
                SolvableType = cost is null ? 0 : N(cost, "solvable_type"), DisplayOrder = N(row, "disp_order"),
                DisableCountCondition = N(row, "disable_count_condition"),
                Tags = S(row, "tag_id").Split('/').Select(int.Parse).Where(x => x != 0).ToArray(),
                EffectTags = Enumerable.Range(1, 2).SelectMany(detail => Enumerable.Range(1, 3).Select(slot =>
                    EffectTag(N(row, $"ability_type_{detail}_{slot}"), N(row, $"float_ability_value_{detail}_{slot}"), N(row, $"target_type_{detail}_{slot}"))))
                    .Where(x => x != 0).Distinct().ToArray(),
                Preconditions = conditionSlots.Select(i => S(row, $"precondition_{i}")).ToArray(),
                Conditions = conditionSlots.Select(i => S(row, $"condition_{i}")).ToArray()
            };
        }).ToArray();
        var talents = Read("select * from available_skill_set");
        object Condition(Dictionary<string, object> row, bool scenario = false)
        {
            var id = N(row, "id");
            if (scenario) return new { Id = id, Group = N(row, "num"), Kind = 0, Operand = 0, Description = "" };
            var text = descriptions[id].Trim();
            var kind = 0;
            var operand = 0;
            var proper = Regex.Match(text, "^(?:＜(?<tag>[^＞]+)＞(?:の)?スキルを[0-9]+個以上所持する|持有[0-9]+個以上＜(?<tag>[^＞]+)＞的技能)$");
            var owned = Regex.Match(text, "^(?:スキル「(?<name>[^」]+)」を所持する|持有技能「(?<name>[^」]+)」)$");
            if (owned.Success)
            {
                var matches = names.Where(x => x.Value == owned.Groups["name"].Value).ToArray();
                kind = matches.Length == 1 ? 2 : 3;
                operand = matches.Length == 1 ? matches[0].Key : 0;
            }
            else if (proper.Success)
            {
                operand = proper.Groups["tag"].Value switch
                { "逃げ" or "領頭" => 101, "先行" or "前列" => 102, "差し" or "居中" => 103, "追込" or "後追" => 104,
                  "短距離" => 201, "マイル" or "一哩" => 202, "中距離" => 203, "長距離" => 204, "ダート" or "沙地" => 502, _ => 0 };
                kind = operand == 0 ? 3 : 1;
            }
            else if (text.Contains("所持") || text.Contains("持有"))
            {
                var effect = Regex.Match(text, "^(?:(?<effect>.+)スキルを[0-9]+個以上所持する|持有[0-9]+個以上(?<effect>.+)的技能)$");
                operand = effect.Groups["effect"].Value switch
                {
                    "速度が上がる" or "使速度上升" => 6,
                    "持久力が回復する" or "回復持久力" => 13,
                    "加速力が上がる" or "使加速力上升" => 8,
                    "コース取りがうまくなる" or "變得擅長爭搶賽道" => 9,
                    "能力を引き出す" or "提升能力" => 1,
                    _ => 0
                };
                kind = operand == 0 ? 3 : 4;
            }
            return new { Id = id, Group = N(row, "num"), Kind = kind, Operand = operand, Description = text };
        }
        var conditions = Read("select * from skill_upgrade_condition").GroupBy(x => N(x, "description_id"))
            .ToDictionary(x => x.Key, x => x.Select(row => Condition(row)).ToArray());
        var evolutions = new List<object>();
        foreach (var row in Read("select * from skill_upgrade_description"))
        {
            var card = N(row, "card_id"); var rank = N(row, "rank"); var id = N(row, "skill_id");
            var talent = talents.Single(x => N(x, "available_skill_set_id") == card && N(x, "need_rank") == rank);
            evolutions.Add(new { BaseSkillId = N(talent, "skill_id"), SkillId = id, Kind = 1, CardId = card,
                Rank = rank, ScenarioId = 0, StartDate = Convert.ToInt64(row["start_date"]), Conditions = conditions[id] });
        }
        foreach (var (table, kind) in new[] { ("skill_upgrade_speciality", 2), ("skill_upgrade_succession_skill", 3) })
            foreach (var row in Read($"select * from {table}"))
            {
                var id = N(row, "skill_id"); var baseId = N(row, "base_skill_id");
                evolutions.Add(new { BaseSkillId = baseId, SkillId = id, Kind = kind,
                    CardId = kind == 3 ? N(row, "card_id") : 0, Rank = 0, ScenarioId = kind == 2 ? N(row, "scenario_id") : 0,
                    StartDate = Convert.ToInt64(row["start_date"]), Conditions = conditions[kind == 3 ? baseId : id] });
            }
        var output = new
        {
            Format = 1, Skills = skills,
            Talents = talents.Select(x => new { CardId = N(x, "available_skill_set_id"), SkillId = N(x, "skill_id"), Rank = N(x, "need_rank") }),
            Evolutions = evolutions,
            ScenarioLimits = Read("select id,skill_upgrade_num from single_mode_scenario").ToDictionary(x => N(x, "id"), x => N(x, "skill_upgrade_num")),
            ScenarioConditions = Read("select * from skill_up_scenario_condition").GroupBy(x => (Scenario: N(x, "scenario_id"), Rank: N(x, "rank")))
                .Select(x => new { ScenarioId = x.Key.Scenario, x.Key.Rank, Conditions = x.Select(row => Condition(row, scenario: true)).ToArray() }),
            HintDiscounts = new Dictionary<int, int> { [1] = 10, [2] = 20, [3] = 30, [4] = 35, [5] = 40 }
        };
        Directory.CreateDirectory(outputDirectory);
        using var stream = File.Create(Path.Combine(outputDirectory, "skill_planning.br"));
        using var compressed = new BrotliStream(stream, CompressionLevel.SmallestSize);
        using var writer = new StreamWriter(compressed, new UTF8Encoding(false));
        writer.Write(JsonConvert.SerializeObject(output));
    }

    // GameAssembly GetSkillEffectTag / IsDebuff / IsSkillDemerit。返回游戏的效果标签，不使用图标猜分类。
    static int EffectTag(int type, int masterValue, int target)
    {
        var value = masterValue * 0.0001f;
        var negative = type switch
        {
            1 or 2 or 3 or 4 or 5 or 8 or 9 or 21 or 22 or 27 or 28 or 30 or 31 or 32 or 48 => value < 0,
            10 or 39 or 40 => value > 1, 13 or 14 => true, 29 => value >= 0, 42 => value < 1, _ => false
        };
        if (negative)
            return target == 1 ? 0 : type switch { 8 => 18, 9 => 17, 13 or 29 => 16, 21 or 22 => 14, 31 => 15, _ => 0 };
        return type switch { >= 1 and <= 5 => type, 27 => 6, 21 or 22 => 7, 31 => 8, 28 => 9, 10 => 10, 8 => 11, 48 => 12, 9 => 13, _ => 0 };
    }
}
