using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Data.SQLite;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using UmamusumeDeserializeDB5.Generator;

namespace UmamusumeDeserializeDB5
{
    public static class UmamusumeDeserializeDB5
    {
        public static readonly string UmamusumeDatabaseFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "../LocalLow", "Cygames", "umamusume", "master", "master.mdb");
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var stories = new Events().Generate();
            new SuccessEvent().Generate(stories);
            new SkillDataMgr().Generate();
            new CardName().Generate();
            new ClimaxItems().Generate();
            new TalentSkillSet().Generate();
            new FactorIds().Generate();
            //new SupportDataGenerator().Generate();

            Story.SerializeIsSupportCard = true;
            new Generator.UmamusumeEventEditor.Events().Generate(stories, new Events().GenerateSingleModeStoryData());
            new Generator.UmamusumeEventEditor.Cards().Generate();

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var i in Directory.EnumerateFiles("./output", "*.br", SearchOption.TopDirectoryOnly))
                {
                    zip.CreateEntryFromFile(i, Path.GetFileName(i));
                }
            }
            File.WriteAllBytes(@$"./output/数据v{DateTime.Now:yyMMddHHmmss}.zip", ms.ToArray());
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
        public bool IsSupportCard { get; set; }
        public List<List<Choice>> Choices { get; set; }

        [JsonIgnore]
        public static bool SerializeIsSupportCard { get; set; } = false;
        public bool ShouldSerializeIsSupportCard()
        {
            if (SerializeIsSupportCard)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
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