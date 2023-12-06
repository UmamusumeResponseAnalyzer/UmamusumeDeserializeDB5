using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UmamusumeResponseAnalyzer.Entities;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class CardName : GeneratorBase
    {
        public void Generate()
        {
            var list = new List<BaseName>();
            list.Add(new(101, "骏川手纲"));
            list.Add(new(102, "理事长"));
            list.Add(new(103, "乙名史记者"));
            list.Add(new(104, "桐生院葵"));
            list.Add(new(106, "代理理事长"));
            list.Add(new(108, "轻柔致意"));

            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from text_data where id=170";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    long index = (long)reader["index"];
                    string name = TLGTranslate.queryText(170, index, (string)reader["text"]);
                    //  list.Add(new((long)reader["index"], (string)reader["text"]));
                    list.Add(new(index, name));
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
                    sb.Append(list.First(x => x.Id == chara_id).Name);
                    string cardName = Data.TextData.First(x => x.category == 76 && x.index == id).text;
                    cardName = TLGTranslate.queryText(76, id, cardName);
                    list.Add(new SupportCardName(id, cardName, command_id, chara_id));
                }
            }
            // 马名
            foreach (var i in Data.TextData.Where(x => x.id == 5).ToDictionary(x => x.index, x => x.text))
            {
                string umaName = TLGTranslate.queryText(5, i.Key, i.Value);
                list.Add(new UmaName(i.Key, umaName));
            }

            foreach (var i in list)
            {
                if (i is UmaName un)
                {
                    un.Nickname = InShort(un.CharaId);
                }
                else if (i is SupportCardName scn)
                {
                    scn.Nickname = InShort(scn.CharaId);
                }
                else if (i is BaseName bn)
                {
                    bn.Nickname = InShort(bn.Id);
                }

                string InShort(long id) => id switch
                {
                    1001 => "特别周",
                    1002 => "铃鹿",
                    1003 => "帝宝",
                    1004 => "司机",
                    1005 => "富士",
                    1006 => "小栗",
                    1007 => "金船",
                    1008 => "伏特加",
                    1009 => "大和",
                    1010 => "大树",
                    1011 => "草上飞",
                    1012 => "亚马逊",
                    1013 => "麦昆",
                    1014 => "神鹰",
                    1015 => "好歌剧",
                    1016 => "白仁",
                    1017 => "皇帝",
                    1018 => "气槽",
                    1019 => "数码",
                    1020 => "青云",
                    1021 => "玉藻",
                    1022 => "美妙",
                    1023 => "大头",
                    1024 => "重炮",
                    1025 => "茶座",
                    1026 => "波旁",
                    1027 => "赖恩",
                    1028 => "菱曙",
                    1029 => "雪美",
                    1030 => "米浴",
                    1031 => "风神",
                    1032 => "速子",
                    1033 => "织姬",
                    1034 => "稻荷",
                    1035 => "奖券",
                    1036 => "神宫",
                    1037 => "闪耀",
                    1038 => "卡莲",
                    1039 => "川上",
                    1040 => "金城",
                    1041 => "进王",
                    1042 => "采珠",
                    1043 => "新光风",
                    1044 => "东商",
                    1045 => "溪流",
                    1046 => "寄寄子",
                    1047 => "荒漠",
                    1048 => "佐敦",
                    1049 => "庆典",
                    1050 => "大进",
                    1051 => "西野花",
                    1052 => "乌拉拉",
                    1053 => "青竹",
                    1054 => "微光",
                    1055 => "周日",
                    1056 => "福来",
                    1057 => "ＣＢ",
                    1058 => "怒涛",
                    1059 => "多伯",
                    1060 => "内恰",
                    1061 => "圣王",
                    1062 => "诗歌剧",
                    1063 => "生野",
                    1064 => "善信",
                    1065 => "太阳神",
                    1066 => "涡轮",
                    1067 => "光钻",
                    1068 => "北黑",
                    1069 => "千代",
                    1070 => "天狼星",
                    1071 => "阿尔丹",
                    1072 => "八重",
                    1073 => "鹤丸",
                    1074 => "光明",
                    1075 => "谋勇",
                    1076 => "桂冠",
                    1077 => "成田路",
                    1078 => "也文",
                    1083 => "吉兆",
                    1084 => "谷野",
                    1085 => "红宝",
                    1086 => "高峰",
                    1087 => "麻酱",
                    1088 => "皇冠",
                    1089 => "高尚",
                    1090 => "极峰",
                    1091 => "强击",
                    1093 => "凯斯",
                    1094 => "宝穴",
                    1098 => "小林",
                    1099 => "北港",
                    1100 => "奶奶",
                    1102 => "万籁",
                    1104 => "葛城",
                    1105 => "新宇",
                    1106 => "奇宝",
                    1107 => "跳舞城",
                    2001 => "米可",
                    2002 => "糖衣",
                    2003 => "蚕茧",
                    2004 => "望族",
                    2005 => "卓芙",
                    2006 => "里格",
                    101 or 9001 => "绿帽",
                    102 or 9002 => "理事",
                    103 or 9003 => "记者",
                    104 or 9004 => "桐生",
                    9005 => "庸医",
                    106 or 9006 => "理子",
                    108 or 9008 => "B95",
                    9040 => "红神",
                    9041 => "蓝神",
                    9042 => "黄神",
                    9043 => "佐岳",
                    _ => throw new Exception($"有新的id{id}需要手动添加简写")
                };
            }

            Save("names", list, true);
        }

    }
}

namespace UmamusumeResponseAnalyzer.Entities
{
    public class BaseName
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; } = string.Empty;

        public BaseName(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
    public class SupportCardName : BaseName
    {
        public long CharaId { get; set; }
        public long Type { get; set; }

        public SupportCardName(long id, string name, long type, long charaId) : base(id, name)
        {
            Type = type;
            CharaId = charaId;
        }
    }
    public class UmaName : BaseName
    {
        public long CharaId { get; set; }
        public UmaName(long id, string name) : base(id, name)
        {
            CharaId = long.Parse(id.ToString()[0] == '9' ? id.ToString()[1..5] : id.ToString()[..4]);
        }
    }
}
