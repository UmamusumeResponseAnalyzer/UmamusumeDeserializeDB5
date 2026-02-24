using UmamusumeResponseAnalyzer.Entities;

namespace UmamusumeDeserializeDB5.Generator
{
    internal class CardName : GeneratorBase
    {
        public void Generate()
        {
            var list = new List<BaseName>();
            list.Add(new(101, "駿川たづな"));
            list.Add(new(102, "秋川理事長"));
            list.Add(new(103, "乙名史記者"));
            list.Add(new(104, "桐生院葵"));
            list.Add(new(106, "樫本理子"));
            list.Add(new(108, "ライトハロー"));
            list.Add(new(111, "都留岐涼花"));

            list.AddRange(Data.JP.TextData.Where(x => x.id == 170).Select(x => new BaseName(x.index, x.text)));
            list.AddRange(Data.JP.SupportCardData.Select(x => new SupportCardName(x.id, Data.JP.TextData.First(y => y.category == 76 && y.index == x.id).text, x.command_id, x.chara_id)));
            list.AddRange(Data.JP.TextData.Where(x => x.id == 5).Select(x => new UmaName(x.index, x.text)));

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
                    1001 => "特别",
                    1002 => "铃鹿",
                    1003 => "帝王",
                    1004 => "司机",
                    1005 => "富士",
                    1006 => "小栗",
                    1007 => "金船",
                    1008 => "伏特",
                    1009 => "大和",
                    1010 => "大树",
                    1011 => "草上",
                    1012 => "亚马",
                    1013 => "麦昆",
                    1014 => "神鹰",
                    1015 => "好歌",
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
                    1043 => "新光",
                    1044 => "东商",
                    1045 => "溪流",
                    1046 => "寄子",
                    1047 => "荒漠",
                    1048 => "佐敦",
                    1049 => "庆典",
                    1050 => "大进",
                    1051 => "西野",
                    1052 => "春丽",
                    1053 => "青竹",
                    1054 => "微光",
                    1055 => "周日",
                    1056 => "福来",
                    1057 => "ＣＢ",
                    1058 => "怒涛",
                    1059 => "多伯",
                    1060 => "内恰",
                    1061 => "圣王",
                    1062 => "诗歌",
                    1063 => "生野",
                    1064 => "善信",
                    1065 => "太阳",
                    1066 => "涡轮",
                    1067 => "光钻",
                    1068 => "北黑",
                    1069 => "千代",
                    1070 => "天狼",
                    1071 => "尔丹",
                    1072 => "八重",
                    1073 => "鹤丸",
                    1074 => "光明",
                    1075 => "谋勇",
                    1076 => "桂冠",
                    1077 => "成田",
                    1078 => "也文",
                    1079 => "狂怒",   // 狂怒乐章
                    1080 => "创升",   // Transcend
                    1081 => "希望",   // 希望之城
                    1082 => "北飞",
                    1083 => "吉兆",
                    1084 => "谷野",
                    1085 => "红宝",
                    1086 => "高峰",
                    1087 => "真弓",
                    1088 => "皇冠",
                    1089 => "高尚",
                    1090 => "极峰",
                    1091 => "强击",
                    1092 => "烈焰",   // 烈焰快驹
                    1093 => "凯斯",
                    1094 => "宝穴",
                    1095 => "信念",
                    1096 => "莫名",   // 莫名其妙
                    1097 => "往昔",   // 爱如往昔
                    1098 => "小林",
                    1099 => "北港",
                    1100 => "奇锐",
                    1102 => "万籁",
                    1103 => "莱斯",
                    1104 => "葛城",
                    1105 => "新宇",
                    1106 => "奇宝",
                    1107 => "舞城",
                    1108 => "大锤",
                    1109 => "莱茵",
                    1110 => "西沙",
                    1111 => "救主",   // 空中救主G
                    1112 => "勇敢",   // 勇敢的心
                    1113 => "火神",
                    1114 => "景致",   // 迷人景致
                    1115 => "巨匠",
                    1116 => "贵妇",
                    1117 => "芭蕾",  // 凯旋芭蕾
                    1118 => "爱慕",  // 爱慕槽
                    1119 => "梦旅",
                    1120 => "金镇",
                    1121 => "多旺",
                    1124 => "吹波",
                    1126 => "千岁",
                    1127 => "超常",   // 超常骏骥
                    1128 => "防爆",   // 防爆装束
                    1129 => "杏目",
                    1130 => "旺紫",   // 旺紫丁
                    1131 => "放声",   // 放声欢呼
                    1132 => "唯爱",   // 唯独爱你
                    1133 => "创世",   // 创世驹
                    1134 => "金花",   // 机伶金花
                    1135 => "旅程",   // 黄金旅程
                    1136 => "红色",  // 红色梦想
                    1137 => "神业",
                    1138 => "太子",
                    1139 => "娱乐",
                    1140 => "洛林", // 洛林軍歌
                    1141 => "神威", // 神威启示
                    1142 => "标志", // 标志名驹
                    1143 => "比萨", // 比萨胜驹
                    1144 => "玫瑰", // 玫瑰帝国
                    1145 => "统治", // 统治地位
                    2001 => "米可",
                    2002 => "糖衣",
                    2003 => "蚕茧",
                    2004 => "望族",
                    2005 => "卓芙",
                    2006 => "里格",
                    2007 => "索诺",
                    2008 => "ST2",
                    101 or 9001 => "绿帽",
                    102 or 9002 => "理事",
                    103 or 9003 => "记者",
                    104 or 9004 => "桐生",
                    9005 or 9007 => "庸医",
                    106 or 9006 => "理子",
                    107 or 9007 => "井上",
                    108 or 9008 => "B95",
                    9040 => "女神",   // 红女神会作为人头单独出现
                    9041 => "蓝神",
                    9042 => "黄神",
                    9043 => "佐岳",
                    111 or 9044 => "凉花",
                    9045 => "砂糖",
                    9046 => "蓝登",
                    9047 => "老登",
                    9048 => "红登",
                    9049 => "塔克",
                    9050 => "健子",
                    9051 => "潭泉", // 潭泉芳华
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
