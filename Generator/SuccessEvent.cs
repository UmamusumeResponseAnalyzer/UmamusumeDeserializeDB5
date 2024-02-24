using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static UmamusumeDeserializeDB5.Generator.SupportData;

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
            foreach (var i in stories.Where(x => x.Choices.Count == 2 && x.Choices[1].Any(x => x.SuccessEffect == "体力+30、スキルPt+10")).Select(x => new SuccessStory
            {
                Id = x.Id,
                Choices = new List<List<SuccessChoice>>
                {
                    new(),
                    new()
                    {
                        new SuccessChoice
                        {
                            SelectIndex = 1,
                            State=1,
                            Scenario=0,
                            Effect = "体力+30、技能点+10"
                        },
                        new SuccessChoice
                        {
                            SelectIndex = 2,
                            State=0,
                            Scenario=0,
                            Effect = "体力+30、技能点+10、速度-5、力量-5、获得『太り気味』"
                        }
                    }
                }
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
                Choices = new List<List<SuccessChoice>>
                    {
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=1,
                                Effect="体力+30、パワー+10、スキルPt+10"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=0,
                                Effect="体力+30、スピード−5、パワー+15、スキルPt+10、「太り気味」獲得"
                            }
                        },
                        new List<SuccessChoice>(),
                        new List<SuccessChoice>
                        {
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=0,
                                State=1,
                                Effect="体力が全回復する"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=2,
                                Scenario=0,
                                State=0,
                                Effect="体力が全回復する、スピード−5、「太り気味」獲得"
                            }
                        }
                    }
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
            foreach (var i in stories.Where(x => new[] { "バレンタイン", "ファン感謝祭", "クリスマス" }.Contains(x.Name)))
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
            foreach (var i in stories.Where(x => x.Name.Contains("レース勝利") && x.Choices[0].Any(x => x.SuccessEffect.Contains("体力-"))))
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
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=6,
                                State=int.MaxValue,
                                Effect="体力-15"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=7,
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
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=6,
                                State=1,
                                Effect="体力-5"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=7,
                                State=1,
                                Effect="体力-5"
                            }
                        }
                    }
                });
            }
            foreach (var i in stories.Where(x => x.Name.Contains("レース入着") && x.Choices[0].Any(x => x.SuccessEffect.Contains("体力-"))))
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
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=6,
                                State=1,
                                Effect="体力-10"
                            },
                            new SuccessChoice
                            {
                                SelectIndex=1,
                                Scenario=7,
                                State=1,
                                Effect="体力-10"
                            }
                        }
                    }
                });
            }
            #endregion
            #region 训练失败
            foreach (var i in stories.Where(x => x.Name.Contains("お大事に！")))
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
                         Effect="智+4，事件中断"
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
                         Effect="智+4，事件中断"
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
            foreach (var i in stories.Where(x => x.Name == "あんし～ん笹針師、参☆上"))
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
                            Effect = "直接习得コーナー回復◯、直線回復"
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
            foreach (var i in stories.Where(x => x.Name.Contains("福引チャンス") ).Select(x => new SuccessStory
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
                            Effect = "体力+30、やる気↑、全ステータス+10、URA優勝で温泉旅行"
                        },
                        new SuccessChoice
                        {
                            SelectIndex = 2,
                            State=1,
                            Scenario=0,
                            Effect = "体力+30、やる気+2、全ステータス+10"
                        },
                        new SuccessChoice
                        {
                            SelectIndex = 3,
                            State=1,
                            Scenario=0,
                            Effect = "体力+20、やる気+1、全ステータス+5"
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
                            Effect = "やる気−1"
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
            #region 神鹰
            successEvent.Add(new SuccessStory
            {
                Id = 830161001,
                Choices = new List<List<SuccessChoice>>
                {
                     new() {
                        new SuccessChoice   // 第1个选项
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+15、「テンポアップ」のヒントLv+3、エルコンドルパサーの絆ゲージ+15"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, // 返回状态
                             State=0,   // 失败
                             Scenario=0,
                             Effect="スピード+5、「テンポアップ」のヒントLv+1、エルコンドルパサーの絆ゲージ+5"
                        }
                    },
                    new()
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830161003,
                Choices = CreateChoices(new List<SuccessChoice>
                    {
                        new SuccessChoice   // 第1个选项
                        {
                            SelectIndex=1, // 返回状态
                            State=1,   // 成功
                            Scenario=0,
                            Effect="スピード+20、スタミナ+20、「王手」のヒントLv+3、エルコンドルパサーの絆ゲージ+5"
                        },
                    }, new List<SuccessChoice>
                    {
                        new SuccessChoice   // 第2个选项
                        {
                            SelectIndex=3, // 返回状态
                            State=1,   // 成功
                            Scenario=0,
                            Effect="体力+10、スピード+10、スタミナ+10、スキルPt+10、「弧線のプロフェッサー」のヒントLv+3、エルコンドルパサーの絆ゲージ+5"
                        }
                    }
                )
            });
            #endregion
            #region 佐岳
            successEvent.Add(new SuccessStory
            {
                Id = 809043003,
                Choices = CreateChoices(new List<SuccessChoice>
                {
                    new SuccessChoice
                    {
                        SelectIndex=1, // 返回状态
                        State=2,   // 大成功
                        Scenario=0,
                        Effect="各ウマ娘のスターゲージ+1、やる気+1、根性+3、スキルPt+3、佐岳メイの絆ゲージ+5"
                    },
                    new SuccessChoice
                    {
                        SelectIndex=2, // 返回状态
                        State=1,   // 成功
                        Scenario=0,
                        Effect="各ウマ娘のスターゲージ+1、根性+3、スキルPt+3、佐岳メイの絆ゲージ+5"
                    }
                })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 809043004,
                Choices = new List<List<SuccessChoice>>
                {
                    new(),  // 第一个选项不用roll
                    new() {
                        new SuccessChoice   // 第二个选项
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="体力+63、やる気+1、「幸運体質」獲得、佐岳メイの絆ゲージ+10、佐岳メイとお出かけできるようになる"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, // 返回状态
                             State=0,   // 失败
                             Scenario=0,
                             Effect="体力-10、やる気+1、**お出かけ不可になる**"
                        },
                    },
                    new()
                }
            });
            #endregion
            #region 北黑，春钻，速子，高峰，水司机
            successEvent.Add(new SuccessStory
            {
                Id = 830028002, // 北黑2
                Choices = new List<List<SuccessChoice>>
                {
                    new(),  
                    new() {
                        new SuccessChoice   // 第二个选项
                        {
                             SelectIndex=1, 
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+10、「直線巧者」のヒントLv+3、キタサンブラックの絆ゲージ+5"
                        }
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 501067800, // 春钻年底
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice 
                        {
                             SelectIndex=1, 
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スタミナ+30、「静かな呼吸」のヒントLv+3"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, 
                             State=0,   // 失败
                             Scenario=0,
                             Effect="スタミナ+10、「静かな呼吸」のヒントLv+1"
                        },
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830101001, // 速子1
                Choices = new List<List<SuccessChoice>>
                {
                    new(), 
                    new() {  // 第二个选项
                        new SuccessChoice 
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+5、スタミナ+5、賢さ+5、「先行直線◯」のヒントLv+3、「ささやき」のヒントLv+1、アグネスタキオンの絆ゲージ+5"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, // 返回状态
                             State=0,   // 失败
                             Scenario=0,
                             Effect="スピード+5、賢さ+5、「先行直線◯」のヒントLv+1、アグネスタキオンの絆ゲージ+5"
                        }
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830134001, // 高峰1
                Choices = new List<List<SuccessChoice>>
                {
                    new(),  // 第一个选项不用roll
                    new(),
                    new() {
                        new SuccessChoice   // 第3个选项
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="賢さ+15、メジロラモーヌの絆ゲージ+15"
                        }
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830107001, // 水司机1
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+15、賢さ+5、マルゼンスキーの絆ゲージ+15"
                        },
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830107002, // 水司机2
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+15、パワー+5、「集中力」のヒントLv+1、マルゼンスキーの絆ゲージ+15"
                        },
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830107001, // 水司机1
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+15、賢さ+5、マルゼンスキーの絆ゲージ+15"
                        },
                    }
                }
            });
            #endregion
            #region 宝穴，麦昆，大锤
            successEvent.Add(new SuccessStory
            {
                Id = 830147001, // 宝穴1
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+20、ジャングルポケットの絆ゲージ+15"
                        },
                        new()
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830147002, // 宝穴2
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+10、スタミナ+10、「ありったけ」のヒントLv+3、ジャングルポケットの絆ゲージ+10"
                        }
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830147003, // 宝穴3
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=2,   // 大成功
                             Scenario=0,
                             Effect="スピード+15、スタミナ+15、「昂ぶる鼓動」のヒントLv+3、ジャングルポケットの絆ゲージ+5"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スピード+10、スタミナ+10、「昂ぶる鼓動」のヒントLv+2、ジャングルポケットの絆ゲージ+5"
                        }
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830172001, // 麦昆1
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="体力+15、やる気+1、メジロマックイーンの絆ゲージ+15"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, // 返回状态
                             State=0,   // 失败
                             Scenario=0,
                             Effect="体力+10、やる気+1、メジロマックイーンの絆ゲージ+5"
                        },
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830172003,
                Choices = CreateChoices(
                    new List<SuccessChoice>
                    {
                        new SuccessChoice   // 第1个选项
                        {
                            SelectIndex=1, // 返回状态
                            State=1,   // 成功
                            Scenario=0,
                            Effect="賢さ+30、スキルPt+15、「かっとばせ！」のヒントLv+3、メジロマックイーンの絆ゲージ+5"
                        },
                    },
                    new List<SuccessChoice>
                    {
                        new SuccessChoice   // 第2个选项
                        {
                            SelectIndex=3, // 返回状态
                            State=1,   // 成功
                            Scenario=0,
                            Effect="体力+15、スピード+30、スキルPt+30、メジロマックイーンの絆ゲージ+5"
                        },
                        new SuccessChoice
                        {
                            SelectIndex=4, // 返回状态
                            State=0,   // 成功
                            Scenario=0,
                            Effect="体力+5、スピード+15、スキルPt+10、メジロマックイーンの絆ゲージ+5"
                        }
                    })
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830178003, // 大锤3
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="体力+15、スピード+20、パワー+20、「ネバーギブアップ」のヒントLv+3、ドゥラメンテの絆ゲージ+5"
                        },
                        new()
                    }
                }
            });
            #endregion
            #region 记者结算（测试）
            successEvent.Add(new SuccessStory
            {
                Id = 400000091,
                Choices = new List<List<SuccessChoice>>
                {
                    new() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=2,   // 成功
                             Scenario=0,
                             Effect="5種ステータス+5，スキルPt+20"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, // 返回状态
                             State=1,   // 失败
                             Scenario=0,
                             Effect="5種ステータス+3，スキルPt+10"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=3, // 返回状态
                             State=0,   // 失败
                             Scenario=0,
                             Effect="スキルPt+5"
                        }
                    }
                }
            });
            #endregion
            #region 万籁，内恰（事件效果由之前的选项决定）
            successEvent.Add(new SuccessStory
            {
                Id = 830173001, // 万籁1
                Choices = new List<List<SuccessChoice>>
                {
                    new List<SuccessChoice>() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="スタミナ+10，「直線巧者』のヒントLv+2，サウンズオブアースの絆ゲージ+10，连续事件3获得技能为「ハヤテ一文字」"
                        }
                    },
                    new List<SuccessChoice>() {
                        new SuccessChoice
                        {
                             SelectIndex=1,
                             State=1,   
                             Scenario=0,
                             Effect="根性+10，「直線回復』のヒントLv+2，サウンズオブアースの絆ゲージ+10，连续事件3获得技能为「好転一息」"
                        },
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830173003, // 万籁3
                Choices = new List<List<SuccessChoice>>
                {
                    new List<SuccessChoice>() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="体力+15、根性+30、「ハヤテ一文字」のヒントLv+2、サウンズオブアースの絆ゲージ+5"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2, // 第二选项羁绊加多少还待确认
                             State=1,   
                             Scenario=0,
                             Effect="体力+15、スキルPt+30、「好転一息」のヒントLv+2、サウンズオブアースの絆ゲージ+5"
                        }
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830138001, // 内恰1
                Choices = new List<List<SuccessChoice>>
                {
                    new List<SuccessChoice>() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="体力+10、やる気+1、ナイスネイチャの絆ゲージ+5，下一事件体力+30"
                        }
                    },
                    new List<SuccessChoice>() {
                        new SuccessChoice
                        {
                             SelectIndex=1,
                             State=1,
                             Scenario=0,
                             Effect="スタミナ+7、パワー+7、「尻尾上がり」のヒントLv+1、ナイスネイチャの絆ゲージ+5，下一事件获得「垂れウマ回避」"
                        }
                    }
                }
            });
            successEvent.Add(new SuccessStory
            {
                Id = 830138002, // 内恰2
                Choices = new List<List<SuccessChoice>>
                {
                    new List<SuccessChoice>() {
                        new SuccessChoice
                        {
                             SelectIndex=1, // 返回状态
                             State=1,   // 成功
                             Scenario=0,
                             Effect="体力+30、ナイスネイチャの絆ゲージ+5"
                        },
                        new SuccessChoice
                        {
                             SelectIndex=2,
                             State=1,
                             Scenario=0,
                             Effect="スタミナ+10、パワー+10、「垂れウマ回避」のヒントLv+1、ナイスネイチャの絆ゲージ+5"
                        }
                    }
                }
            });
            #endregion
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
                    var isA = choice.FirstOrDefault(x => x.SuccessEffect.Contains("愛嬌◯"));
                    var isB = choice.FirstOrDefault(x => x.SuccessEffect.Contains("切れ者"));
                    var isC = choice.FirstOrDefault(x => x.SuccessEffect.Contains("練習上手◯"));
                    var isD = choice.FirstOrDefault(x => x.SuccessEffect.Contains("注目株"));
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
            #region #3
            foreach (var i in stories)
            {
                var choice = i.Choices.FirstOrDefault(x => x.Any(y => y.SuccessEffect.Contains("ヒントLv")));
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
                stories.Where(x => x.Choices.Count == 1 && x.Choices[0].Any(x => x.SuccessEffect.Contains("ヒントLv")) && x.Choices[0].Any(x => x.FailedEffect.Contains("ヒントLv"))).Select(x => new SuccessStory
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
