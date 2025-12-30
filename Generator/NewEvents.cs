using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using UmamusumeResponseAnalyzer.Entities;

namespace UmamusumeDeserializeDB5.Generator
{
    internal partial class NewEvents : GeneratorBase
    {
        public static bool TrainerIsMale = true;
        public static string STORY_DATA_PATH = @"K:\repos\UmamusumeStoryDataExtractor\UmamusumeStoryDataExtractor\bin\Release\net8.0\Ext_Jp\story\data\";
        readonly string SCENARIO_EVENT_PATH = Path.Combine(STORY_DATA_PATH, "40");
        readonly string CHARACTER_EVENT_PATH = Path.Combine(STORY_DATA_PATH, "50");
        readonly string SUPPORT_CARD_R_EVENT_PATH = Path.Combine(STORY_DATA_PATH, "80");
        readonly string SUPPORT_CARD_SR_EVENT_PATH = Path.Combine(STORY_DATA_PATH, "82");
        readonly string SUPPORT_CARD_SSR_EVENT_PATH = Path.Combine(STORY_DATA_PATH, "83");
        List<Story> oldStories = [];
        Dictionary<string, string> localizedUmaNames = null!;
        Dictionary<string, string> localizedSkillNames = null!;
        Dictionary<string, string> staticTranslation = JsonConvert.DeserializeObject<Dictionary<string, string>>("{\"ウマ娘の\":\"马娘的\",\"スピード\":\"速度\",\"スタミナ\":\"耐力\",\"パワー\":\"力量\",\"根性\":\"根性\",\"賢さ\":\"智力\",\"マイナススキル\":\"负面寄能\",\"スキル\":\"技能\",\"ヒント\":\"Hint \",\"やる気\":\"干劲\",\"の絆ゲージ\":\"的羁绊\",\"ステータス\":\"属性\",\"ランダムな\":\"随机\",\"つの\":\"项\",\"〜\":\"~\",\"練習上手\":\"擅长练习\",\"愛嬌\":\"惹人怜爱\",\"切れ者\":\"能人（概率获得）\",\"直前のトレーニング能力\":\"之前训练的属性\",\"直前のトレーニングに応じた\":\"之前训练的\",\"太り気味\":\"变胖\",\"練習ベタ\":\"不擅长练习\",\"夜ふかし気味\":\"熬夜\",\"バッドコンディションが治る\":\"治疗负面状态\",\"バッドコンディションが解消\":\"解除部分负面状态\",\"確率で\":\"概率\",\"なまけ癖\":\"摸鱼癖\",\"進行イベント打ち切り\":\"事件中断\",\"アタシに指図しないで！！！\":\"别对我指指点点！\",\"スターゲージ\":\"明星量表\",\"お出かけ不可になる\":\"不能外出\",\"とお出かけできる\":\"外出解锁\",\"ようになる\":\"\",\"ポジティブ思考\":\"正向思考\",\"ファン\":\"粉丝\",\"絆が一番低いサポートカード\":\"羁绊最低的支援卡\",\"全ての\":\"全部\",\"ランダム\":\"随机\",\"」の\":\"」的\"}")!;
        [GeneratedRegex(@"「(.*?)」")]
        private static partial Regex LocalizedLineRegex();

