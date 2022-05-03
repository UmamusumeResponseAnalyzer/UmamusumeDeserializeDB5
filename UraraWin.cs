using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5
{
    internal static class UraraWin
    {
        static JObject content = JObject.Parse(Regex.Replace(new WebClient().DownloadString("https://raw.githubusercontent.com/wrrwrr111/pretty-derby/master/src/assert/db.json"), "\\\\\"(.+?)\\\\\"", "“$1”")
            .Replace("❝", "“").Replace("❞", "”")
            .Replace("夏合宿(2年目)にて", "夏合宿（2年目）にて").Replace("支えあいの秘訣", "支え合いの秘訣").Replace("えっアタシのバイト…やばすぎ？", "えっアタシのバイト……ヤバすぎ？")
            .Replace("夏合宿(3年目)終了", "夏合宿（3年目）終了").Replace("挑め、”宿命”", "挑め、“宿命”").Replace("楽しめ！一番！", "楽しめ！　1番！").Replace("ホット＆フライ", "ホット＆フライ！")
            .Replace("ラスボスはスペ", "ラスボスはスぺ").Replace("かっくいいね！", "かっくいぃね！").Replace("麗姿、瞳に焼き付いて", "麗姿、瞳に焼きついて").Replace("バ･ナ･ナ･先･輩♪", "バ・ナ・ナ・先・輩♪")
            .Replace("同室のあの子~そうだと思いましたわ~", "同室のあの子～そうだと思いましたわ～").Replace("メジロ家のあの子たち~難しい選択~", "メジロ家のあの子たち～難しい選択～").Replace("ライバルのあの子~どんな舞台でも~", "ライバルのあの子～どんな舞台でも～")
            .Replace("すべてはーーーのため", "すべては――のため").Replace("届いてforYou☆恩返し♪", "届いてforYOU☆恩返し♪").Replace("You’re My Sunshine☆", "You\'re My Sunshine☆").Replace("With My Whole Heart!", "With My Whole Heart！")
            .Replace("検証〜ネコ語は実在するのか？", "検証～ネコ語は実在するのか？").Replace("＠DREAM_MAKER", "@DREAM_MAKER").Replace("人生最大の幸運とは", "人生最大の幸福とは").Replace("愛情はほどほどに", "愛情はほどほどに……").Replace("What a wonderful stage!", "What a wonderful stage！")
            .Replace("あんしんかばん", "あんしんカバン").Replace("私がお守りです", "私がお守りです！").Replace("Search  or Mommy", "Search or Mommy").Replace("シチーガールの今の気分♪", "“シチーガール”の今の気分♪").Replace("勝利の味ってヤツ！", "勝利の味ってヤツ!")
            .Replace("殿下と映画鑑賞会", "殿下と映画観賞会").Replace("天皇賞(秋)の後に・空に手を", "天皇賞（秋）の後に・空に手を").Replace("チヨノオーは頼られたい", "チヨノオーは頼られたい！").Replace("言葉+……", "言葉＋……").Replace("かわいいの才能っ！", "かわいいの才能っ！？")
            .Replace("甦れ！ゴルシ印のソース焼きそば！", "甦れ！　ゴルシ印のソース焼きそば！").Replace("シークレット・ノート", "シークレット・ノート！").Replace("08:36/朝寝坊、やばっ", "08:36／朝寝坊、やばっ").Replace("キネマの思ひ出（お出かけ3）", "キネマの思ひ出")
            .Replace("ウマ娘ちゃん欠乏症", "ウマ娘ちゃん欠乏症！").Replace("シチースポットを目指して", "“シチースポット”を目指して").Replace("信仰心と親切心が交わる時ーー", "信仰心と親切心が交わる時――").Replace("公園での遊び方（お出かけ2）", "公園での遊び方")
            .Replace("13:12/昼休み、気合い入れなきゃ", "13:12／昼休み、気合い入れなきゃ").Replace("エモのためなら雨の中でも", "エモのためなら雨の中でも！").Replace("ミラクル☆エスケープ", "ミラクル☆エスケープ！")
            .Replace("オゥ！トゥナイト・パーティー☆", "オゥ！　トゥナイト・パーティー☆").Replace("皇帝の激励", "“皇帝”の激励").Replace("#lol #Party! #2nd", "#lol #Party!! #2nd").Replace("どぎまぎアフタヌーンティー", "どぎまぎアフタヌーンティー！")
            .Replace("みんなで作る特製ドリンク", "みんなで作る特製ドリンク♪").Replace("奏でようWINNING!", "奏でようWINNING！").Replace("喜ぶ顔を思い浮かべて（お出かけ3）", "喜ぶ顔を思い浮かべて").Replace("決戦☆あんし～んよ永遠に！（お出かけ3）", "決戦☆あんし～んよ永遠に！")
            .Replace("決戦☆あんし～んよ永遠に！（Rお出かけ3）", "決戦☆あんし～んよ永遠に！").Replace("アナタの心にチャージ一発", "アナタの心にチャージ一発！").Replace("あんし〜ん笹針師、参☆上", "あんし～ん笹針師、参☆上").Replace("『全力』&『普通』ダイエット！", "『全力』＆『普通』ダイエット！")
            .Replace("レース勝利！(1着)", "レース勝利！").Replace("レース勝利！(クラシック10月後半以前1着)", "レース勝利！").Replace("レース勝利！(クラシック11月前半以降1着)", "レース勝利！").Replace("レース勝利！(シニア5月前半以降1着)", "レース勝利！")
            .Replace("レース入着(2~5着)", "レース入着").Replace("レース入着(2/4/5着)", "レース入着").Replace("レース入着(クラシック10月後半以前2~5着)", "レース入着").Replace("レース入着(クラシック11月前半以降2~5着)", "レース入着").Replace("レース入着(シニア5月前半以降2~5着)", "レース入着")
            .Replace("レース敗北(6着以下)", "レース敗北").Replace("レース敗北(クラシック10月後半以前6着以下)", "レース敗北").Replace("レース敗北(クラシック11月前半以降6着以下)", "レース敗北").Replace("レース敗北(シニア5月前半以降6着以下)", "レース敗北")
            .Replace("ソリューション/勝ちたい", "ソリューション／勝ちたい").Replace("エラー/ざわつき", "エラー／ざわつき").Replace("レコード/勇気", "レコード／勇気").Replace("リクエスト/強者", "リクエスト／強者").Replace("Das Erlrbnis", "Das Erlebnis").Replace("Das Ende…", "Das Ende …")
            .Replace("Wo ein Wille ist,ist auch ein Weg.", @"Wo ein Wille ist\\[COMMA] ist auch ein Weg.").Replace("『B』＆『W』", "『B』&『W』").Replace("気合で完食。でも……", "気合いで完食。でも……").Replace("Something Special", "“Something Special”").Replace("Rain or Shine, I'm Fine!", @"Rain or Shine\\[COMMA] I'm Fine!")
            .Replace("ゆーわく！　ふにゃ〜ん！", "ゆーわく！　ふにゃ～ん！").Replace("夕日に向かって", "夕日にむかって").Replace("ふわふわ、きらきら、あこがれは", "ふわふわ、キラキラ、あこがれは").Replace("仮想カーレースの結果は…", "仮想カーレースの結果は……")
            .Replace("炸裂・エル・スペシャル！", "炸裂、エル・スペシャル！").Replace("来襲！スペース野球ゾンビ", "来襲！　スペース野球ゾンビ").Replace("母親", "母娘").Replace("ボクたちの1年戦争！", "ボクたちの1年戦争").Replace("予想も余裕で……？", "予想も余裕……で？")
            .Replace("落葉が照らすは", "落陽が照らすは").Replace("フジキセキ登場", "フジキセキ登場！").Replace("第五幕　置きに召すまま", "第五幕　お気に召すまま").Replace("\"…ボクは健康だもん\"", "\"……ボクは健康だもん\"").Replace("本気のかわいい", "カワイイの本気")
            .Replace("コロンとしたいの", "ころんってしたいのー").Replace("ゴルシ七不思議(未定)", "ゴルシ七不思議（未定）")
            //
            .Replace("『せんでん』最高！", "『せんでん』最高！！").Replace("みんなの応援で頑張る！", "みんなの応援でがんばる！").Replace("ヒシアマ姐さん奮闘記～問題児編～", "ヒシアマ姐さん奮闘記　～問題児編～").Replace("ヒシアマ姐さん奮闘記～追い込み編～", "ヒシアマ姐さん奮闘記　～追い込み編～")
            .Replace("姉御流の解決法", "姐御流の解決法").Replace("ユーザーネーム『W&T』", "ユーザーネーム『W＆T』").Replace("強敵と書いて『とも』と読むッ！", "強敵と書いて『とも』と読むッ！！")
            .Replace("（お出かけ1）", "").Replace("（お出かけ2）", "").Replace("（お出かけ3）", "").Replace("（お出かけ4）", "").Replace("（お出かけ5）", "")
            .Replace("がんばり屋と強さの秘訣（ライスシャワー）", "がんばり屋と強さの秘訣").Replace("景色に続く道（サイレンススズカ）", "“景色”に続く道").Replace("支えられて、見守られて", "支えらえて、見守られて")
            .Replace("熱血ッ！エアバスケッ！", "熱血ッ！　エアバスケッ！").Replace("踊れDREAM!", "踊れDREAM！").Replace("響き合うStage!", "響き合うStage！").Replace("ライスちゃんと頑張る！", "ライスちゃんとがんばる！")
            .Replace("キングちゃんと頑張る！", "キングちゃんとがんばる！").Replace("個性的な走り方", "個性的な走り方？").Replace("どんど晴れの立役者", "“どんど晴れ”の立役者").Replace("友達と頑張った日の1ページ", "友だちと頑張った日の1ページ")
            .Replace("わからせて差し上げますわ（メジロマックイーン）", "わからせて差し上げますわ").Replace("紡ぎはじめた想い（ウイニングチケット）", "紡ぎはじめた想い").Replace("妹はつらいよ（ナリタブライアン）", "妹はつらいよ")
            .Replace("知ってほしい夢（スペシャルウィーク）", "知ってほしい夢").Replace("支えるということ（チーム＜シリウス＞）", "支えるということ")
            .Replace("凛と咲く", "凜と咲く")
            );

        public static List<Unit> Units { get; set; } = new();
        public static void Init()
        {
            foreach (var i in content["supports"])
            {
                var unit = new Unit
                {
                    Name = i["charaName"].ToString(),
                    Prefix = i["name"].ToString()
                };
                foreach (var j in i["eventList"])
                {
                    var e = content["events"].FirstOrDefault(x => x["id"].ToString() == j.ToString());
                    unit.Events.Add(e["name"].ToString());
                }
                Units.Add(unit);
            }
            Units[Units.FindIndex(x => x.Prefix == "泥濘一笑、真っつぐに")].Events.Add("いきちょん！");
        }
    }
    public class Unit
    {
        public string Name { get; set; }
        public string Prefix { get; set; }
        public List<string> Events { get; set; } = new();
    }
}
