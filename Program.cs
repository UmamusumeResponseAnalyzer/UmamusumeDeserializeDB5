using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using UmamusumeDeserializeDB5.Generator;

namespace UmamusumeDeserializeDB5
{
    public static class UmamusumeDeserializeDB5
    {
        public static readonly string UmamusumeDatabaseFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "../LocalLow", "Cygames", "umamusume", "master", "master.mdb");
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            new CardName().Generate();
            var stories = new Events().Generate();
            new SuccessEvent().Generate(stories);
            new SkillDataMgr().Generate();
            new ClimaxItems().Generate();
            new TalentSkillSet().Generate();
            new FactorIds().Generate();
            //new SupportDataGenerator().Generate();

            Story.SerializeIsSupportCard = true;
            new Generator.UmamusumeEventEditor.Events().Generate(stories, new Events().GenerateSingleModeStoryData());
            new Generator.UmamusumeEventEditor.Cards().Generate();

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var i in Directory.EnumerateFiles("./output", "*.br", SearchOption.TopDirectoryOnly))
                {
                    zip.CreateEntryFromFile(i, Path.GetFileName(i));
                }
            }
            File.WriteAllBytes(@$"./output/数据v{DateTime.Now:yyMMddHHmmss}.zip", ms.ToArray());
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
        public bool IsSupportCard { get; set; }
        public List<List<Choice>> Choices { get; set; }

        [JsonIgnore]
        public static bool SerializeIsSupportCard { get; set; } = false;
        public bool ShouldSerializeIsSupportCard()
        {
            if (SerializeIsSupportCard)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class EffectValue
    {
        public int[] Values = new int[10];  // 速耐力根智，pt，hint, 体力，羁绊，心情
        public string? BuffName { get; set; }
        public List<string> SkillNames = new List<string>();
        public List<string> Extras = new List<string>();
    }

    public class Choice
    {
        public string Option { get; set; }
        public string SuccessEffect { get; set; }
        public string FailedEffect { get; set; }

        public EffectValue? SuccessEffectValue { get; set; }
        public EffectValue? FailedEffectValue { get; set; }
        
        public static List<string> EffectTextToId = new List<string>
        {
            "スピード",
            "スタミナ",
            "パワー",
            "根性",
            "賢さ",
            "スキルPt",
            "ヒント",
            "体力",
            "絆",
            "やる気",  // 以上对应EffectValue.Values
            "全ステータス",
            "獲得",
            "ランダムな",
            "直前のトレーニング",
            "全能力",
            "解消", // 以下词条不处理，作为其他说明放在Extras里面
            "進行イベント打ち切り",   
            "トレーニングに現れるようになる",
            "-"
        };

        public static EffectValue? ParseEffectValue(string effect)
        {
            EffectValue ret = new EffectValue();
            if (effect.Length == 0) return null;    // 空事件返回Null

            foreach (string s in effect.Split("、"))
            {
                string t = s.Replace("−", "-"); // 替换减号
                int effectId = EffectTextToId.FindIndex(x => t.Contains(x));
                int effectValue = 0;
                Match m = Regex.Match(t, "[+-]\\d+");
                if (m.Success)
                {
                    try
                    {
                        effectValue = Int32.Parse(m.Value);
                    }
                    catch (FormatException e)
                    {
                        AnsiConsole.WriteLine($"非法数字：{m.Value}");
                    }
                }
                if (effectId < 0)
                {
                    AnsiConsole.WriteLine($"未知事件效果: {s}"); // 其他还有很多无效词条，或者输入不规范的（包括打针），这里就不管了
                    ret.Extras.Add(t);
                }
                else
                {
                    if (effectId < 10)
                    {
                        ret.Values[effectId] = effectValue;
                        if (t.Contains("やる気↑"))     // 特判
                            ret.Values[9] = 1;
                        if (effectId == 6)  // hint
                        {
                            Match n = Regex.Match(t, "「(.+?)」");
                            if (n.Success)
                                ret.SkillNames.Add(n.Groups[1].Value);
                        }
                    }
                    else if (effectId == 10 || effectId == 14)    // 全属性
                    {
                        for (int i = 0; i < 5; ++i)
                            ret.Values[i] = effectValue;
                    }
                    else if (effectId == 11) // 获得状态
                    {
                        Match n = Regex.Match(t, "「(.+?)」");
                        if (n.Success)
                            ret.BuffName = n.Groups[1].Value;
                    }
                    else if (effectId == 12) // 随机，转为全属性
                    {
                        Match n = Regex.Match(t, "ランダム.(\\d+)");
                        if (n.Success)
                        {
                            try
                            {
                                effectValue *= Int32.Parse(n.Groups[1].Value);
                            }
                            catch (FormatException e)
                            {
                                AnsiConsole.WriteLine($"非法数字：{m.Value}");
                            }
                            if (effectValue > 0) effectValue = Math.Max(5, effectValue);
                            if (effectValue < 0) effectValue = Math.Min(-5, effectValue);
                            for (int i = 0; i < 5; ++i)
                                ret.Values[i] += effectValue / 5;
                        }
                    }
                    else if (effectId == 13) // 上次训练的，转为全属性
                    {
                        if (effectValue > 0) effectValue = Math.Max(5, effectValue);
                        if (effectValue < 0) effectValue = Math.Min(-5, effectValue);
                        for (int i = 0; i < 5; ++i)
                            ret.Values[i] += effectValue / 5;
                    }
                    else
                    {
                        // 已知但不处理的词条放在Extras里
                        ret.Extras.Add(t);
                    }
                }
            }
           // AnsiConsole.WriteLine(effect);
           // AnsiConsole.WriteLine(JsonConvert.SerializeObject(ret));
            return ret;
        }
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