        public async Task<List<Story>> Generate(List<Story> oldStories, List<Story> jpNewEvents = null!)
        {
            this.oldStories = oldStories;
            var a = Task.Run(() => qwq(SCENARIO_EVENT_PATH));
            var b = Task.Run(() => qwq(CHARACTER_EVENT_PATH));
            var c = Task.Run(() => qwq(SUPPORT_CARD_R_EVENT_PATH));
            var d = Task.Run(() => qwq(SUPPORT_CARD_SR_EVENT_PATH));
            var e = Task.Run(() => qwq(SUPPORT_CARD_SSR_EVENT_PATH));
            Task.WaitAll(a, b, c, d, e);
            var all = new List<Story>([.. a.Result, .. b.Result, .. c.Result, .. d.Result, .. e.Result]);
            Save($"events{(TrainerIsMale ? "_male" : "_female")}", all);

            if (jpNewEvents != null)
            {
                var jpNewEventsDictionary = jpNewEvents.ToDictionary(x => x.Id, x => x);
                foreach (var story in all)
                {
                    jpNewEventsDictionary[story.Id] = story;
                }
                foreach (var story in jpNewEventsDictionary.Values)
                {
                    foreach (var choice in story.Choices)
                    {
                        choice[0].SuccessEffect = TranslateLine(choice[0].SuccessEffect);
                        choice[0].FailedEffect = TranslateLine(choice[0].FailedEffect);
                    }
                }
                Save($"events{(TrainerIsMale ? "_male" : "_female")}", jpNewEventsDictionary.Values.ToList().OrderBy(x => x.Id));
            }
            return all;
        }
        List<Story> qwq(string path)
        {
            var sts = new ConcurrentBag<Story>();
            Parallel.ForEach(Directory.EnumerateDirectories(path), triggerDirectory =>
            {
                var triggerId = int.Parse(triggerDirectory.Replace(STORY_DATA_PATH, string.Empty).Replace("\\", string.Empty).Replace("/", string.Empty));
                // >800000 比如8|30001 第一位表示支援卡 第二位表示稀有度(0是R) 第一位往后就是S卡的ID
                // >500000 比如50|1051 第一位表示角色事件 第二位固定0 往后是角色ID
                // >400000 比如40|0001 第一位表示剧本事件 第二位固定0 往后是剧本ID 0是通用？
                var idFromPath = int.Parse(triggerId.ToString()[1..]);
                foreach (var storyPath in Directory.EnumerateFiles(triggerDirectory))
                {
                    var storyId = long.Parse(Path.GetFileNameWithoutExtension(storyPath).Replace("storytimeline_", string.Empty));
                    var triggerName = "不知道哦";
                    switch (triggerId)
                    {
                        case >= 820000:
                            triggerName = $"{Data.JP.TextData.First(x => x.category == 76 && x.index == idFromPath).text}{Data.JP.TextData.First(x => x.category == 170 && x.index == Data.JP.SupportCardData.First(x => x.id == idFromPath).chara_id).text}";
                            break;
                        case >= 800000:
                            if (idFromPath == 1)
                            {
                                triggerName = "[通用]状态事件";
                            }
                            else if (idFromPath == 1000)
                            {
                                triggerName = "[支援卡]共通事件";
                            }
                            else
                            {
                                triggerName = $"[支援卡]{Data.JP.TextData.First(x => x.category == 170 && x.index == idFromPath).text}";
                            }
                            break;
                        case >= 500000:
                            {
                                var storyData = Data.JP.SingleModeStoryData.First(x => x.story_id == storyId || x.short_story_id == storyId);
                                var cardId = storyData.card_id;
                                if (cardId == 0) // 角色通用事件
                                {
                                    cardId = storyData.card_chara_id;
                                    triggerName = cardId == 0 ? "[角色]通用事件" : $"[角色]{Data.JP.TextData.First(x => x.category == 170 && x.index == cardId).text}";
                                }
                                else
                                {
                                    triggerName = Data.JP.TextData.First(x => x.category == 4 && x.index == cardId).text;
                                }
                                break;
                            }
                        case >= 400000:
                            {
                                triggerName = $"[剧本]{Data.TranslateScenarioId(idFromPath)}";
                                break;
                            }
                    }

                    var story = JsonConvert.DeserializeObject<StoryTimeline>(File.ReadAllText(storyPath));
                    var textBlock = story?.TextBlockList.FirstOrDefault(x => x is not null && x.ChoiceDataList.Count >= 2 && x.ChoiceDataList.DistinctBy(y => y.NextBlock).Count() >= 2);
                    var oldStory = oldStories.FirstOrDefault(x => x.Id == storyId);
                    if (textBlock != null)
                    {
                        var st = new Story();
                        st.Name = story.Title;
                        st.Id = storyId;
                        st.TriggerName = triggerName;
                        var choices = textBlock.ChoiceDataList.GroupBy(x => x.NextBlock);
                        for (var i = 0; i < choices.Count(); i++)
                        {
                            var choiceGroup = choices.ElementAt(i);
                            var newChoice = new Choice();
                            if (choiceGroup.Count() == 1)
                            { // 没有性别差分
                                newChoice.Option = choiceGroup.First().Text;
                            }
                            else
                            {
                                for (var j = 0; j < choiceGroup.Count(); j++)
                                {
                                    if (TrainerIsMale)
                                    {
                                        newChoice.Option = choiceGroup.First().Text;
                                    }
                                    else
                                    {
                                        newChoice.Option = choiceGroup.Skip(1).First().Text;
                                    }
                                }
                            }
                            if (oldStory != default)
                            {
                                try
                                {
                                    newChoice.SuccessEffect = TranslateLine(oldStory.Choices[i][0].SuccessEffect);
                                    newChoice.SuccessEffectValue = oldStory.Choices[i][0].SuccessEffectValue;
                                    newChoice.FailedEffect = TranslateLine(oldStory.Choices[i][0].FailedEffect);
                                    newChoice.FailedEffectValue = oldStory.Choices[i][0].FailedEffectValue;
                                }
                                catch { }
                            }
                            st.Choices.Add(new List<Choice> { newChoice });
                        }
                        sts.Add(st);
                    }
                    else
                    {
                        var st = new Story();
                        st.Name = story.Title;
                        st.Id = storyId;
                        st.TriggerName = triggerName;
                        if (oldStory != default)
                        {
                            oldStory.Choices[0][0].SuccessEffect = TranslateLine(oldStory.Choices[0][0].SuccessEffect);
                            oldStory.Choices[0][0].FailedEffect = TranslateLine(oldStory.Choices[0][0].FailedEffect);
                            st.Choices.Add(new List<Choice> { oldStory.Choices[0][0] });
                        }
                        else
                        {
                            st.Choices.Add(new List<Choice> { new Choice() { Option = "无选项" } });
                        }
                        sts.Add(st);
                    }
                }
            });
            return sts.ToList();
        }
        string TranslateLine(string s)
        {
            if (localizedSkillNames == null || localizedUmaNames == null)
            {
                localizedUmaNames = [];
                localizedSkillNames = [];
                var jp = new Data(Data.MDB_JP_FILEPATH);
                var tw = new Data(Data.MDB_TW_FILEPATH);
                foreach (var i in jp.TextData.Where(x => x.category == 75))
                {
                    localizedUmaNames.Add(i.text, tw.TextData.FirstOrDefault(x => x.category == 75 && x.index == i.index)?.text ?? i.text);
                }
                foreach (var i in jp.TextData.Where(x => x.category == 47))
                {
                    localizedSkillNames.TryAdd(i.text, tw.TextData.FirstOrDefault(x => x.category == 47 && x.index == i.index)?.text ?? i.text);
                }
            }
            s = s.Replace("◯", "○");
            if (!Data.IsTw) return s;
            s = DictionaryReplace(s, localizedUmaNames); // 替换马娘名字
                                                         // 技能名字
            foreach (var match in LocalizedLineRegex().Matches(s).Cast<Match>())
            {
                if (match.Success)
                {
                    var skillName = match.Groups[1].Value;
                    if (localizedSkillNames.TryGetValue(skillName, out var value))
                        s = s.Replace(skillName, $"{value}");
                }
            }
            s = DictionaryReplace(s, staticTranslation);    // 替换固定文本
            return s;
            static string DictionaryReplace(string line, Dictionary<string, string> dict)
            {
                var s = new StringBuilder(line);
                foreach (var item in dict)
                    s.Replace(item.Key, item.Value);
                return s.ToString();
            }
        }
    }
    public class StoryTimeline
    {
        public string Title { get; set; } = string.Empty;
        public List<TextBlock> TextBlockList { get; set; } = [];

        public class TextBlock
        {
            public string Name { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public List<ChoiceData> ChoiceDataList { get; set; } = [];
            public List<string> ColorTextInfoList { get; set; } = [];

            public class ChoiceData
            {
                public string Text { get; set; } = string.Empty;
                public int NextBlock { get; set; }
                public int DifferenceFlag { get; set; }
                public bool IsMale => DifferenceFlag == 2;
            }
        }
    }
    public class NewStory
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TriggerName { get; set; } = string.Empty;
        public bool IsSupportCard { get; set; }
        public List<Choice> Choices { get; set; } = [];
    }
}
