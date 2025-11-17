using System.Text;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class ClimaxItems : GeneratorBase
    {
        public void Generate()
        {
            var sb = new StringBuilder();
            foreach (var i in Data.JP.TextData.Where(x => x.id == 225).ToDictionary(x => x.index, x => CN(x.text)))
            {
                sb.Append($"{{ {i.Key}, \"{i.Value}\" }}, ");
            }
            sb.Length -= 2;
            Console.WriteLine($"Climax Items: {sb}");
            //Save("climax_items", dic);
        }
        static string CN(string text)
        {
            return text
                .Replace("スピード", "速").Replace("スタミナ", "耐").Replace("パワー", "力").Replace("根性", "根").Replace("賢さ", "智")
                .Replace("のメモ帳", "+3").Replace("戦術書", "+7").Replace("秘伝書", "+15")
                .Replace("バイタル", "体力+").Replace("ロイヤルビタージュース", "苦茶").Replace("ロングエネドリンクMAX", "体力上限+8").Replace("エネドリンクMAX", "体力上限+4")
                .Replace("プレーンカップケーキ", "干劲+1").Replace("スイートカップケーキ", "干劲+2").Replace("おいしい猫缶", "猫罐头").Replace("にんじんBBQセット", "BBQ")
                .Replace("プリティーミラー", "爱娇").Replace("名物記者の双眼鏡", "注目株").Replace("効率練習のススメ", "练习上手").Replace("博学帽子", "切者")
                .Replace("すやすや安眠枕", "解寝不足").Replace("ポケットスケジュール帳", "解摸鱼癖").Replace("うるおいハンドクリーム", "解肌荒").Replace("スリムスキャナー", "解发胖").Replace("アロマディフューザー", "解头痛").Replace("練習改善DVD", "解练习下手").Replace("ナンデモナオール", "解全DB")
                .Replace("トレーニング", "").Replace("嘆願書", "请愿书")
                .Replace("リセットホイッスル", "哨子").Replace("チアメガホン", "20%喇叭").Replace("スパルタメガホン", "40%喇叭").Replace("ブートキャンプメガホン", "60%喇叭").Replace("アンクルウェイト", "负重脚环")
                .Replace("健康祈願のお守り", "御守").Replace("蹄鉄ハンマー", "蹄铁").Replace("三色ペンライト", "荧光棒");
        }
    }
}
