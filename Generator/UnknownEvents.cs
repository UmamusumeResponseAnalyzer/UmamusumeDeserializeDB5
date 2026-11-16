namespace UmamusumeDeserializeDB5.Generator
{
    internal class UnknownEvents : GeneratorBase
    {
        public List<Story> Generate(List<Story> stories)
        {
            var unknownEvents = Data.JP.SingleModeStoryData.Where(x => stories.FirstOrDefault(y => x.story_id == y.Id) == default);
            foreach (var i in unknownEvents)
            {
                var story = new Story
                {
                    Name = i.Name,
                    Id = i.story_id,
                    TriggerName = i.card_chara_id == 0 ? "系统" : Data.JP.TextData.First(x => x.id == 6 && x.category == 6 && x.index == i.card_chara_id).text,
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
                        story.TriggerName = Data.JP.NameToId.First(x => x.Value == charaId).Key;
                    story.IsSupportCard = true;
                }
                else if (i.Name == "想いの継承" && i.story_id.ToString()[0] != '4')
                {
                    var charaId = int.Parse(i.story_id.ToString()[2..6]);
                    story.TriggerName = Data.JP.NameToId.First(x => x.Value == charaId).Key;
                    story.IsSupportCard = false;
                }
                else if (i.story_id is 820102001 or 820102002)
                {
                    // SR ステイゴールド连续事件：https://gamewith.jp/uma-musume/article/show/587274
                    var choices = i.story_id == 820102001
                        ? new (string Option, string Effect)[]
                        {
                            ("触らぬ神になんとやらだね……", "パワー+15、絆ゲージ+5"),
                            ("藪、つついてみる？", "体力+10、やる気+1、絆ゲージ+5")
                        }
                        : [
                            ("あまのじゃく的な……？", "体力+10、パワー+10、「間隙を突いて」のヒントLv+1、絆ゲージ+5"),
                            ("一生懸命さが響いたのかも", "体力+20、スキルPt+15、絆ゲージ+5")
                        ];
                    story.TriggerName = Data.JP.NameToId.First(x => x.Value == i.support_card_id).Key;
                    story.IsSupportCard = true;
                    story.Choices = choices.Select(choice => new List<Choice>
                    {
                        new()
                        {
                            Option = choice.Option,
                            SuccessEffect = choice.Effect,
                            SuccessEffectValue = Choice.ParseEffectValue(choice.Effect)
                        }
                    }).ToList();
                }
                if (i.gallery_main_scenario != 0)
                {
                    story.TriggerName = i.gallery_main_scenario switch
                    {
                        1 => "URA",
                        6 => "LArc",
                        7 => "UAF",
                        _ => Data.TranslateScenarioId(i.gallery_main_scenario)
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
