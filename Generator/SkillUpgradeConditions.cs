using System.Text.RegularExpressions;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class SkillUpgradeConditions : GeneratorBase
    {
        public void Generate()
        {
            var data = Data.JP.TextData.Where(x => x.category == 290)
                .Where(x => !x.text.Contains("育成イベント"))
                .ToDictionary(x => x.index, x => x.text);
            var skill = data.Where(x => x.Value.Contains("スキル")); //需要
            var byProper = skill.Where(x => x.Value.Contains('＜') && x.Value.Contains('＞'));
            var hasSpecific = skill.Where(x => x.Value.Contains("を所持する"));
            var hasSpeedSkill = skill.Where(x => x.Value.Contains("速度が上がるスキル"));
            var hasRecoverSkill = skill.Where(x => x.Value.Contains("持久力が回復する"));
            var hasAcceSkill = skill.Where(x => x.Value.Contains("加速力が上がるスキル"));
            var hasCourseSkill = skill.Where(x => x.Value.Contains("コース取りがうまくなる"));
            var hasGreenSkill = skill.Where(x => x.Value.Contains("能力を引き出すスキル"));
            //var prop = data.Where(x => x.Value.Contains("能力"));
            //var winRaces = data.Where(x => x.Value.Contains("勝利"));
            //var genericRaces = data.Where(x => x.Value.Contains("勝以上"));
            //var condition = data.Where(x => x.Value.Contains("を持つ状態"));
            //var fan = data.Where(x => x.Value.Contains("ファン数"));
            //var others = data.Where(x => !(x.Value.Contains("スキル") || x.Value.Contains("ファン数") || x.Value.Contains("を持つ状態") || x.Value.Contains("能力") || x.Value.Contains("勝利") || x.Value.Contains("勝以上")));
            foreach (var item in hasSpecific)
            {
                var temp = item.Value;
                //temp = new Regex("＜(.*?)＞のスキルを(.*?)個以上所持する").Match(item.Value).Groups[1].Value;
                temp = new Regex("「(.*?)」").Match(item.Value).Groups[1].Value; //hasSpecific
                //temp = new Regex("速度が上がるスキルを(.*?)個以上所持する").Match(item.Value).Groups[1].Value; //hasSpeed
                //temp = new Regex("持久力が回復するスキルを(.*?)個以上所持する").Match(item.Value).Groups[1].Value; //hasRecover
                //temp = new Regex("加速力が上がるスキルを(.*?)個以上所持する").Match(item.Value).Groups[1].Value;
                //temp = new Regex("コース取りがうまくなるスキルを(.*?)個以上所持する").Match(item.Value).Groups[1].Value;
                //temp = new Regex("能力を引き出すスキルを(.*?)個以上所持する").Match(item.Value).Groups[1].Value;
                Console.WriteLine(temp);
            }
        }
    }
}
