using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class UnknownEvents : GeneratorBase
    {
        public List<Story> Generate(List<SingleModeStoryData> dbStories, List<Story> stories, List<TextData> textData)
        {
            var unknownEvents = dbStories.Where(x => stories.FirstOrDefault(y => x.story_id == y.Id) == default);
            foreach (var i in unknownEvents)
            {
                var story = new Story
                {
                    Name = i.Name,
                    Id = i.story_id,
                    TriggerName = i.card_chara_id == 0 ? "系统" : textData.First(x => x.id == 6 && x.category == 6 && x.index == i.card_chara_id).text,
                    Choices = new List<List<Choice>>
                    {
                        new()
                        {
                            new Choice
                            {
                                Option = "系统事件",
                                SuccessEffect = "固定效果",
                                FailedEffect = ""
                            }
                        }
                    }
                };
                var similarEvent = stories.FirstOrDefault(x => x.Name == i.Name);
                if (i.Name == "お大事に！")
                {
                    story.Choices = new List<List<Choice>>
                        {
                            new()
                            {
                                new Choice
                                {
                                    Option = "きっちり休ませる",
                                    SuccessEffect = similarEvent!.Choices[0][0].SuccessEffect,
                                    FailedEffect = similarEvent!.Choices[1][0].FailedEffect
                                }
                            },
                            new()
                            {
                                new Choice
                                {
                                    Option = "頑張らせる",
                                    SuccessEffect = similarEvent!.Choices[0][0].SuccessEffect,
                                    FailedEffect = similarEvent!.Choices[1][0].FailedEffect
                                }
                            },
                        };
                    stories.Add(story);
                    continue;
                }
                if (i.Name == "無茶は厳禁！")
                {
                    story.Choices = new List<List<Choice>>
                        {
                            new()
                            {
                                  new Choice
                                {
                                    Option = "とにかく労わる",
                                    SuccessEffect = similarEvent!.Choices[0][0].SuccessEffect,
                                    FailedEffect = similarEvent!.Choices[1][0].FailedEffect
                                }
                            },
                            new()
                            {
                                  new Choice
                                {
                                    Option = "厳しくいく！",
                                    SuccessEffect = similarEvent!.Choices[0][0].SuccessEffect,
                                    FailedEffect = similarEvent!.Choices[1][0].FailedEffect
                                }
                            },
                        };
                    stories.Add(story);
                    continue;
                }
                if (i.Name == "成長のヒント")
                {
                    if (i.story_id.ToString()[0] == '4') continue;
                    var charaId = int.Parse(i.story_id.ToString()[2..6]);
                    if (charaId == 1000)
                        story.TriggerName = "系统";
                    else
                        story.TriggerName = Data.NameToId.First(x => x.Value == charaId).Key;
                    story.IsSupportCard = true;
                }
                else if (i.Name == "想いの継承" && i.story_id.ToString()[0] != '4')
                {
                    var charaId = int.Parse(i.story_id.ToString()[2..6]);
                    story.TriggerName = Data.NameToId.First(x => x.Value == charaId).Key;
                    story.IsSupportCard = false;
                }
                if (i.gallery_main_scenario != 0)
                {
                    story.TriggerName = i.gallery_main_scenario switch
                    {
                        0 => "通用",
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
                        12 => "温泉杯",
                        _ => "新剧本"
                    };
                }
                else
                {
                    if (new long[] { 400004013, 400004014, 400004015 }.Contains(i.story_id))
                    {
                        story.TriggerName = "巅峰杯";
                    }
                    else if (new long[] { 400002116 }.Contains(i.story_id))
                    {
                        story.TriggerName = "青春杯";
                    }
                }
                stories.Add(story);
            }
            return stories;
        }
    }
}
