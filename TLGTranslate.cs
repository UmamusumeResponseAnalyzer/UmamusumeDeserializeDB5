using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static string umaNameReplace(string s)
        {
            StringBuilder ret = new StringBuilder(s);
            foreach (var key in Data.NameToId.Keys)
            {
                if (s.Contains(key) && TextData["6"].ContainsKey(Data.NameToId[key].ToString()))
                {
                    string trans = TextData["6"][Data.NameToId[key].ToString()];
                    ret.Replace(key + "の", trans + "的");
                    ret.Replace(key, trans);
                    break;
                }
            }
            return ret.ToString();
        }

        public static string skillNameReplace(string s)
        {
            // get skill name
            Match m = Regex.Match(s, @"「(.*?)」");
            if (m.Success)
            {
                string skillName = m.Groups[1].Value;
                if (Data.SkillNameToId.ContainsKey(skillName))
                {
                    long id = Data.SkillNameToId[skillName];
                    if (TextData["47"].ContainsKey(id.ToString()))
                    {
                        StringBuilder ret = new StringBuilder(s);
                        ret.Replace(skillName, skillName + "/" + TextData["47"][id.ToString()]);
                        return ret.ToString();
                    }
                }
            }
            return s;
        }

        public static string replaceAll(string s)
        {
            return staticReplace(skillNameReplace(umaNameReplace(s)));
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
