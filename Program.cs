using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Data.SQLite;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace UmamusumeDeserializeDB5
{
    public static class UmamusumeDeserializeDB5
    {
        public static readonly string UmamusumeDatabaseFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "../LocalLow", "Cygames", "umamusume", "master", "master.mdb");
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Directory.CreateDirectory("output");
            var SingleModeStoryData = new List<SingleModeStoryData>();
            var TextData = new List<TextData>();
            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDatabaseFilePath }.ToString());
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
            var StoryTextData = TextData.Where(x => x.id == 181 && x.category == 181).ToDictionary(x => x.index, x => x);
            var NameToId = TextData.Where(x => (x.id == 4 && x.category == 4) || (x.id == 6 && x.category == 6) || (x.id == 75 && x.category == 75)).ToDictionary(x => x.text, x => x.index); //P-本名-S
            var SupportCardIdToCharaId = new Dictionary<long, long>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from single_mode_story_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var storyId = (long)reader["story_id"];
                    SingleModeStoryData.Add(new SingleModeStoryData
                    {
                        Name = StoryTextData.ContainsKey(storyId) ? StoryTextData[storyId].text : "成長のヒント",
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
                        story_id = storyId,
                        support_card_id = (long)reader["support_card_id"],
                        support_chara_id = (long)reader["support_chara_id"],
                    });
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from support_card_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SupportCardIdToCharaId.Add((long)reader["id"], (long)reader["chara_id"]);
                }
            }

            //源: https://kamigame.jp/umamusume/page/152540608660042049.html
            var kamigame = JArray.Parse(new WebClient().DownloadString("https://kamigame.jp/vls-kamigame-gametool/json/1JrYvw5XiwWeKR5c2BKVQykutI_Lj2_zauLvaWtnzvDo_411452117.json").Replace(@"\n", "[Linebreak]").Replace("[Linebreak]\"", "\""));
            var correctedEventNames = new Dictionary<string, string>();
            var correctedTriggerNames = new Dictionary<string, string>();
            if (File.Exists(@"correctedEventNames.txt"))
            {
                correctedEventNames = File.ReadAllLines(@"correctedEventNames.txt").Select(x => x.Split("【分隔符】")).ToDictionary(x => x[0], x => x[1]);
                correctedEventNames.TryAdd("きんぐちゃんとがんばる！", "キングちゃんとがんばる！");
                correctedEventNames.TryAdd("「いつもの」ください", "『いつもの』ください！");
                for (var i = 0; i < kamigame.Count; i++)
                {
                    if (correctedEventNames.TryGetValue(kamigame[i][0]!.ToString(), out var correctedEventName))
                    {
                        kamigame[i][0] = correctedEventName;
                    }
                }
            }
            if (File.Exists(@"correctedTriggerNames.txt"))
            {
                correctedTriggerNames = File.ReadAllLines(@"correctedTriggerNames.txt").Select(x => x.Split("【分隔符】")).ToDictionary(x => x[0], x => x[1]);
                for (var i = 0; i < kamigame.Count; i++)
                {
                    if (correctedTriggerNames.TryGetValue(kamigame[i][2]!.ToString(), out var correctedTriggerName))
                    {
                        kamigame[i][2] = correctedTriggerName;
                    }
                }
            }

            var events = new List<Story>();
            var failed = new List<string>();
            for (var i = 1; i < kamigame.Count; i++)
            {
                var item = kamigame[i];
                var eventName = item[0]!.ToString();
                if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(item[2]!.ToString())) continue;
                var eventCategory = item[1]!.ToString();
                var triggerName = NameToId.FirstOrDefault(x => x.Key == item[2]!.ToString()).Key;
                if (triggerName == default && !correctedTriggerNames.TryGetValue(item[2]!.ToString(), out triggerName))
                {
                    var corrected = CorrectTriggerName(item[2]!.ToString(), eventCategory == "サポートカード");
                    triggerName = corrected;
                    correctedTriggerNames.Add(item[2]!.ToString(), corrected);
                }
                var options = item[4]!.ToString().Split("[Linebreak]");
                var successEffect = item[5]!.ToString().Split("[Linebreak]");
                var failureEffect = item[6]!.ToString().Split("[Linebreak]");
                var choices = new List<Choice>();
                for (var j = 0; j < options.Length; j++)
                {
                    choices.Add(new Choice
                    {
                        Option = options[j],
                        SuccessEffect = (j + 1) > successEffect.Length ? string.Empty : successEffect[j],
                        FailedEffect = (j + 1) > failureEffect.Length ? string.Empty : failureEffect[j]
                    });
                }
                if (eventCategory == "サポートカード")
                {
                    triggerName = GetSupportCardNameByEventName(eventName);
                }
                var id = NameToId.ContainsKey(triggerName) ? NameToId[triggerName] : 0;
                //id长度大于4位即当前S卡专属的进度事件（比如北黑给金弯的那三个），否则为所有该角色S卡共有的事件（比如西野花的爱娇）
                var charaId = eventCategory == "サポートカード" ? (id.ToString().Length > 4 ? SupportCardIdToCharaId[id] : id) : (id.ToString().Length > 4 ? int.Parse(id.ToString()[..4]) : id);
                if (eventName == "あんし～ん笹針師、出☆没")
                {
                    id = 0;
                    charaId = 0;
                }
                var storyData = SingleModeStoryData.Where(x => x.Name == eventName && (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId));
                if (!storyData.Any())
                {
                    if (correctedEventNames.ContainsKey(eventName)) continue;
                    var correctedEventName = CorrectEventName(eventName, charaId, id);
                    if (eventName == correctedEventName)
                    {
                        failed.Add($"Can't Find {eventName}||{correctedEventName} For {id}||{charaId}||{triggerName} In Cy's Database!");
                        continue;
                    }
                    correctedEventNames.Add(eventName, correctedEventName);
                    Console.WriteLine($"纠正 {eventName} 为 {correctedEventName}");
                    storyData = SingleModeStoryData.Where(x => x.Name == correctedEventName && (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId));
                    if (storyData.Any())
                        AddStory(triggerName, correctedEventName, storyData, choices);
                }
                else
                {
                    AddStory(triggerName, eventName, storyData, choices);
                }
            }

            foreach (var i in failed.Distinct()) Console.WriteLine(i);
            File.WriteAllLines("failed.txt", failed.Distinct());
            File.WriteAllLines("correctedEventNames.txt", correctedEventNames.Select(x => $"{x.Key}【分隔符】{x.Value}"));
            File.WriteAllLines("correctedTriggerNames.txt", correctedTriggerNames.Select(x => $"{x.Key}【分隔符】{x.Value}"));
            new SkillDataMgr().Generate();
            new CardName().Generate();
            new ClimaxItems().Generate();
            new TalentSkillSet().Generate();
            new SuccessEvent().Generate(events);
            events = new UnknownEvents().Generate(SingleModeStoryData, events, TextData);
            File.WriteAllText("output/id.json", JsonConvert.SerializeObject(TextData.Where(x => x.id == 4).ToDictionary(x => x.index, x => x.text), Formatting.Indented));
            File.WriteAllText("output/events.json", JsonConvert.SerializeObject(events.DistinctBy(x => x.Id), Formatting.Indented));

            void AddStory(string triggerName, string eventName, IEnumerable<SingleModeStoryData> storyData, List<Choice> choices)
            {
                if (triggerName == "共通")
                {
                    foreach (var j in storyData)
                    {
                        if (events.Where(x => x.Id == j.story_id).Any())
                        {
                            return;
                        }
                        events.Add(new Story
                        {
                            Id = j.story_id,
                            Name = eventName,
                            TriggerName = triggerName,
                            Choices = choices
                        });
                    };
                }
                else
                {
                    var j = storyData.First();
                    if (events.Where(x => x.Id == j.story_id).Any())
                    {
                        return;
                    }
                    events.Add(new Story
                    {
                        Id = j.story_id,
                        Name = eventName,
                        TriggerName = triggerName,
                        Choices = choices
                    });
                }
            }
            string GetSupportCardNameByEventName(string eventName, bool corrected = false)
            {
                switch (eventName)
                {
                    //听说这个是带庸医时的扎针事件，效果和普通版完全一致，但是大部分网站都没收录
                    case "あんし～ん笹針師、出☆没":
                        return "[ブスッといっとく？]安心沢刺々美";
                }
                var data = SingleModeStoryData.First(x => x.Name == eventName);
                var actualId = Math.Max(data.support_chara_id, data.support_card_id); //这两个必定有一个是0
                return NameToId.First(x => x.Value == actualId).Key;
            }
            string CorrectEventName(string eventName, long charaId, long id)
            {
                var prompt = "NONE";
                var offset = 1;
                do
                {
                    var fragment = eventName.Length - offset;
                    var possibleNames = SingleModeStoryData
                        .Where(x => (
                            x.Name.StartsWith(eventName[..fragment]) ||
                            x.Name.EndsWith(eventName[fragment..]) ||
                            x.Name.Contains(eventName[..fragment][fragment..])) &&
                            (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId))
                        .Select(x => x.Name)
                        .ToList();
                    offset++;
                    if (possibleNames.Count == 1) return possibleNames[0];
                    if (!possibleNames.Any()) continue;
                    if (offset == eventName.Length + 1) return eventName;
                    prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title($"Select correct event name for {eventName}")
                        .PageSize(10)
                        .AddChoices(possibleNames
                            .Distinct()
                            .Where(x => x.Intersect(eventName).Count() > 2)
                            .OrderByDescending(x => x.Intersect(eventName).Count())
                            .Append("SHOW MORE")
                            .Append("NONE")
                            .Select(x => x.EscapeMarkup()))
                        );
                    if (prompt == "NONE") return eventName;
                } while (prompt == "SHOW MORE");
                return prompt.Replace("[[", "[").Replace("]]", "]");
            }
            string CorrectTriggerName(string triggerName, bool isSupportCard)
            {
                var prompt = "KEEP CURRENT";
                var offset = 1;
                do
                {
                    var fragment = triggerName.Length - offset;
                    var possibleNames = TextData.Where(x => x.id == 6 || x.id == (isSupportCard ? 75 : 4))
                        .Where(x => (
                            x.text.StartsWith(triggerName[..fragment]) ||
                            x.text.EndsWith(triggerName[fragment..]) ||
                            x.text.Contains(triggerName[..fragment][fragment..])))
                        .Select(x => x.text)
                        .ToList();
                    offset++;
                    if (possibleNames.Count == 1) return possibleNames[0];
                    if (!possibleNames.Any()) continue;
                    if (offset == triggerName.Length + 1) return triggerName;
                    prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title($"Select correct trigger name for {triggerName}")
                        .PageSize(10)
                        .AddChoices(possibleNames
                            .Distinct()
                            .Where(x => x.Intersect(triggerName).Count() > 2)
                            .OrderByDescending(x => x.Intersect(triggerName).Count())
                            .Append("SHOW MORE")
                            .Append("KEEP CURRENT")
                            .Select(x => x.EscapeMarkup()))
                        );
                    if (prompt == "KEEP CURRENT") return triggerName;
                } while (prompt == "SHOW MORE");
                return prompt.Replace("[[", "[").Replace("]]", "]"); // un-EscapeMarkup
            }
        }
    }

    class KamigameEvent
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Character { get; set; }
        public string Timing { get; set; }
        public List<Choice> Choices { get; set; }
        public string ScenarioLinkCharacter { get; set; }
    }
    public class Story
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string TriggerName { get; set; }
        public List<Choice> Choices { get; set; }

    }
    public class Choice
    {
        public string Option { get; set; }
        public string SuccessEffect { get; set; }
        public string FailedEffect { get; set; }
    }
    public class SingleModeStoryData
    {
        public string Name { get; set; } = "未找到";
        public long id { get; set; }
        public long story_id { get; set; }
        public long short_story_id { get; set; }
        public long card_id { get; set; }
        public long card_chara_id { get; set; }
        public long support_card_id { get; set; }
        public long support_chara_id { get; set; }
        public long show_progress_1 { get; set; }
        public long show_progress_2 { get; set; }
        public long show_clear { get; set; }
        public long show_succession { get; set; }
        public long event_title_style { get; set; }
        public long event_title_dress_icon { get; set; }
        public long event_title_chara_icon { get; set; }
        public long se_change { get; set; }
        public long ending_type { get; set; }
        public long race_event_flag { get; set; }
        public long mini_game_result { get; set; }
        public long gallery_main_scenario { get; set; }
        public long gallery_flag { get; set; }
        public long gallery_list_id { get; set; }
        public long past_race_id { get; set; }
    }
    public class TextData
    {
        public long id { get; set; }
        public long category { get; set; }
        public long index { get; set; }
        public string text { get; set; }
    }
}