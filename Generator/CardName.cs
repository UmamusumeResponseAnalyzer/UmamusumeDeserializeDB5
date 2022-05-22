using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class CardName : GeneratorBase
    {
        public void Generate()
        {
            var cn = new Dictionary<string, string>();
            foreach (var i in JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(new WebClient().DownloadString("https://raw.githubusercontent.com/wrrwrr111/pretty-derby/master/src/assert/locales/zh_CN.json")))
            {
                // 因为源文件里有
                // "Outfit as No": [
                //     null,
                //     ""
                //   ],
                // 这样的数据所以不能直接反序列化为Dictionary<string,string>
                if (i.Value is string s)
                {
                    cn.Add(i.Key, s);
                }
            }
            var dic = new Dictionary<long, string>();
            dic.Add(101, "骏川手纲");
            dic.Add(102, "理事长");
            dic.Add(103, "乙名史记者");
            dic.Add(104, "桐生院葵");
            dic.Add(106, "代理理事长");

            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from text_data where id=170";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var jp = (string)reader["text"];
                    jp = jp switch
                    {
                        "ツルマルツヨシ" => "鹤丸刚志",
                        "ハッピーミーク" => "快乐米可",
                        "ビターグラッセ" => "Bitter Glasse",
                        "リトルココン" => "Little Cocon",
                        "秋川やよい" => "秋川弥生",
                        _ => jp
                    };
                    var translated = cn.ContainsKey(jp) ? cn[jp] : jp;
                    translated = translated switch
                    {
                        "梅吉罗布赖特" => "目白光明",
                        "阿德米亚贝加" => "爱慕织姬",
                        "鲁道夫象征" => "皇帝", //大概不是错翻但是我喊皇帝喊习惯了
                        "雷电顶载" => "成田路",
                        "山宁泽弗" => "也文摄辉",
                        "海比先生" => "Mr.CB",
                        _ => translated
                    };
                    dic.Add((long)reader["index"], translated);
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from support_card_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var id = (long)reader["id"];
                    var chara_id = (long)reader["chara_id"];
                    var command_id = (long)reader["command_id"];

                    var sb = new StringBuilder();
                    sb.Append(command_id switch
                    {
                        101 => "[速]",
                        102 => "[力]",
                        103 => "[根]",
                        105 => "[耐]",
                        106 => "[智]",
                        0 => "[友]",
                        _ => ""
                    });
                    sb.Append(dic[chara_id]);
                    dic.Add(id, sb.ToString());
                }
            }

            Save("name_cn", dic);
        }
    }
}
