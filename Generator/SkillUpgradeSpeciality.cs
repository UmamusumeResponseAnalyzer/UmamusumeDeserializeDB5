using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    /// <summary>
    /// 剧本限定进化技能
    /// </summary>
    internal class SkillUpgradeSpecialityGenerator : GeneratorBase
    {
        Regex Proper = new Regex("＜(.*?)＞(.*?)スキルを(.*?)個以上所持する");
        Regex Specific = new Regex("「(.*?)」");
        Regex Speed = new Regex("速度が上がるスキルを(.*?)個以上所持する");
        Regex Recovery = new Regex("持久力が回復するスキルを(.*?)個以上所持する");
        Regex Acceleration = new Regex("加速力が上がるスキルを(.*?)個以上所持する");
        Regex Lane = new Regex("コース取りがうまくなるスキルを(.*?)個以上所持する");
        Regex Stat = new Regex("能力を引き出すスキルを(.*?)個以上所持する");
        public void Generate()
        {
            var list = new List<SkillUpgradeSpeciality>();
            var conditions = Data.SkillUpgradeConditionTables.GroupBy(x => x.description_id).ToDictionary(x => x.Key, x => x.Select(y => y.id).ToArray());
            foreach (var i in Data.SkillUpgradeSpecialityTable)
            {
                var conds = conditions[i.skill_id].Select(conditionId =>
                {
                    var conditionText = Data.TextData.First(x => x.category == 290 && x.index == conditionId).text.Replace("\t", string.Empty);
                    if (conditionText.Contains('＜') && conditionText.Contains('＞'))
                    {
                        var regex = Proper.Match(conditionText);
                        return new UpgradeCondition
                        {
                            ConditionId = conditionId,
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
                            AdditionalRequirement = long.Parse(regex.Groups[3].Value)
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
                var obj = new SkillUpgradeSpeciality
                {
                    ScenarioId = i.scenario_id,
                    BaseSkillId = i.base_skill_id,
                    SkillId = i.skill_id,
                    UpgradeSkills = new Dictionary<long, UpgradeCondition[]>()
                    {
                        [i.skill_id] = conds.ToArray()
                    }
                };
                list.Add(obj);
            }
            Save("skill_upgrade_speciality", list);
        }
    }
    public class SkillUpgradeSpeciality
    {
        public long ScenarioId;
        public long BaseSkillId;
        public long SkillId;
        public Dictionary<long, UpgradeCondition[]> UpgradeSkills = new();
    }
}
