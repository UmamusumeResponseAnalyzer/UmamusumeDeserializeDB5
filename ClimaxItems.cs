using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5
{
    public class ClimaxItems
    {
        public void Generate()
        {
            var dic = new Dictionary<long, string>();

            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from text_data where id=225";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var jp = (string)reader["text"];
                    dic.Add((long)reader["index"], CN(jp));
                }
            }
            File.WriteAllText($"output/climaxitems.json", JsonConvert.SerializeObject(dic, Formatting.Indented));
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
