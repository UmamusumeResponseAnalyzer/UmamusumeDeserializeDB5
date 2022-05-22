﻿using System;
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
                                Option = "未知选项",
                                SuccessEffect = "未知效果",
                                FailedEffect = "未知效果"
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
                stories.Add(story);
            }
            return stories;
        }
    }
}
