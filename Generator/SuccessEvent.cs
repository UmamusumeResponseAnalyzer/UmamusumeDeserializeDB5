﻿using Newtonsoft.Json;
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
            //后添加的 不会 覆盖之前添加的，所以需要把模糊匹配的事件放在最后
            #region 吃饭
            foreach (var i in stories.Where(x => x.Choices.Count == 2
                        && x.Choices[1].Any(x => x.SuccessEffect == "体力+30、スキルPt+10" || x.SuccessEffect == "体力+30、技能Pt+10")
                        ).Select(x => new SuccessStory
            {
                Id = x.Id,
                // 返回值，剧本，对应颜色（0红1蓝2黄），提示文字
                Choices = LoadChoicesJson("""
                    [
                      [],
                      [
                        { "SelectIndex": 1, "Scenario": 0, "State": 1, "Effect": "体力+30、技能点+10" },
                        { "SelectIndex": 2, "Scenario": 0, "State": 0, "Effect": "体力+30、技能点+10、速度-5、力量-5、『变胖』" }
                      ]
                    ]
                    """)
            }))
            {
                if (!successEvent.Any(x => x.Id == i.Id))
                    successEvent.Add(i);
            }
            #endregion
            #region 三选项特殊吃饭
            successEvent.Add(new SuccessStory
            {
                Id = 501001524,
                Choices = LoadChoicesJson("""
                    [
                      [
                        { "SelectIndex": 1, "Scenario": 0, "State": 1, "Effect": "体力+30、力量+10、技能Pt+10" },
                        { "SelectIndex": 2, "Scenario": 0, "State": 0, "Effect": "体力+30、速度−5、力量+15、技能Pt+10、获得「变胖」" }
                      ],
                      [],
                      [
                        { "SelectIndex": 1, "Scenario": 0, "State": 1, "Effect": "体力全回復" },
                        { "SelectIndex": 2, "Scenario": 0, "State": 0, "Effect": "体力全回復、速度−5、获得「变胖」" }
                      ]
                    ]
                    """)
            });//特别周，修正kamigame
            //三选项特殊吃饭通用匹配，选项1、3有失败率，selectIndex为1时成功
            foreach (var i in stories.Where(x => x.Choices.Count == 3 && x.Choices[0].Any(x => x.FailedEffect.Contains("体力+30")) && x.Choices[0].Any(x => x.FailedEffect.Contains("「太り気味」獲得"))).Select(x => new SuccessStory
            {
                Id = x.Id,
                Choices = new List<List<SuccessChoice>>
                {
                    new()
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 1,
                            State=1,
                            Scenario=0,
                            Effect = x.Choices[0][0].SuccessEffect
                        }
                    }
                }
            }))
            {
                if (!successEvent.Any(x => x.Id == i.Id))
                    successEvent.Add(i);
            }
            #endregion
            #region 固有
            foreach (var i in stories.Where(x => new[] { "バレンタイン", "ファン感謝祭", "クリスマス",
                                                         "情人节", "粉丝感谢祭", "圣诞节"}.Contains(x.Name)))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new List<List<SuccessChoice>>
                    {
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=1,
                                Effect=i.Choices[0][0].SuccessEffect
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=0,
                                Effect=i.Choices[0][0].FailedEffect
                            }
                        }
                    }
                });
            }
            #endregion
            #region 赛后
            foreach (var i in stories.Where(x => (x.Name.Contains("レース勝利") || x.Name.Contains("比赛胜利"))
                        && x.Choices[0].Any(x => x.SuccessEffect.Contains("体力-"))))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new List<List<SuccessChoice>>
                    {
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=3,
                                State=int.MaxValue,
                                Effect="体力-15"
                            },
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
                                Effect="体力-20,大概率更新商店道具"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=5,
                                State=int.MaxValue,
                                Effect="体力-15"
                            }
                        },
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=4,
                                State=1,
                                Effect="体力-10,大概率更新商店道具"
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
                                Effect="体力-25,大概率更新商店道具"
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
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=3,
                                State=1,
                                Effect="体力-5"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=5,
                                State=1,
                                Effect="体力-5"
                            }
                        }
                    }
                });
            }
            foreach (var i in stories.Where(x => (x.Name.Contains("レース入着") || x.Name.Contains("比赛入围"))
                    && x.Choices[0].Any(x => x.SuccessEffect.Contains("体力-"))))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new List<List<SuccessChoice>>
                    {
                        new List<SuccessChoice>
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
                                Effect="体力-25,大概率更新商店道具"
                            },
                        },
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=4,
                                State=1,
                                Effect="体力-15,大概率更新商店道具"
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
                                Effect="体力-35,大概率更新商店道具"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=4,
                                Scenario=4,
                                State=0,
                                Effect="体力-35"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=3,
                                State=1,
                                Effect="体力-10"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=5,
                                State=1,
                                Effect="体力-10"
                            }
                        }
                    }
                });
            }
            #endregion
            #region 训练失败
            foreach (var i in stories.Where(x => (x.Name.Contains("お大事に！") || x.Name.Contains("请保重")) ))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new List<List<SuccessChoice>>
                    {
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=1,
                                Effect="心情-1 上次训练属性-5"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=0,
                                Effect="心情-1 上次训练属性-5 【练习下脚】"
                            }
                        },
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=0,
                                Effect="心情-1 上次训练属性-10"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=1,
                                Effect="【练习上手】"
                            }
                        }
                    }
                });
            }
            foreach (var i in stories.Where(x => x.Name.Contains("無茶は厳禁！") ))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = new List<List<SuccessChoice>>
                    {
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=1,
                                Effect="体力+10 心情-3 上次训练属性-10 随机2属性-10"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=0,
                                Effect="体力+10 心情-3 上次训练属性-10 随机2属性-10 【练习下脚】"
                            }
                        },
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=0,
                                Effect="心情-3 上次训练-10 随机2属性-10 【练习下脚】"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=1,
                                Effect="体力+10 【练习上手】"
                            }
                        }
                    }
                });
            }
            #endregion
            #region ライバルに勝利
            successEvent.Add(new SuccessStory
            {
                Id = stories.First(x => x.Name == "ライバルに勝利！").Id,
                Choices = new List<List<SuccessChoice>>
                {
                    new List<SuccessChoice>
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
            #region 福来
            successEvent.Add(new SuccessStory
            {
                Id = 830078001,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                         SelectIndex=1,
                         State=1,
                         Scenario=0,
                         Effect="全ステータス+7、スキルPt+7、マチカネフクキタルの絆ゲージ+7"
                    },
                    new SuccessChoice
                    {
                         SelectIndex=2,
                         State=0,
                         Scenario=0,
                         Effect="智+4，告辞"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830078002,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                         SelectIndex=1,
                         State=1,
                         Scenario=0,
                         Effect="全ステータス+7、スキルPt+7、マチカネフクキタルの絆ゲージ+7"
                    },
                    new SuccessChoice
                    {
                         SelectIndex=2,
                         State=0,
                         Scenario=0,
                         Effect="智+4，告辞"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830078003,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                         SelectIndex=3,
                         State=0,
                         Scenario=0,
                         Effect="スキルPt+7、「ラッキーセブン」のヒントLv+3、マチカネフクキタルの絆ゲージ+5"
                    },
                    new SuccessChoice
                    {
                         SelectIndex=2,
                         State=1,
                         Scenario=0,
                         Effect="全ステータス+7、スキルPt+7、「スーパーラッキーセブン」のヒントLv+1、マチカネフクキタルの絆ゲージ+5"
                    },
                    new SuccessChoice
                    {
                         SelectIndex=1,
                         State=2,
                         Scenario=0,
                         Effect="全ステータス+7、スキルPt+77、「スーパーラッキーセブン」のヒントLv+3、マチカネフクキタルの絆ゲージ+5"
                    }
                })
            });
            #endregion
            #region 打针
            foreach (var i in stories.Where(x => x.Name == "あんし～ん笹針師、参☆上" || (x.Name.Contains("笹针师") && x.Id != 809005003)) )
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = CreateChoices(new List<SuccessChoice>
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 1,
                            State = 1,
                            Scenario = 0,
                            Effect = "全属性+20"
                        }
                    }, new List<SuccessChoice>
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 3,
                            State = 1,
                            Scenario = 0,
                            Effect = "直接习得弯道回复◯、直线回复"
                        }
                    }, new List<SuccessChoice>
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 5,
                            State = 1,
                            Scenario = 0,
                            Effect = "体力最大值+12，体力+40，治愈所有负面效果"
                        }
                    }, new List<SuccessChoice>
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 7,
                            State = 1,
                            Scenario = 0,
                            Effect = "体力+20，干劲提升，获得爱娇"
                        }
                    })
                });
            }
            #endregion
            #region 根诗歌剧
            foreach (var i in GetStoriesByName("求む、個性！"))
            {
                successEvent.Add(new SuccessStory
                {
                    Id = i.Id,
                    Choices = CreateChoices(new List<SuccessChoice>(), new List<SuccessChoice>
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 1,
                            Scenario=0,
                            State=1,
                            Effect="体力+30，绊+5"
                        }
                    })
                });
            }
            #endregion
            #region 温泉抽奖
            foreach (var i in stories.Where(x => x.Name.Contains("福引チャンス") || x.Name.Contains("抽奖试手气"))
                      .Select(x => new SuccessStory
            {
                Id = x.Id,
                Choices = new List<List<SuccessChoice>>
                {
                    new()
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 1,
                            State=1,
                            Scenario=0,
                            Effect = "体力+30、干劲↑、全属性+10、URA優勝后温泉旅行"
                        },
                        new SuccessChoice
                        {
                            SelectIndex = 2,
                            State=1,
                            Scenario=0,
                            Effect = "体力+30、干劲+2、全属性+10"
                        },
                        new SuccessChoice
                        {
                            SelectIndex = 3,
                            State=1,
                            Scenario=0,
                            Effect = "体力+20、干劲+1、全属性+5"
                        },
                        new SuccessChoice
                        {
                            SelectIndex = 4,
                            State=1,
                            Scenario=0,
                            Effect = "体力+20"
                        },
                        new SuccessChoice
                        {
                            SelectIndex = 5,
                            State=0,
                            Scenario=0,
                            Effect = "干劲−1"
                        }
                    }
                }
            }))
            {
                if (!successEvent.Any(x => x.Id == i.Id))
                    successEvent.Add(i);
            }
            #endregion
            #region 根双涡轮
            successEvent.Add(new SuccessStory
            {
                Id = 830112001,
                Choices = new List<List<SuccessChoice>>
                    {
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=1,
                                Effect="スピード(速度)+15、根性(毅力)+15、ツインターボの絆ゲージ+5"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=0,
                                Effect="体力-20、スピード(速度)+10、根性(毅力)+10、『遊びはおしまいっ！』のヒントLv+1、ツインターボの絆ゲージ+5、※連続イベントが終了"
                            }
                        },
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=1,
                                Effect="スキルPt(技能点数)+10、ツインターボの絆ゲージ+5"
                            }
                        }
                    }
            });//根两喷 事件一，修正kamigame
            successEvent.Add(new SuccessStory
            {
                Id = 830112002,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力-5、スピード(速度)+15、根性(毅力)+15、スキルPt(技能点数)+15、ツインターボの絆ゲージ+5"
                    },
                    new SuccessChoice
                    {
                        SelectIndex = 2,
                        State = 0,
                        Scenario = 0,
                        Effect = "スピード(速度)+10、根性(毅力)+10、『出力1000%！』のヒントLv+1、ツインターボの絆ゲージ+5、※連続イベントが終了"
                    }
                })
            });//根两喷 事件二，修正kamigame
            #endregion
            successEvent.Add(new SuccessStory
            {
                Id = 830098001,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力+15、「愛嬌◯」獲得、ハルウララの絆ゲージ+5"
                    }
                }, new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 2,
                        State = 0,
                        Scenario = 0,
                        Effect = "体力+15、ハルウララの絆ゲージ+5"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830041001,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力-10,SP+45"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 501020524,
                Choices = CreateChoices(new List<SuccessChoice>(), new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 3,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力+20,获得【深呼吸】的Hint"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 809006004,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力+15~19，耐力+10~13，ペースアップ的hint+3，解锁理事长的外出事件"
                    },
                    new SuccessChoice
                    {
                        SelectIndex = 2,
                        State = 0,
                        Scenario = 0,
                        Effect = "『おひとり様◯』的Hint Lv+5，樫本理子绊-10，无法与理事长外出"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830045001,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "干劲提升，直线一气的Hint+2"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 501026524,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力-10，力量+20，技能点+10"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 501022524,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 2,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力+30，技能点+15（待确认）"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 820034001,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "体力+10，速度+5,智力+5，技能点+10"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 501033524,
                Choices = CreateChoices(new List<SuccessChoice>(), new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "干劲提升，力量+15，直线一气的Hint+2"
                    }
                },
                new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 4,
                        State = 1,
                        Scenario = 0,
                        Effect = "干劲提升，智力+15，中距离直线的Hint+2"
                    }
                },
                new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 7,
                        State = 1,
                        Scenario = 0,
                        Effect = "干劲提升，速度+15，隠れ蓑的Hint+2"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830053002,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex = 1,
                        State = 1,
                        Scenario = 0,
                        Effect = "パワー(力量)+15、『マイルコーナー◯』のヒントLv+2、タイキシャトルの絆ゲージ+5"
                    },
                    new SuccessChoice
                    {
                        SelectIndex = 2,
                        State = 0,
                        Scenario = 0,
                        Effect = "パワー(力量)+10、連続イベントが終了"
                    }
                })
            });//SSR大树快车 事件二，修正kamigame
            #region 爱娇、切者、练习上手、注目株
            for (int i = 0; i < stories.Count; i++)
            {
                var successStory = new SuccessStory
                {
                    Id = stories[i].Id,
                    Choices = new List<List<SuccessChoice>>()
                };
                for (var j = 0; j < stories[i].Choices.Count; ++j)
                {
                    var choice = stories[i].Choices[j];
                    if (successStory.Choices.Count <= j) successStory.Choices.Add(new List<SuccessChoice>());
                    if (choice.Any(x => string.IsNullOrEmpty(x.FailedEffect)) || successEvent.Any(x => x.Id == stories[i].Id) != default) continue;
                    var isA = choice.FirstOrDefault(x => x.SuccessEffect.Contains("愛嬌◯") || x.SuccessEffect.Contains("惹人怜爱"));
                    var isB = choice.FirstOrDefault(x => x.SuccessEffect.Contains("切れ者") || x.SuccessEffect.Contains("能人"));
                    var isC = choice.FirstOrDefault(x => x.SuccessEffect.Contains("練習上手◯") || x.SuccessEffect.Contains("擅长练习"));
                    var isD = choice.FirstOrDefault(x => x.SuccessEffect.Contains("注目株") || x.SuccessEffect.Contains("潜力股"));
                    var realChoice = new[] { isA, isB, isC, isD }.FirstOrDefault(x => x != default);
                    if (realChoice == default) continue;
                    var successChoice = new SuccessChoice
                    {
                        SelectIndex = 2,
                        Scenario = 0,
                        State = 1,
                        Effect = realChoice.SuccessEffect
                    };
                    successStory.Choices[j] = new List<SuccessChoice> { successChoice };
                }
                if (successStory.Choices.Any(x => x.Any()))
                    successEvent.Add(successStory);
            }
            #endregion
            #region 神鹰
            successEvent.Add(new SuccessStory
            {
                Id = 830161001,
                Choices = LoadChoicesJson("""
                    [
                      [
                        { "SelectIndex": 1, "Scenario": 0, "State": 1, "Effect": "速度+15、「テンポアップ/加快节奏」的Hint Lv+3、神鹰的羁绊+15" },
                        { "SelectIndex": 2, "Scenario": 0, "State": 0, "Effect": "速度+5、「テンポアップ/加快节奏」的Hint Lv+1、神鹰的羁绊+5" }
                      ],
                      []
                    ]
                    """)
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830161003,
                Choices = LoadChoicesJson("""
                    [
                      [
                        { "SelectIndex": 1, "Scenario": 0, "State": 1, "Effect": "速度+20、耐力+20、「王手/将军」的Hint Lv+3、神鹰的羁绊+5" }
                      ],
                      [
                        { "SelectIndex": 3, "Scenario": 0, "State": 1, "Effect": "体力+10、速度+10、耐力+10、技能Pt+10、「弧線的プロフェッサー/弧线大师」的Hint Lv+3、神鹰的羁绊+5" }
                      ]
                    ]
                    """)
            });
            #endregion
            #region 佐岳
            successEvent.Add(new SuccessStory
            {
                Id = 809043003,
                Choices = LoadChoicesJson("""
                    [  
                      [
                        { "SelectIndex": 1, "Scenario": 0, "State": 2, "Effect": "干劲+1、明星量表+1、根性+3、技能Pt+3、佐岳的羁绊+5" },
                        { "SelectIndex": 2, "Scenario": 0, "State": 1, "Effect": "明星量表+1、根性+3、技能Pt+3、佐岳的羁绊+5" }
                      ]
                    ]
                    """)
            });
            successEvent.Add(new SuccessStory
            {
                Id = 809043004,
                Choices = LoadChoicesJson("""
                    [
                      [],
                      [
                        { "SelectIndex": 1, "Scenario": 0, "State": 1, "Effect": "体力+63、干劲+1、获得「幸运体质」、佐岳的羁绊+10、可以和佐岳外出" },
                        { "SelectIndex": 2, "Scenario": 0, "State": 0, "Effect": "体力-10、干劲+1、**不能外出**" }
                      ],
                      []
                    ]
                    """)
            });
            #endregion
            #region wildcard
            foreach (var i in stories)
            {
                var choice = i.Choices.FirstOrDefault(x => x.Any(y => y.SuccessEffect.Contains("ヒントLv") || y.SuccessEffect.Contains("Hint Lv")
                  || y.SuccessEffect.Contains("羁绊")));
                if (choice != default && !string.IsNullOrEmpty(choice[0].FailedEffect))
                {
                    var index = i.Choices.IndexOf(choice);
                    var story = new SuccessStory
                    {
                        Id = i.Id,
                        Choices = Enumerable.Range(0, index + 1).Select(x => new List<SuccessChoice>()).ToList()
                    };
                    story.Choices[index] = new List<SuccessChoice>
                    {
                        new SuccessChoice
                        {
                            SelectIndex=1,
                            Scenario=0,
                            State=1,
                            Effect=choice[0].SuccessEffect
                        }
                    };
                    successEvent.Add(story);
                }
            }
            #endregion
            #region 无选项事件且随机给不同技能hint
            successEvent.AddRange(
                stories.Where(x => x.Choices.Count == 1 && x.Choices[0].Any(x => x.SuccessEffect.Contains("ヒントLv") || x.SuccessEffect.Contains("Hint Lv")) && x.Choices[0].Any(x => x.FailedEffect.Contains("ヒントLv"))).Select(x => new SuccessStory
                {
                    Id = x.Id,
                    Choices = new List<List<SuccessChoice>>
                 {
                     new()
                     {
                         new SuccessChoice
                         {
                           SelectIndex=1,
                           Scenario=0,
                           State=1,
                           Effect= x.Choices[0][0].SuccessEffect
                         }
                     }
                 }
                }));
            #endregion
            
            Save("success_events", successEvent.DistinctBy(x => x.Id));

            List<List<SuccessChoice>> CreateChoices(params List<SuccessChoice>[] choices)
            {
                var list = new List<List<SuccessChoice>>();
                list.AddRange(choices);
                return list;
            }
            IEnumerable<Story> GetStoriesByName(string name)
            {
                return stories.Where(x => x.Name == name);
            }
            List<List<SuccessChoice>> LoadChoicesJson(string json)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore
                };
                return JsonConvert.DeserializeObject<List<List<SuccessChoice>>>(json);
            }
        }
    }
    public class SuccessStory
    {
        public long Id { get; set; }
        public List<List<SuccessChoice>> Choices { get; set; }
    }
    public class SuccessChoice
    {
        public int SelectIndex { get; set; }
        public int Scenario { get; set; }
        public int State { get; set; }
        public string Effect { get; set; } = string.Empty;
    }
}
