using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UmamusumeDeserializeDB5
{
    public class TLGStoryTextBlock
    {
        public string Name;
        public string Text;
        public List<string> ChoiceDataList;
        public List<string> ColorTextInfoList;
    }

    public class TLGStoryTimeline
    {
        public string Title;
        public List<TLGStoryTextBlock> TextBlockList;
    }

    public static class TLGTranslate
    {
        public static readonly string LocalizedDataRoot = "D:\\UMA\\Umamusume\\localized_data\\";

        public static Dictionary<string, string> StaticDict;
        public static Dictionary<string, Dictionary<string, string>> TextData;
        public static void load()
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            StaticDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("static.json", Encoding.UTF8), settings);
            TextData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>> >(
                File.ReadAllText(LocalizedDataRoot + "text_data.json", Encoding.UTF8), settings);
        }

        // 使用本地的static.json（非TLG）只替换少数词条
        public static string staticReplace(string s)
        {
            StringBuilder ret = new StringBuilder(s);
            foreach (var key in StaticDict.Keys)
            {
                ret.Replace(key, StaticDict[key]);
            }
            return ret.ToString();
        }

        // 查询对应译文，如果不存在则返回backupText
        public static string queryText(IFormattable category, IFormattable key, string backupText)
        {
            if (TextData.ContainsKey(category.ToString()) && TextData[category.ToString()].ContainsKey(key.ToString()))
                return TextData[category.ToString()][key.ToString()];
            else return backupText;
        }

        // 再封装一次，总有一个不为0的
        public static string queryTriggerName(SingleModeStoryData story, string backupText)
        {
            long id = 0;
            string ret = backupText;
            if (story.card_id != 0)
            {
                id = story.card_id;
                ret = queryText(4, id, backupText);
            }
            else if (story.card_chara_id != 0)
            {
                id = story.card_chara_id;
                ret = queryText(6, id, backupText);
            }
            else if (story.support_card_id != 0)
            {
                id = story.support_card_id;
                ret = queryText(75, id, backupText);
            }
            else if (story.support_chara_id != 0)
            {
                id = story.support_chara_id;
                ret = queryText(6, id, backupText);
            }
            if (!Data.NameToId.ContainsKey(ret) && id > 0)
                Data.NameToId[ret] = id;    // 更新名字缓存
            return ret;
        }

        public static string queryEventName(long id, string backupText)
        {
            string category = id.ToString()[..2];
            string card_id = id.ToString()[2..6];
            if (Directory.Exists(LocalizedDataRoot + $"stories\\story\\data\\{category}\\{card_id}"))
            {
                string filename = LocalizedDataRoot + $"stories\\story\\data\\{category}\\{card_id}\\storytimeline_{id}.json";
                if (File.Exists(filename))
                {
                    var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    TLGStoryTimeline timeline = JsonConvert.DeserializeObject<TLGStoryTimeline>(
                        File.ReadAllText(filename, Encoding.UTF8), settings);
                    Console.WriteLine($"{id} -> {timeline.Title}");
                    return timeline.Title;
                }
            }
            return backupText;
        }
    }
}
