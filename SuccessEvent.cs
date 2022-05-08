using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5
{
    public class SuccessEvent
    {
        internal static string SUCCESS_EVENT_FILEPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UmamusumeResponseAnalyzer", "successevents.json");
        public void Generate(List<Story> stories)
        {
            //加载已有事件
            var successEvent = JsonConvert.DeserializeObject<List<SuccessStory>>(new WebClient().DownloadString("https://raw.githubusercontent.com/EtherealAO/UmamusumeResponseAnalyzer/48d10e25b781bf408545bc00b4d1e051896a3ae2/successevents.json"));
            //吃饭
            foreach (var i in stories.Where(x => x.Choices.Count == 2 && x.Choices[1].SuccessEffect == "体力+30、スキルPt+10").Select(x => new SuccessStory
            {
                Name = x.Name,
                Choices = new List<SuccessChoice>
                 {
                     new SuccessChoice
                     {
                           ChoiceIndex=2,
                           SelectIndex=1,
                           Effects=new SuccessChoiceEffectDictionary
                           {
                               { 0, "体力+30、技能点+10" }
                           }
                     }
                 }
            }))
            {
                if (successEvent.FirstOrDefault(x => x.Name == i.Name) == default)
                    successEvent.Add(i);
            }
            //无选项事件且随机给不同技能hint
            successEvent.AddRange(stories.Where(x => x.Choices.Count == 1 && x.Choices[0].SuccessEffect.Contains("ヒントLv") && x.Choices[0].FailedEffect.Contains("ヒントLv")).Select(x => new SuccessStory
            {
                Name = x.Name,
                Choices = new List<SuccessChoice>
                 {
                     new SuccessChoice
                     {
                           ChoiceIndex=1,
                           SelectIndex=1,
                           Effects=new SuccessChoiceEffectDictionary
                           {
                               { 0, x.Choices[0].SuccessEffect }
                           }
                     }
                 }
            }));
            //爱娇、切者、练习上手、注目株
            for (int i = 0; i < stories.Count; i++)
            {
                var choices = stories[i].Choices
                    .Where(x => (x.SuccessEffect.Contains("愛嬌◯") || x.SuccessEffect.Contains("切れ者") || x.SuccessEffect.Contains("練習上手◯") || x.SuccessEffect.Contains("注目株"))
                && !string.IsNullOrEmpty(x.FailedEffect) && x.FailedEffect != "-").Select(x => new SuccessChoice
                {
                    ChoiceIndex = stories[i].Choices.IndexOf(x) + 1,
                    SelectIndex = 2,
                    Effects = new SuccessChoiceEffectDictionary
                    {
                        { 0, x.SuccessEffect }
                    }
                });
                if (!choices.Any() || successEvent.FirstOrDefault(x => x.Name == stories[i].Name) != default) continue;
                successEvent.Add(new SuccessStory
                {
                    Name = stories[i].Name,
                    Choices = choices.ToList()
                });
            }
            //固有
            foreach (var i in new[] { "バレンタイン", "ファン感謝祭", "クリスマス" })
                successEvent.Add(new SuccessStory
                {
                    Name = i,
                    Choices = new List<SuccessChoice>
                    {
                        new SuccessChoice
                        {
                            ChoiceIndex=1,
                            SelectIndex=2,
                            Effects=new SuccessChoiceEffectDictionary
                            {
                                { 0, stories.First(x=>x.Name==i).Choices[0].SuccessEffect }
                            }
                        }
                    }
                });

            File.WriteAllText($"output/successevents.json", JsonConvert.SerializeObject(successEvent, Formatting.Indented));
        }
    }
    public class SuccessStory
    {
        public string Name { get; set; } = string.Empty;
        public List<SuccessChoice> Choices { get; set; } = new();
    }
    public class SuccessChoice
    {
        /// <summary>
        /// 服务器下发的ChoiceIndex
        /// </summary>
        public int ChoiceIndex { get; set; }
        /// <summary>
        /// 服务器下发的SelectIndex，即第几个选项
        /// </summary>
        public int SelectIndex { get; set; }
        /// <summary>
        /// 事件效果，Key为限定的剧本ID（部分事件需要，如赛后事件），不限定剧本ID的Key统一为0
        /// </summary>
        public SuccessChoiceEffectDictionary Effects { get; set; } = new();//ScenarioId-Effect
    }
    public class SuccessChoiceEffectDictionary : Dictionary<int, string>
    {
        public new bool ContainsKey(int key) => base.ContainsKey(key) || base.ContainsKey(0);
        public new string this[int key]
        {
            get => base.ContainsKey(key) ? base[key] : base[0]; //如果有对应剧本的效果则返回，否则返回通用
            set => base[key] = value;
        }
    }
}
