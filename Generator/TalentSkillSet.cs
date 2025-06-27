using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class TalentSkillSet : GeneratorBase
    {
        /// <summary>
        /// upgrade_type 1角色2剧本
        /// num 疑似分组 1是第一个条件 2是第二个
        /// sub_num 分组内顺序
        /// timing_type
        /// count_type 1属性 2剧本/事件之类的?
        /// </summary>
        Dictionary<long, string> ConditionText = Data.TextData.Where(x => x.category == 290).ToDictionary(x => x.index, x => x.text);

        Regex Proper = new Regex("＜(.*?)＞のスキルを(.*?)個以上所持する");
        Regex Specific = new Regex("「(.*?)」");
        Regex Speed = new Regex("速度が上がるスキルを(.*?)個以上所持する");
        Regex Recovery = new Regex("持久力が回復するスキルを(.*?)個以上所持する");
        Regex Acceleration = new Regex("加速力が上がるスキルを(.*?)個以上所持する");
        Regex Lane = new Regex("コース取りがうまくなるスキルを(.*?)個以上所持する");
        Regex Stat = new Regex("能力を引き出すスキルを(.*?)個以上所持する");
        public void Generate()
        {
            var conditions = Data.SkillUpgradeConditionTables.GroupBy(x => x.description_id).ToDictionary(x => x.Key, x => x.Select(y => y.id).ToArray());
            var result = Data.AvailableSkillSetTableList.GroupBy(x => x.available_skill_set_id).ToDictionary(x => x.Key, x => x.ToArray().Select(y => new TalentSkill
            {
                SkillId = y.skill_id,
                Rank = y.need_rank
            }).ToList());
            foreach (var i in result)
            {
                var upgraded = Data.SkillUpgradeDescriptionTable.Where(x => x.card_id == i.Key);
                foreach (var j in upgraded)
                {
                    var conds = conditions[j.skill_id].Select(conditionId =>
                    {
                        var conditionText = ConditionText[conditionId];
                        if (conditionText.Contains('＜') && conditionText.Contains('＞'))
                        {
                            var regex = Proper.Match(conditionText);
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Group = Data.SkillUpgradeConditionTables.First(x => x.id == conditionId).num,
                                Type = UpgradeCondition.ConditionType.Proper,
                                Requirement = regex.Groups[1].Value switch
                                {
                                    "逃げ" => 1,
                                    "先行" => 2,
                                    "差し" => 3,
                                    "追込" => 4,
                                    "短距離" => 5,
                                    "マイル" => 6,
                                    "中距離" => 7,
                                    "長距離" => 8,
                                    "ダート" => 9
                                },
                                AdditionalRequirement = long.Parse(regex.Groups[2].Value)
                            };
                        }
                        else if (conditionText.Contains("を所持する"))
                        {
                            var regex = Specific.Match(conditionText).Groups[1].Value;
                            var skillId = Data.TextData.First(x => x.category == 47 && x.text == regex).index;
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = UpgradeCondition.ConditionType.Specific,
                                Requirement = skillId
                            };
                        }
                        else if (conditionText.Contains("速度が上がるスキル"))
                        {
                            var regex = Speed.Match(conditionText).Groups[1].Value;
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = UpgradeCondition.ConditionType.Speed,
                                Requirement = long.Parse(regex)
                            };
                        }
                        else if (conditionText.Contains("持久力が回復する"))
                        {
                            var regex = Recovery.Match(conditionText).Groups[1].Value;
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = UpgradeCondition.ConditionType.Recovery,
                                Requirement = long.Parse(regex)
                            };
                        }
                        else if (conditionText.Contains("加速力が上がるスキル"))
                        {
                            var regex = Acceleration.Match(conditionText).Groups[1].Value;
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = UpgradeCondition.ConditionType.Acceleration,
                                Requirement = long.Parse(regex)
                            };
                        }
                        else if (conditionText.Contains("コース取りがうまくなる"))
                        {
                            var regex = Lane.Match(conditionText).Groups[1].Value;
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = UpgradeCondition.ConditionType.Lane,
                                Requirement = long.Parse(regex)
                            };
                        }
                        else if (conditionText.Contains("能力を引き出すスキル"))
                        {
                            var regex = Stat.Match(conditionText).Groups[1].Value;
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                                Type = UpgradeCondition.ConditionType.Stat,
                                Requirement = long.Parse(regex)
                            };
                        }
                        else
                        {
                            return new UpgradeCondition
                            {
                                ConditionId = conditionId,
                            };
                        }
                    });
                    switch (j.rank)
                    {
                        case 3:
                            i.Value[i.Value.IndexOf(i.Value.First(x => x.Rank == 3))].UpgradeSkills.Add(j.skill_id, conds.ToArray());
                            break;
                        case 5:
                            i.Value[i.Value.IndexOf(i.Value.First(x => x.Rank == 5))].UpgradeSkills.Add(j.skill_id, conds.ToArray());
                            break;
                        default:
                            throw new Exception("出现了白技能的进化技能？");
                    }
                }
            }
            Save("talent_skill_sets", result);
        }
    }
}
