using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Net;
using System.Text.RegularExpressions;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class Events : GeneratorBase
    {
        public List<Story> Generate()
        {
            var StoryTextData = Data.JP.TextData.Where(x => x.id == 181 && x.category == 181).ToDictionary(x => x.index, x => x);
            var SupportCardIdToCharaId = Data.JP.SupportCardData.ToDictionary(x=>x.id,x=>x.chara_id);
            var SpecialCardEvents = Data.JP.SingleModeStoryData.Where(x => x.card_id != 0).GroupBy(x => x.card_id).ToDictionary(x => x.Key, x => x.ToList()); //胜负服事件

            //源: https://kamigame.jp/umamusume/page/152540608660042049.html
            var kamigame = JArray.Parse(new WebClient().DownloadString("https://kamigame.jp/vls-kamigame-gametool/json/1JrYvw5XiwWeKR5c2BKVQykutI_Lj2_zauLvaWtnzvDo_411452117.json")
                .Replace(@"\r\n", "[Linebreak]").Replace(@"\n", "[Linebreak]").Replace("[Linebreak]\"", "\"")
                .Replace("<br>", ""));
            var correctedEventNames = new Dictionary<string, string>();
            var correctedTriggerNames = new Dictionary<string, string>();
            if (File.Exists(@"../../correctedEventNames.txt"))
            {
                correctedEventNames = File.ReadAllLines(@"../../correctedEventNames.txt").Select(x => x.Split("【分隔符】")).ToDictionary(x => x[0], x => x[1]);
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
            if (File.Exists(@"../../correctedTriggerNames.txt"))
            {
                correctedTriggerNames = File.ReadAllLines(@"../../correctedTriggerNames.txt").Select(x => x.Split("【分隔符】")).ToDictionary(x => x[0], x => x[1]);
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
                var triggerName = Data.JP.NameToId.FirstOrDefault(x => x.Key == item[2]!.ToString()).Key;
                if (eventCategory == "メインシナリオ")
                    triggerName = "剧本事件";
                if (triggerName == default && !correctedTriggerNames.TryGetValue(item[2]!.ToString(), out triggerName))
                {
                    var corrected = CorrectTriggerName(item[2]!.ToString(), eventCategory == "サポートカード");
                    triggerName = corrected;
                    Console.WriteLine(triggerName);
                    correctedTriggerNames.Add(item[2]!.ToString(), corrected);
                }
                var options = item[4]!.ToString().Split("[Linebreak]");
                var successEffects = item[5]!.ToString().Split("[Linebreak]");
                var failureEffects = item[6]!.ToString().Split("[Linebreak]");
                var choices = new List<Choice>();
                for (var j = 0; j < options.Length; j++)
                {
                    var successEffect = (j + 1) > successEffects.Length ? string.Empty : successEffects[j];
                    var failureEffect = (j + 1) > failureEffects.Length ? string.Empty : failureEffects[j];
                    failureEffect = failureEffect == "-" ? string.Empty : failureEffect;
                    if (successEffect.Contains("【G1】") || failureEffect.Contains("【G1】"))
                    {
                        successEffect = Regex.Split(successEffect, "【.+?】、").Where(x => !string.IsNullOrEmpty(x)).First();
                        if (!string.IsNullOrEmpty(failureEffect))
                        {
                            failureEffect = Regex.Split(failureEffect, "【.+?】、").Where(x => !string.IsNullOrEmpty(x)).First();
                        }
                    }

                    // July 29: 统一用语
                    // 参见Program.cs EffectTextToId
                    var replacedTerms = new Dictionary<string, string>()
                    {
                        { "体力−", "体力-" },
                        { "全ての野菜+3", "全ての野菜+40" },
                        { "切れ物", "切れ者" }, // 切れ者
                        { "のスキルLv", "のヒントLv" },
                        { "パワ+", "パワー+" },
                        { "スキル+", "スキルPt+" },
                        { "スイルPt", "スキルPt" },
                        { "スキルpt", "スキルPt" },
                        { "進行イベント終了", "進行イベント打ち切り"},
                    };
                    foreach (var key in replacedTerms.Keys)
                    {
                        if (successEffect.Contains(key) && key != "体力−")
                        {
                            AnsiConsole.MarkupLine($"[yellow]纠正 {key} -> {replacedTerms[key]}[/]");                            
                        }
                        successEffect = successEffect.Replace(key, replacedTerms[key]);
                        failureEffect = failureEffect.Replace(key, replacedTerms[key]);
                    }
                    choices.Add(new Choice
                    {
                        Option = options[j],
                        SuccessEffect = successEffect,
                        FailedEffect = failureEffect,
                        SuccessEffectValue = Choice.ParseEffectValue(successEffect),
                        FailedEffectValue = Choice.ParseEffectValue(failureEffect)
                    });
                }
                if (eventCategory == "サポートカード")
                {
                    triggerName = GetSupportCardNameByEventName(eventName);
                    if (string.IsNullOrEmpty(triggerName))
                    {
                        var correctedEventName = CorrectEventName(eventName, 0, 0);
                        correctedEventNames.Add(eventName, correctedEventName);
                        Console.WriteLine($"纠正 {eventName} 为 {correctedEventName}");
                        eventName = correctedEventName;
                        triggerName = GetSupportCardNameByEventName(eventName);
                    }
                }
                var id = Data.JP.NameToId.ContainsKey(triggerName) ? Data.JP.NameToId[triggerName] : 0;
                //id长度大于4位即当前S卡专属的进度事件（比如北黑给金弯的那三个），否则为所有该角色S卡共有的事件（比如西野花的爱娇）
                var charaId = eventCategory == "サポートカード" ? (id.ToString().Length > 4 ? SupportCardIdToCharaId[id] : id) : (id.ToString().Length > 4 ? int.Parse(id.ToString()[..4]) : id);
                if (eventName == "あんし～ん笹針師、出☆没")
                {
                    id = 0;
                    charaId = 0;
                }
                var storyData = Data.JP.SingleModeStoryData.Where(x => x.Name == eventName && (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId));
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
                    storyData = Data.JP.SingleModeStoryData.Where(x => x.Name == correctedEventName && (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId));
                    if (storyData.Any())
                        eventName = correctedEventName;
                }
                AddStory(triggerName, eventName, eventCategory, storyData, choices);
            }

            foreach (var i in failed.Distinct()) Console.WriteLine(i);
            File.WriteAllLines("../../failed.txt", failed.Distinct());
            File.WriteAllLines("../../correctedEventNames.txt", correctedEventNames.Select(x => $"{x.Key}【分隔符】{x.Value}"));
            File.WriteAllLines("../../correctedTriggerNames.txt", correctedTriggerNames.Select(x => $"{x.Key}【分隔符】{x.Value}"));
            events = new UnknownEvents().Generate(events).DistinctBy(x => x.Id).ToList();
            Console.WriteLine(events.Count);
            Save("events", events);
            return events;

            void AddStory(string triggerName, string eventName, string eventCategory, IEnumerable<SingleModeStoryData> storyData, List<Choice> choices)
            {
                if (storyData.Count() == 0) return;

                if (!Data.JP.NameToId.ContainsKey(triggerName))
                {
                    foreach (var j in storyData)
                    {
                        if (events.Where(x => x.Id == j.story_id).Any())
                        {
                            return;
                        }
                        if (j.card_id != 0)
                        {
                            triggerName = Data.JP.NameToId.First(x => x.Value == j.card_id).Key;
                        }
                        else if (j.card_chara_id != 0)
                        {
                            triggerName = Data.JP.NameToId.First(x => x.Value == j.card_chara_id).Key;
                        }
                        else if (j.support_card_id != 0)
                        {
                            triggerName = Data.JP.NameToId.First(x => x.Value == j.support_card_id).Key;
                        }
                        else if (j.support_chara_id != 0)
                        {
                            triggerName = Data.JP.NameToId.First(x => x.Value == j.support_chara_id).Key;
                        }
                        if (j.gallery_main_scenario != 0)
                        {
                            triggerName = j.gallery_main_scenario switch
                            {
                                1 => "URA",
                                2 => "青春杯",
                                3 => "偶像杯",
                                4 => "巅峰杯",
                                5 => "女神杯",
                                6 => "LArc",
                                7 => "UAF",
                                8 => "田园杯",
                                9 => "机甲杯",
                                10 => "传奇杯",
                                11 => "野人杯",
                                _ => "未知剧本"
                            };
                        }
                        events.Add(new Story
                        {
                            Id = j.story_id,
                            Name = eventName,
                            TriggerName = triggerName,
                            IsSupportCard = eventCategory == "サポートカード",
                            Choices = choices.Select(x => new List<Choice> { x }).ToList()
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
                    if (((j.card_chara_id != 0 && j.card_id == 0) || j.support_chara_id != 0) && triggerName.Contains(']'))
                    {
                        triggerName = triggerName[(triggerName.IndexOf(']') + 1)..];
                    }
                    if (SpecialCardEvents.Any(x => x.Value.Contains(j)) && !triggerName.Contains(']'))
                    {
                        triggerName = Data.JP.NameToId.First(x => x.Value == j.card_id).Key;
                    }
                    events.Add(new Story
                    {
                        Id = j.story_id,
                        Name = eventName,
                        TriggerName = triggerName,
                        IsSupportCard = eventCategory == "サポートカード",
                        Choices = choices.Select(x => new List<Choice> { x }).ToList()
                    });
                }
            }
            string GetSupportCardNameByEventName(string eventName)
            {
                switch (eventName)
                {
                    //听说这个是带庸医时的扎针事件，效果和普通版完全一致，但是大部分网站都没收录
                    case "あんし～ん笹針師、出☆没":
                        return "[ブスッといっとく？]安心沢刺々美";
                }
                var data = Data.JP.SingleModeStoryData.FirstOrDefault(x => x.Name == eventName);
                if (data == default)
                {
                    return string.Empty;
                }
                var actualId = Math.Max(data.support_chara_id, data.support_card_id); //这两个必定有一个是0
                return Data.JP.NameToId.First(x => x.Value == actualId).Key;
            }
            string CorrectEventName(string eventName, long charaId, long id)
            {
                var prompt = "NONE";
                var offset = 1;
                do
                {
                    var fragment = eventName.Length - offset;
                    var possibleNames = (charaId == 0 && id == 0) ?
                        Data.JP.SingleModeStoryData
                        .Where(x => (
                            x.Name.StartsWith(eventName[..fragment]) ||
                            x.Name.EndsWith(eventName[fragment..]) ||
                            x.Name.Contains(eventName[..fragment][fragment..])) &&
                            (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId))
                        .Select(x => x.Name)
                        .ToList() :
                        Data.JP.SingleModeStoryData
                        .Where(x => (
                            x.Name.StartsWith(eventName[..fragment]) ||
                            x.Name.EndsWith(eventName[fragment..]) ||
                            x.Name.Contains(eventName[..fragment][fragment..])))
                        .Select(x => x.Name)
                        .ToList();
                    offset++;
                    if (possibleNames.Count == 1) return possibleNames[0];
                    if (!possibleNames.Any()) continue;
                    if (offset == eventName.Length + 1) return eventName;
                    prompt = AnsiConsole.Prompt(new SelectionPrompt<string>()
                        .Title($"Select correct event name for {eventName.EscapeMarkup()}({Data.JP.NameToId.FirstOrDefault(x => x.Value == charaId).Key})")
                        .PageSize(20)
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
                    var possibleNames = Data.JP.TextData.Where(x => x.id == 6 || x.id == (isSupportCard ? 75 : 4))
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
                        .Title($"Select correct trigger name for {triggerName} ({isSupportCard})")
                        .PageSize(20)
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
}
