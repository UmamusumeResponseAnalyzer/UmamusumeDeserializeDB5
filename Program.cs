using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;
using System.Net;
using System.Text.RegularExpressions;

namespace UmamusumeDeserializeDB5
{
    public static class UmamusumeDeserializeDB5
    {
        public const string UmamusumeDatabaseFilePath = @"C:\Users\micro\AppData\LocalLow\Cygames\umamusume\master\master.mdb";
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Directory.CreateDirectory("output");
            var SingleModeStoryData = new List<SingleModeStoryData>();
            var TextData = new List<TextData>();
            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from text_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    TextData.Add(new TextData
                    {
                        category = (long)reader["category"],
                        id = (long)reader["id"],
                        index = (long)reader["index"],
                        text = (string)reader["text"]
                    });
                }
            }
            var StoryTextData = TextData.Where(x => x.id == 181 && x.category == 181).ToDictionary(x => x.index, x => x);
            var NameToId = TextData.Where(x => (x.id == 4 && x.category == 4) || (x.id == 6 && x.category == 6) || (x.id == 5 && x.category == 5) || (x.id == 75 && x.category == 75)).ToDictionary(x => x.text, x => x.index); //P-本名-S
            var SupportCardIdToCharaId = new Dictionary<long, long>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from single_mode_story_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var storyId = (long)reader["story_id"];
                    SingleModeStoryData.Add(new SingleModeStoryData
                    {
                        Name = StoryTextData.ContainsKey(storyId) ? StoryTextData[storyId].text : "成長のヒント",
                        id = (long)reader["id"],
                        card_chara_id = (long)reader["card_chara_id"],
                        card_id = (long)reader["card_id"],
                        ending_type = (long)reader["ending_type"],
                        event_title_chara_icon = (long)reader["event_title_chara_icon"],
                        event_title_dress_icon = (long)reader["event_title_dress_icon"],
                        event_title_style = (long)reader["event_title_style"],
                        gallery_flag = (long)reader["gallery_flag"],
                        gallery_list_id = (long)reader["gallery_list_id"],
                        gallery_main_scenario = (long)reader["gallery_main_scenario"],
                        mini_game_result = (long)reader["mini_game_result"],
                        past_race_id = (long)reader["past_race_id"],
                        race_event_flag = (long)reader["race_event_flag"],
                        se_change = (long)reader["se_change"],
                        short_story_id = (long)reader["short_story_id"],
                        show_clear = (long)reader["show_clear"],
                        show_progress_1 = (long)reader["show_progress_1"],
                        show_progress_2 = (long)reader["show_progress_2"],
                        show_succession = (long)reader["show_succession"],
                        story_id = storyId,
                        support_card_id = (long)reader["support_card_id"],
                        support_chara_id = (long)reader["support_chara_id"],
                    });
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from support_card_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SupportCardIdToCharaId.Add((long)reader["id"], (long)reader["chara_id"]);
                }
            }

            new SkillDataMgr().Generate();
            new CardName().Generate();
            new ClimaxItems().Generate();
            UraraWin.Init();
            //源: https://kamigame.jp/umamusume/page/152540608660042049.html
            File.WriteAllText("kamigame.json", new WebClient().DownloadString("https://storage.googleapis.com/vls-kamigame-gametool/json/1JrYvw5XiwWeKR5c2BKVQykutI_Lj2_zauLvaWtnzvDo_411452117.json"));
            var regexed = Regex.Replace(File.ReadAllText("kamigame.json"), "\\\\\"(.+?)\\\\\"", "“$1”");
            var kamigame = JArray.Parse(File.ReadAllText("kamigame.json") //接下来的一大片都是修复事件名使其与CY数据库中的事件名相匹配
                        .Replace(@"\n", "[Linebreak]").Replace("[Linebreak]\"", "\"").Replace("〜", "～").Replace("......", "……")
                        .Replace("なかよし☆こよしちゃん～探求編～", "なかよし☆こよしちゃん　～探求編～").Replace("なかよし☆こよしちゃん～調査編～", "なかよし☆こよしちゃん　～調査編～").Replace("なかよし☆こよしちゃん～実践編～", "なかよし☆こよしちゃん　～実践編～").Replace("ヒシアマ姐さん奮闘記～問題児編～", "ヒシアマ姐さん奮闘記　～問題児編～").Replace("ヒシアマ姐さん奮闘記～追い込み編～", "ヒシアマ姐さん奮闘記　～追い込み編～")
                        .Replace("クリスマスオグリ", "[キセキの白星]オグリキャップ").Replace("サンタビワハヤヒデ", "[ノエルージュ・キャロル]ビワハヤヒデ").Replace("ハロウィンスーパークリーク", "[シフォンリボンマミー]スーパークリーク")
                        .Replace("ハロウィンライスシャワー", "[Make up Vampire!]ライスシャワー").Replace("フルアーマー・フクキタル", "[吉兆・初あらし]マチカネフクキタル").Replace("花嫁エアグルーヴ", "[クエルクス・キウィーリス]エアグルーヴ")
                        .Replace("花嫁マヤノトップガン", "[サンライト・ブーケ]マヤノトップガン").Replace("桐生院葵", "[共に同じ道を！]桐生院葵").Replace("新ゴールドシチー", "[秋桜ダンツァトリーチェ]ゴールドシチー").Replace("新シンボリルドルフ", "[皓月の弓取り]シンボリルドルフ")
                        .Replace("新衣装エルコンドルパサー", "[ククルカン・モンク]エルコンドルパサー").Replace("新衣装グラスワンダー", "[セイントジェード・ヒーラー]グラスワンダー").Replace("新衣装トウカイテイオー", "[ビヨンド・ザ・ホライズン]トウカイテイオー")
                        .Replace("新衣装メジロマックイーン", "[エンド・オブ・スカイ]メジロマックイーン").Replace("水着スペシャルウィーク", "[ほっぴん♪ビタミンハート]スペシャルウィーク").Replace("水着マルゼンスキー", "[ぶっとび☆さまーナイト]マルゼンスキー")
                        .Replace("正月ハルウララ", "[初うらら♪さくさくら]ハルウララ").Replace("正月テイエムオペラオー", "[初晴・青き絢爛]テイエムオペラオー").Replace("バレンタインミホノブルボン", "[CODE：グラサージュ]ミホノブルボン")
                        .Replace("バレンタインエイシンフラッシュ", "[コレクト・ショコラティエ]エイシンフラッシュ")
                        .Replace("WeareBNW！！", "We are BNW！！").Replace("俊敏にして豪腕", "俊敏にして剛腕").Replace("メラメラ・ファイアー", "メラメラ・ファイアー！").Replace("世界級の…！？", "世界級の……！？").Replace("ダシが重要！？", "ダシが重要！！")
                        .Replace("08:36/朝寝坊、やばっ", "08:36／朝寝坊、やばっ").Replace("13:12/昼休み、気合い入れなきゃ", "13:12／昼休み、気合い入れなきゃ").Replace("甦れ！ゴルシ印のソース焼きそば！", "甦れ！　ゴルシ印のソース焼きそば！")
                        .Replace("強敵と書いて「トモ」と読むッ！！", "強敵と書いて『とも』と読むッ！！").Replace("諦めないで！可能性は無限大！", "諦めないで！　可能性は無限大！").Replace("イエス！レッツ・ハグ☆", "イエス！　レッツ・ハグ☆")
                        .Replace("オゥ！トゥナイト・パーティー☆", "オゥ！　トゥナイト・パーティー☆").Replace("#bff#Party!", "#bff #Party!").Replace("#lol#Party!!#2nd", "#lol #Party!! #2nd").Replace("きんぐちゃんとがんばる！", "キングちゃんとがんばる！")
                        .Replace("熱い誓い！アタシはヒーローになる！", "熱い誓い！　アタシはヒーローになる！").Replace("猛特訓！現れたキャロットマン！？", "猛特訓！　現れたキャロットマン！？").Replace("決戦！栄光の勝利をこの手に！", "決戦！　栄光の勝利をこの手に！")
                        .Replace("私...改革ですっ", "私……改革ですっ").Replace("にんじん…買ってくださいっ", "にんじん……買ってくださいっ")
                        .Replace("筋肉とともに明日へ！", "筋肉と共に明日へ！").Replace("剛毅木訥、仁に近し", "剛毅朴訥、仁に近し")
                        .Replace("「いつもの」ください", "『いつもの』ください！").Replace("いきなりマーダーミステリー！その1", "いきなりマーダーミステリー！　その1").Replace("いきなりマーダーミステリー！その2", "いきなりマーダーミステリー！　その2")
                        .Replace("いきなりマーダーミステリー！その3", "いきなりマーダーミステリー！　その3").Replace("『せんでん』最高！！", "『せんでん』最高！！")
                        .Replace("Wo ein Wille ist, ist auch ein Weg", "Wo ein Wille ist\\\\[COMMA] ist auch ein Weg.").Replace("'女帝\\\"の血族", "“女帝”の血族").Replace("愉悦ッ！密着取材！", "愉快ッ！　密着取材！")
                        .Replace("トレーナー並みの知識", "トレーナー並の知識").Replace("決起ッ！夜明け前ッ！！", "決起ッ！　夜明け前ッ！！").Replace("愉快ッ！密着取材！", "愉快ッ！　密着取材！").Replace("あと1曲！あと1曲だけ……！", "あと1曲！　あと1曲だけ……！")
                        .Replace("推し事探訪～入門編～", "推し事探訪　～入門編～").Replace("推し事探訪～極編～", "推し事探訪　～極編～").Replace("推し事探訪～免許皆伝編～", "推し事探訪　～免許皆伝編～")
                        .Replace("ユーザーネーム『W&T』", "ユーザーネーム『W＆T』").Replace("宝塚記念の後に・君はもう", "宝塚記念の後に・君はもう――").Replace("菊花賞の後に・Vorwarts gerichtet", "菊花賞の後に・Vorwärts gerichtet")
                        .Replace("Leichter gesagt als getan", "Leichter gesagt als getan.").Replace("デビュー戦の後に・突然の大宣言", "デビュー戦の後に・突然の大宣言！").Replace("ホット&フライ！", "ホット＆フライ！").Replace("オグリキャップ登場", "オグリキャップ登場！")
                        .Replace("ふたりの進む、その先へ", "ふたりの進む、その“先”へ").Replace("有馬記念の後に・芦毛の怪物、立つ", "有馬記念の後に・葦毛の怪物、立つ").Replace("すべてはーのため", "すべては――のため").Replace("私に一番似合う服", "私に1番似合う服")
                        .Replace("追って追われて", "追って、追われて").Replace("『一流ウマ娘』キングヘイロー", "『一流ウマ娘、キングヘイロー』").Replace("届かぬ人", "届かぬひと").Replace("ヘルプ！テスト前パニック", "ヘルプ！　テスト前パニック")
                        .Replace("ゴールドシチー登場", "ゴールドシチー登場！").Replace("熱血ッ！エアバスケッ！", "熱血ッ！　エアバスケッ！").Replace("ホープフルＳの後に・エデンへの道", "ホープフルSの後に・エデンへの道").Replace("バクシン的トレーニング…？", "バクシン的トレーニング……？")
                        .Replace("高松宮記念の後に・有終バクシン", "高松宮記念の後に・有終バクシン！").Replace("セントウルSの後に・3600！", "セントウルSの後に・3600！！！").Replace("実直！実験！実証！", "実直！　実験！　実証！").Replace("粋か！バクシンか！", "粋か！　バクシンか！")
                        .Replace("家事は大変", "家事は大変！").Replace("ダービートレーナー", "『ダービートレーナー』").Replace("ダービー記者会見", "ダービー記者会見！")
                        .Replace("日本ダービーの後に・「そうだね」", "日本ダービーの後に・『そうだね』").Replace("「ただいま」", "『ただいま』")
                        .Replace("宝塚記念の後に・『つまんない』②", "宝塚記念の後に・『つまんない』")
                        .Replace("ユニコーンSの後に・砂塵を超えて", "ユニコーンSの後に・砂塵を越えて").Replace("日本ダービーの後に・アイツの背中", "日本ダービーの後に・アイツの姿")
                        .Replace("目標達成の後に・毎日ボーノ配置☆", "目標達成の後に・毎日ボーノ配信☆")
                        .Replace("タイマン！スケバン！勝負服！", "タイマン！　スケバン！　勝負服！")
                        .Replace("\\\"皇帝”の激励", "“皇帝”の激励").Replace("桜花賞の後に・次のティアラへ", "桜花賞の後に・次のティアラヘ").Replace("オークスの後に・最後のティアラへ", "オークスの後に・最後のティアラヘ").Replace("高松宮記念の後に・遠ざかる背中＜1着時＞", "高松宮記念の後に・遠ざかる背中")
                        .Replace("＠DREAM_MAKER", "@DREAM_MAKER").Replace("弥生賞の後に・次は……え！？", "弥生賞の後に・次は……え？").Replace("食い倒れ！七福神グルメめぐり", "食い倒れ！　七福神グルメめぐり").Replace("目指せ！大人のちゃんこ鍋☆", "目指せ！　大人のちゃんこ鍋☆")
                        .Replace("待ち\\\"豆”来たらず", "待ち“豆”来たらず").Replace("ライ ホウ シャ", "ライ　ホウ　シャ").Replace("RIVER　RUNS……", "RIVER RUNS……").Replace("募る思い、嗚呼届いていますか", "募る想い、嗚呼届いていますか").Replace("来襲！スペース野球ゾンビ", "来襲！　スペース野球ゾンビ")
                        .Replace("マッスル一発", "マッスル一発！").Replace("寝不足で...…", "寝不足で……").Replace("デビュー戦の後に・突然の大宣言！！", "デビュー戦の後に・突然の大宣言！").Replace("日本ダービーの後に・現実の高低差", "日本ダービーの後に・現実の高度差")
                        .Replace("もう一度、決意を", "もう1度、決意を").Replace("夜の学校のご老公？", "夜の学園のご老公？").Replace("・・・ボクは健康だもん", "……ボクは健康だもん").Replace("天皇賞(秋)の後に・未踏の栄光", "天皇賞（秋）の後に・未踏の栄光")
                        .Replace("デビュー戦の後に・First step", "デビュー戦の後に・First Step").Replace("エリザベス女王杯の後に・Win！", "エリザベス女王杯の後に・Win!").Replace("エリザベス女王杯の後に・Lost", "エリザベス女王杯の後に・Lost...").Replace("有馬記念の後に・Still with you", "有馬記念の後に・Still with You")
                        .Replace("皐月賞の後に・汝が吠えるは", "皐月賞の後に・汝が吼えるは").Replace("毎日王冠の後に・運命、此処に集いて", "毎日王冠の後に・運命、彼方に集いて").Replace("道分かたれて", "道、分かたれて").Replace("日本ダービーの後に・希望の灯火", "日本ダービーの後に・希望の灯光")
                        .Replace("ジャパンCにの後に・重なり合う源流", "ジャパンCの後に・重なり合う源流").Replace("開設ッ！特別ショップ！", "開店ッ！特別ショップ！").Replace("ファル子の愛情ブレンド", "ファル子の愛情ブレンド☆").Replace("皐月賞の後に・次は絶対", "皐月賞の後に・次は絶対！")
                        .Replace("目指せ、頼れるお姉ちゃん", "目指せ！頼れるお姉ちゃん").Replace("鼻血がでるのも悪くない", "鼻血が出るのも悪くない").Replace("『全力』&『普通』ダイエット！", "『全力』＆『普通』ダイエット！").Replace("『退学」を賭けた勝負", "『退学』を賭けた勝負")
                        .Replace("たまにはホワイトに", "たまには、ホワイトに").Replace("委員達の井戸端会議", "委員たちの井戸端会議").Replace("マーベラス☆ダイブ！", "マーベラス☆ダイブ！！")
                        .Replace("レース勝利！", "[レース勝]利！").Replace("レース勝利", "レース勝利！").Replace("[レース勝]利！", "レース勝利！")
                        .Replace("強敵と書いて「トモ」と読むッ！！", "強敵と書いて『とも』と読むッ！！").Replace("ブライアンは見た…", "ブライアンは見た……！").Replace("砂の修行！", "砂の修業！")
                        .Replace("支えられて、見守られて", "支えらえて、見守られて").Replace("ふたりのゆめはおわらない", "2人の夢は終わらない")
                        .Replace("凛と咲く", "凜と咲く")
                ); ;

            var events = new List<Story>();
            var failed = new List<string>();
            for (var i = 1; i < kamigame.Count; i++)
            {
                var item = kamigame[i];
                var eventName = item[0].ToString();
                eventName = Regex.Replace(eventName, "\"(.+?)\"", "“$1”");
                if (eventName.Count(x => x == '“') < 2)
                    eventName = Regex.Replace(eventName, "”(.+?)”", "“$1”");
                if (string.IsNullOrEmpty(eventName)) continue;
                var eventCategory = item[1].ToString();
                var triggerName = item[2].ToString();
                var options = item[4].ToString().Split("[Linebreak]");
                var successEffect = item[5].ToString().Split("[Linebreak]");
                var failureEffect = item[6].ToString().Split("[Linebreak]");
                //这上面也差不多都是修复事件名
                var choices = new List<Choice>();
                for (var j = 0; j < options.Length; j++)
                {
                    //if ((j + 1) > successEffect.Length)
                    //Console.WriteLine(item[0].ToString());
                    choices.Add(new Choice
                    {
                        Option = options[j],
                        SuccessEffect = (j + 1) > successEffect.Length ? string.Empty : successEffect[j],
                        FailedEffect = (j + 1) > failureEffect.Length ? string.Empty : failureEffect[j]
                    });
                }
                if (eventCategory == "サポートカード")
                {
                    triggerName = GetSupportCardNameByEventName(eventName);
                }
                var id = NameToId.ContainsKey(triggerName) ? NameToId[triggerName] : 0;
                //id长度大于4位即当前S卡专属的进度事件（比如北黑给金弯的那三个），否则为所有该角色S卡共有的事件（比如西野花的爱娇）
                var charaId = eventCategory == "サポートカード" ? (id.ToString().Length > 4 ? SupportCardIdToCharaId[id] : id) : (id.ToString().Length > 4 ? int.Parse(id.ToString()[..4]) : id);
                if (eventName == "あんし～ん笹針師、出☆没")
                {
                    id = 0;
                    charaId = 0;
                }
                //if (eventCategory == "サポートカード" && id != 0)
                //{
                //    using var cmd = conn.CreateCommand();
                //    cmd.CommandText = $"select chara_id from support_card_data where id={id}";
                //    var reader = cmd.ExecuteReader();
                //    reader.Read();
                //    charaId = (long)reader["chara_id"];
                //}
                var storyData = SingleModeStoryData.Where(x => x.Name == eventName && (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId));
                if (!storyData.Any())
                {
                    //试图匹配一个差不多的事件名，也是摆烂的做法
                    var correctedEventName = CorrectEventName(eventName);
                    if (!string.IsNullOrEmpty(correctedEventName))
                    {
                        storyData = SingleModeStoryData.Where(x => x.Name == correctedEventName && (x.card_chara_id == charaId || x.card_id == id || x.support_card_id == id || x.support_chara_id == charaId));
                        if (!storyData.Any())
                        {
                            //反正我自己看得懂（
                            failed.Add($"Can't Find {eventName}||{correctedEventName} For {id}||{charaId}||{triggerName} In Cy's Database!");
                        }
                        else
                        {
                            AddStory(triggerName, correctedEventName, storyData, choices);
                        }
                    }
                }
                else
                {
                    AddStory(triggerName, eventName, storyData, choices);
                }
            }

            foreach (var i in failed.Distinct()) Console.WriteLine(i);
            SuccessEvent.Generate(events);
            File.WriteAllText("output/id.json", JsonConvert.SerializeObject(TextData.Where(x => x.id == 4).ToDictionary(x => x.index, x => x.text), Formatting.Indented));
            File.WriteAllText("output/events.json", JsonConvert.SerializeObject(events.DistinctBy(x => x.Id), Formatting.Indented));

            void AddStory(string triggerName, string eventName, IEnumerable<SingleModeStoryData> storyData, List<Choice> choices)
            {
                if (triggerName == "共通")
                {
                    foreach (var j in storyData)
                    {
                        if (events.Where(x => x.Id == j.story_id).Any())
                        {
                            return;
                        }
                        events.Add(new Story
                        {
                            Id = j.story_id,
                            Name = eventName,
                            TriggerName = triggerName,
                            Choices = choices
                        });
                    };
                }
                else
                {
                    var j = storyData.First();
                    if (events.Where(x => x.Id == j.story_id).Any())
                    {
                        return;
                    }
                    events.Add(new Story
                    {
                        Id = j.story_id,
                        Name = eventName,
                        TriggerName = triggerName,
                        Choices = choices
                    });
                }
            }
            string GetSupportCardNameByEventName(string eventName, bool corrected = false)
            {
                switch (eventName)
                {
                    //听说这个是带庸医时的扎针事件，效果和普通版完全一致，但是大部分网站都没收录
                    case "あんし～ん笹針師、出☆没":
                        return "[ブスッといっとく？]安心沢刺々美";
                }

                var urarawinEvent = UraraWin.Units.Where(x => x.Events.Contains(eventName));
                if (urarawinEvent.Any())
                {
                    if (urarawinEvent.Count() > 1)
                    {
                        return urarawinEvent.First().Name;
                    }
                    else if (urarawinEvent.Count() == 1)
                    {
                        return $"[{urarawinEvent.First().Prefix}]{urarawinEvent.First().Name}";
                    }
                    else
                    {
                        failed.Add($"UraraWin Can't Find Support Card: {eventName}");
                    }
                }
                else if (!corrected)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"select *,count(*) from text_data where text like '{eventName[..(eventName.Length / 2)]}%'";
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        var count = (long)reader["count(*)"];
                        if (count > 1)
                        {
                            failed.Add($"UraraWin Not Found: {eventName} AND TOO MANY TO MATCH");
                        }
                        else if (count == 1)
                        {
                            corrected = true;
                            eventName = (string)reader["text"];
                            return GetSupportCardNameByEventName(eventName, corrected);
                        }
                        else
                        {
                            failed.Add($"UraraWin Not Found: {eventName}");
                        }
                    }
                }
                else
                {
                    failed.Add($"UraraWin Can't Find Event: {eventName}");
                }
                return string.Empty;
            }
            string CorrectEventName(string eventName, bool corrected = false)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"select *,count(*) from text_data where text like '{eventName[..(eventName.Length / 2)]}%'";
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    var count = (long)reader["count(*)"];
                    if (count > 2)
                    {
                        failed.Add($"Correct Failed: {eventName} AND TOO MANY TO MATCH");
                    }
                    else if (count > 0)
                    {
                        corrected = true;
                        eventName = (string)reader["text"];
                        return eventName;
                    }
                    else
                    {
                        failed.Add($"Correct Failed: {eventName}");
                    }
                }
                return string.Empty;
            }
        }
    }

    class KamigameEvent
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Character { get; set; }
        public string Timing { get; set; }
        public List<Choice> Choices { get; set; }
        public string ScenarioLinkCharacter { get; set; }
    }
    public class Story
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string TriggerName { get; set; }
        public List<Choice> Choices { get; set; }

    }
    public class Choice
    {
        public string Option { get; set; }
        public string SuccessEffect { get; set; }
        public string FailedEffect { get; set; }
    }
    public class SingleModeStoryData
    {
        public string Name { get; set; } = "未找到";
        public long id { get; set; }
        public long story_id { get; set; }
        public long short_story_id { get; set; }
        public long card_id { get; set; }
        public long card_chara_id { get; set; }
        public long support_card_id { get; set; }
        public long support_chara_id { get; set; }
        public long show_progress_1 { get; set; }
        public long show_progress_2 { get; set; }
        public long show_clear { get; set; }
        public long show_succession { get; set; }
        public long event_title_style { get; set; }
        public long event_title_dress_icon { get; set; }
        public long event_title_chara_icon { get; set; }
        public long se_change { get; set; }
        public long ending_type { get; set; }
        public long race_event_flag { get; set; }
        public long mini_game_result { get; set; }
        public long gallery_main_scenario { get; set; }
        public long gallery_flag { get; set; }
        public long gallery_list_id { get; set; }
        public long past_race_id { get; set; }
    }
    public class TextData
    {
        public long id { get; set; }
        public long category { get; set; }
        public long index { get; set; }
        public string text { get; set; }
    }
}