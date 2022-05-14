using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class SuccessEvent : GeneratorBase
    {
        internal static string SUCCESS_EVENT_FILEPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UmamusumeResponseAnalyzer", "successevents.json");
        public void Generate(List<Story> stories)
        {
            //加载已有事件
            //var successEvent = JsonConvert.DeserializeObject<List<SuccessStory>>(new WebClient().DownloadString("https://raw.githubusercontent.com/EtherealAO/UmamusumeResponseAnalyzer/48d10e25b781bf408545bc00b4d1e051896a3ae2/successevents.json"))!;
            var successEvent = new List<SuccessStory>();
            #region 吃饭
            foreach (var i in stories.Where(x => x.Choices.Count == 2 && x.Choices[1].SuccessEffect == "体力+30、スキルPt+10").Select(x => new SuccessStory
            {
                Id = x.Id,
                Choices = new[]
                {
                    Array.Empty<SuccessChoice>(),
                    new[]
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 1,
                            Scenario=0,
                            State=1,
                            Effect = "体力+30、技能点+10"
                        }
                    }
                }
            }))
            {
                if (!successEvent.Any(x => x.Id == i.Id))
                    successEvent.Add(i);
            }
            #endregion
            #region 无选项事件且随机给不同技能hint
            successEvent.AddRange(stories.Where(x => x.Choices.Count == 1 && x.Choices[0].SuccessEffect.Contains("ヒントLv") && x.Choices[0].FailedEffect.Contains("ヒントLv")).Select(x => new SuccessStory
            {
                Id = x.Id,
                Choices = new[]
                 {
                     new[]
                     {
                         new SuccessChoice
                         {
                           SelectIndex=1,
                           Scenario=0,
                           State=1,
                           Effect= x.Choices[0].SuccessEffect
                         }
                     }
                 }
            }));
            #endregion
            #region 爱娇、切者、练习上手、注目株
            for (int i = 0; i < stories.Count; i++)
            {
                var choices = stories[i].Choices
                    .Where(x => (
                        x.SuccessEffect.Contains("愛嬌◯") ||
                        x.SuccessEffect.Contains("切れ者") ||
                        x.SuccessEffect.Contains("練習上手◯") ||
                        x.SuccessEffect.Contains("注目株")) &&
                        !string.IsNullOrEmpty(x.FailedEffect))
                    .Select(x => (stories[i].Choices.IndexOf(x), new SuccessChoice
                    {
                        SelectIndex = 2,
                        Scenario = 0,
                        State = 1,
                        Effect = x.SuccessEffect
                    }));
                if (!choices.Any() || successEvent.Any(x => x.Id == stories[i].Id) != default) continue;
                var successStory = new SuccessStory
                {
                    Id = stories[i].Id
                };
                var choiceArray = new List<List<SuccessChoice>>();
                foreach (var j in choices)
                {
                    if (choiceArray.ElementAtOrDefault(j.Item1) == default)
                        choiceArray.AddRange(Enumerable.Range(0, j.Item1 + 1).Select(x => new List<SuccessChoice>()));
                    choiceArray[j.Item1].Add(j.Item2);
                }
                successStory.Choices = choiceArray.Select(x => x.ToArray()).ToArray();
                successEvent.Add(successStory);
            }
            #endregion
            #region 固有
            foreach (var i in stories.Where(x => new[] { "バレンタイン", "ファン感謝祭", "クリスマス" }.Contains(x.Name)))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new[]
                    {
                        new[]
                        {
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=1,
                                Effect=stories.First(x=>x.Id==i.Id).Choices[0].SuccessEffect
                            }
                        }
                    }
                });
            }
            #endregion
            #region 赛后
            foreach (var i in stories.Where(x => x.Name.Contains("レース勝利") && x.Choices[0].SuccessEffect.Contains("体力-")))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new[]
                    {
                        new[]
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=4,
                                State=int.MaxValue,
                                Effect="体力-20"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=4,
                                State=int.MaxValue,
                                Effect="体力-20"
                            },
                        },
                        new[]
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=4,
                                State=1,
                                Effect="体力-10"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=4,
                                State=1,
                                Effect="体力-10"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=3,
                                Scenario=4,
                                State=0,
                                Effect="体力-25"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=4,
                                Scenario=4,
                                State=0,
                                Effect="体力-25"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=2,
                                State=1,
                                Effect="体力-5"
                            },
                        }
                    }
                });
            }
            foreach (var i in stories.Where(x => x.Name.Contains("レース入着") && x.Choices[0].SuccessEffect.Contains("体力-")))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new[]
                    {
                        new[]
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=4,
                                State=int.MaxValue,
                                Effect="体力-25"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=4,
                                State=int.MaxValue,
                                Effect="体力-25"
                            },
                        },
                        new[]
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=4,
                                State=1,
                                Effect="体力-15"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=4,
                                State=1,
                                Effect="体力-15"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=3,
                                Scenario=4,
                                State=0,
                                Effect="体力-35"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=4,
                                Scenario=4,
                                State=0,
                                Effect="体力-35"
                            },
                        }
                    }
                });
            }
            #endregion
            #region ライバルに勝利
            successEvent.Add(new SuccessStory
            {
                Id = stories.First(x => x.Name == "ライバルに勝利！").Id,
                Choices = new[]
                {
                    new[]
                    {
                        new SuccessChoice
                        {
                            SelectIndex=1,
                            Scenario=4,
                            State=int.MaxValue,
                            Effect="获得随机技能Hint"
                        },
                        new SuccessChoice
                        {
                            SelectIndex=2,
                            Scenario=4,
                            State=int.MaxValue,
                            Effect="随机两项属性+5"
                        }
                    }
                }
            });
            #endregion

            Save("successevents", successEvent);
        }
    }
    public class SuccessStory
    {
        public long Id { get; set; }
        public SuccessChoice[][] Choices { get; set; }
    }
    public class SuccessChoice
    {
        public int SelectIndex { get; set; }
        public int Scenario { get; set; }
        public int State { get; set; }
        public string Effect { get; set; } = string.Empty;
    }
}
