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
    internal class SupportDataGenerator : GeneratorBase
    {
        public void Generate()
        {
            var dataFromGamewith = new WebClient().DownloadString("https://gamewith-tool.s3.ap-northeast-1.amazonaws.com/uma-musume/support_card_datas.js").Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty);
            var array = JArray.Parse(dataFromGamewith.Replace("window.supportCardDatas=", string.Empty)[..^1]);
            var commandIdDic = new Dictionary<string, long> { { "友人", 0 }, { "スピード", 101 }, { "パワー", 102 }, { "スタミナ", 105 }, { "根性", 103 }, { "賢さ", 106 }, { "グループ", 0 } };

            var supports = new List<SupportData>();
            foreach (var i in array)
            {
                var support = new SupportData();
                support.Name = i["t"].ToString() + i["n"].ToString();
                support.CharaName = i["n"].ToString();
                support.Rarity = i["r"].ToString() switch
                {
                    "SSR" => 3,
                    "SR" => 2,
                    "R" => 1,
                    _ => 0
                };
                support.Id = support switch
                {
                    { CharaName: "桐生院葵", Rarity: 2 } => 20021,
                    { CharaName: "桐生院葵", Rarity: 1 } => 10022,
                    { CharaName: "チーム＜シリウス＞", Rarity: 3 } => 30081,
                    { CharaName: "玉座に集いし者たち", Rarity: 3 } => 30067,
                    _ => long.MaxValue
                };
                if (support.Id == long.MaxValue)
                {
                    var charaId = Data.NameToId[i["n"].ToString()];
                    if (support.Rarity == 3)
                    {
                        switch (charaId)
                        {
                            case 1002:
                                {
                                    support.Id = i["id"].Value<int>() == 176 ? 30076 : 30002; ;
                                }
                                break;
                            case 1001:
                                {
                                    support.Id = i["id"].Value<int>() switch
                                    {
                                        226 => 30105,
                                        1 => 30001,
                                        97 => 30025,
                                        263 => 30127,
                                    };
                                    break;
                                }
                            case 1009:
                                {
                                    support.Id = i["id"].Value<int>() switch
                                    {
                                        146 => 30047,
                                        205 => 30093,
                                    };
                                    break;
                                }
                            case 1024:
                                {
                                    support.Id = i["id"].Value<int>() switch
                                    {
                                        170 => 30072,
                                    };
                                    break;
                                }
                            case 1025:
                                {
                                    support.Id = i["id"].Value<int>() switch
                                    {
                                        230 => 30110,
                                        173 => 30075,
                                    };
                                    break;
                                }
                            case 1066:
                                {
                                    support.Id = i["id"].Value<int>() switch
                                    {
                                        237 => 30112,
                                        142 => 30060,
                                        98 => 30026,
                                    };
                                    break;
                                }
                            default:
                                {
                                    var t = Data.SupportCardData.Where(x => x.chara_id == charaId && x.rarity == support.Rarity);
                                    if (t.Count() == 1)
                                    {
                                        support.Id = t.First().id;
                                    }
                                    else
                                    {
                                        var fi = t.Where(x => x.command_id == commandIdDic[i["t"].ToString()]);
                                        if (fi.Count() == 1)
                                            support.Id = fi.First().id;
                                    }
                                    break;
                                }
                        };
                    }
                    else
                    {
                        var t = Data.SupportCardData.Where(x => x.chara_id == charaId && x.rarity == support.Rarity);
                        if (t.Count() == 1)
                        {
                            support.Id = t.First().id;
                        }
                        else
                        {
                            var fi = t.Where(x => x.command_id == commandIdDic[i["t"].ToString()]);
                            if (fi.Count() == 1)
                                support.Id = fi.First().id;
                        }
                    }
                }
                if (i == null || i["bn"] == null) continue;
                support.Effects.Add(1, new SupportData.Effect(i["bn"]["友情ボーナス"]));
                support.Effects.Add(2, new SupportData.Effect(i["bn"]["やる気効果"]));
                support.Effects.Add(3, new SupportData.Effect(i["bn"]["スピードボーナス"]));
                support.Effects.Add(4, new SupportData.Effect(i["bn"]["スタミナボーナス"]));
                support.Effects.Add(5, new SupportData.Effect(i["bn"]["パワーボーナス"]));
                support.Effects.Add(6, new SupportData.Effect(i["bn"]["根性ボーナス"]));
                support.Effects.Add(7, new SupportData.Effect(i["bn"]["賢さボーナス"]));
                support.Effects.Add(8, new SupportData.Effect(i["bn"]["トレーニング効果"]));
                support.Effects.Add(9, new SupportData.Effect(i["bn"]["初期スピード"]));
                support.Effects.Add(10, new SupportData.Effect(i["bn"]["初期スタミナ"]));
                support.Effects.Add(11, new SupportData.Effect(i["bn"]["初期パワー"]));
                support.Effects.Add(12, new SupportData.Effect(i["bn"]["初期根性"]));
                support.Effects.Add(13, new SupportData.Effect(i["bn"]["初期賢さ"]));
                support.Effects.Add(14, new SupportData.Effect(i["bn"]["初期絆ゲージ"]));
                support.Effects.Add(15, new SupportData.Effect(i["bn"]["レースボーナス"]));
                support.Effects.Add(16, new SupportData.Effect(i["bn"]["ファン数ボーナス"]));
                support.Effects.Add(17, new SupportData.Effect(i["bn"]["ヒントLv"]));
                support.Effects.Add(18, new SupportData.Effect(i["bn"]["ヒント発生率"]));
                support.Effects.Add(19, new SupportData.Effect(i["bn"]["得意率"]));
                support.Effects.Add(25, new SupportData.Effect(i["bn"]["イベント回復量"]));
                support.Effects.Add(26, new SupportData.Effect(i["bn"]["イベント効果"]));
                support.Effects.Add(27, new SupportData.Effect(i["bn"]["失敗率ダウン"]));
                support.Effects.Add(28, new SupportData.Effect(i["bn"]["体力消費ダウン"]));
                support.Effects.Add(30, new SupportData.Effect(i["bn"]["スキルPtボーナス"]));
                support.Effects.Add(31, new SupportData.Effect(i["bn"]["賢さ友情回復量"]));

                foreach (var uni in i["uni"].ToObject<List<string>>())
                {
                    switch (uni)
                    {
                        case "根性ボーナス":
                            support.Effects[6].Lv30 += 1;
                            support.Effects[6].Lv35 += 1;
                            support.Effects[6].Lv40 += 1;
                            support.Effects[6].Lv45 += 1;
                            support.Effects[6].Lv50 += 1;
                            break;
                        case "初期根性":
                            support.Effects[12].Lv30 += 20;
                            break;
                        case "やる気効果":
                            support.Effects[2].Lv30 += 30;
                            break;
                        case "ヒント発生率":
                            support.Effects[18].Lv30 += 99999999999;
                            break;
                        case "友情ボーナス":
                            support.Effects[1].Lv30 *= 1.12;
                            support.Effects[1].Lv35 *= 1.12;
                            support.Effects[1].Lv40 *= 1.12;
                            support.Effects[1].Lv45 *= 1.12;
                            support.Effects[1].Lv50 *= 1.12;
                            break;
                        case "初期スピード":
                            support.Effects[9].Lv30 += 20;
                            support.Effects[9].Lv35 += 20;
                            support.Effects[9].Lv40 += 20;
                            support.Effects[9].Lv45 += 20;
                            support.Effects[9].Lv50 += 20;
                            break;
                        case "トレーニング効果":
                            support.Effects[8].Lv30 += 0.05;
                            support.Effects[8].Lv35 += 0.05;
                            support.Effects[8].Lv40 += 0.05;
                            support.Effects[8].Lv45 += 0.05;
                            support.Effects[8].Lv50 += 0.05;
                            break;
                        case "初期絆ゲージ":
                            support.Effects[14].Lv30 += 15;
                            support.Effects[14].Lv35 += 15;
                            support.Effects[14].Lv40 += 15;
                            support.Effects[14].Lv45 += 15;
                            support.Effects[14].Lv50 += 15;
                            break;
                        case "得意率":
                            support.Effects[19].Lv30 *= 1.2;
                            support.Effects[19].Lv35 *= 1.2;
                            support.Effects[19].Lv40 *= 1.2;
                            support.Effects[19].Lv45 *= 1.2;
                            support.Effects[19].Lv50 *= 1.2;
                            break;
                        case "初期スタミナ":
                            support.Effects[10].Lv30 += 20;
                            support.Effects[10].Lv35 += 20;
                            support.Effects[10].Lv40 += 20;
                            support.Effects[10].Lv45 += 20;
                            support.Effects[10].Lv50 += 20;
                            break;
                        case "スピードボーナス":
                            support.Effects[3].Lv30 += 1;
                            support.Effects[3].Lv35 += 1;
                            support.Effects[3].Lv40 += 1;
                            support.Effects[3].Lv45 += 1;
                            support.Effects[3].Lv50 += 1;
                            break;
                        case "パワーボーナス":
                            support.Effects[5].Lv30 += 1;
                            support.Effects[5].Lv35 += 1;
                            support.Effects[5].Lv40 += 1;
                            support.Effects[5].Lv45 += 1;
                            support.Effects[5].Lv50 += 1;
                            break;
                        case "初期パワー":
                            support.Effects[11].Lv30 += 20;
                            support.Effects[11].Lv35 += 20;
                            support.Effects[11].Lv40 += 20;
                            support.Effects[11].Lv45 += 20;
                            support.Effects[11].Lv50 += 20;
                            break;
                        case "失敗率ダウン":
                            support.Effects[27].Lv30 += 0.1;
                            support.Effects[27].Lv35 += 0.1;
                            support.Effects[27].Lv40 += 0.1;
                            support.Effects[27].Lv45 += 0.1;
                            support.Effects[27].Lv50 += 0.1;
                            break;
                        case "体力消費ダウン":
                            support.Effects[28].Lv30 += 0.1;
                            support.Effects[28].Lv35 += 0.1;
                            support.Effects[28].Lv40 += 0.1;
                            support.Effects[28].Lv45 += 0.1;
                            support.Effects[28].Lv50 += 0.1;
                            break;
                        case "スキルPtボーナス":
                            support.Effects[30].Lv30 += 1;
                            support.Effects[30].Lv35 += 1;
                            support.Effects[30].Lv40 += 1;
                            support.Effects[30].Lv45 += 1;
                            support.Effects[30].Lv50 += 1;
                            break;
                        case "初期賢さ":
                            support.Effects[13].Lv30 += 20;
                            support.Effects[13].Lv35 += 20;
                            support.Effects[13].Lv40 += 20;
                            support.Effects[13].Lv45 += 20;
                            support.Effects[13].Lv50 += 20;
                            break;
                        case "レースボーナス":
                            support.Effects[15].Lv30 += 0.05;
                            support.Effects[15].Lv35 += 0.05;
                            support.Effects[15].Lv40 += 0.05;
                            support.Effects[15].Lv45 += 0.05;
                            support.Effects[15].Lv50 += 0.05;
                            break;
                        case "スタミナボーナス":
                            support.Effects[4].Lv30 += 1;
                            support.Effects[4].Lv35 += 1;
                            support.Effects[4].Lv40 += 1;
                            support.Effects[4].Lv45 += 1;
                            support.Effects[4].Lv50 += 1;
                            break;
                        case "賢さボーナス":
                            support.Effects[7].Lv30 += 1;
                            support.Effects[7].Lv35 += 1;
                            support.Effects[7].Lv40 += 1;
                            support.Effects[7].Lv45 += 1;
                            support.Effects[7].Lv50 += 1;
                            break;
                        case "ヒントイベント効果":
                            break;
                        case "スキルPtボーナス(絆80)":
                            support.Effects[18].Lv30 += 1;
                            break;
                        case "スタミナボーナス(絆80)":
                            support.Effects[18].Lv30 += 1;
                            break;
                    }
                }

                supports.Add(support);
            }
            Save("support_data", supports);
        }
    }
    public class SupportData
    {
        [JsonIgnore]
        public string Name { get; set; }
        [JsonIgnore]
        public string CharaName { get; set; }
        [JsonIgnore]
        public int Rarity { get; set; }
        public long Id { get; set; }
        public Dictionary<int, Effect> Effects { get; set; } = new();

        public class Effect
        {
            public double Lv30;
            public double Lv35;
            public double Lv40;
            public double Lv45;
            public double Lv50;

            public Effect(JToken? str)
            {
                if (str == null) return;
                var split = str.ToString()[1..^1].Split('|');
                for (var i = 0; i < split.Length; i++) split[i] = split[i] == "-" ? "0" : split[i];
                Lv30 = split[0].Contains("%") ? double.Parse(split[0].Replace("%", string.Empty)) / 100 : double.Parse(split[0]);
                Lv35 = split[1].Contains("%") ? double.Parse(split[1].Replace("%", string.Empty)) / 100 : double.Parse(split[1]);
                Lv40 = split[2].Contains("%") ? double.Parse(split[2].Replace("%", string.Empty)) / 100 : double.Parse(split[2]);
                Lv45 = split[3].Contains("%") ? double.Parse(split[3].Replace("%", string.Empty)) / 100 : double.Parse(split[3]);
                Lv50 = split[4].Contains("%") ? double.Parse(split[4].Replace("%", string.Empty)) / 100 : double.Parse(split[4]);
            }
        }

        public class UniqueCondition
        {
            public string Type; // 羁绊"evaluation" "友情训练次数" 合计羁绊"total_evaluation" 训练等级"training_level" "人头数" "体力最大值" "当前体力" "编成S卡种类" "粉丝数" "非得意训练" ""
        }
    }
}
