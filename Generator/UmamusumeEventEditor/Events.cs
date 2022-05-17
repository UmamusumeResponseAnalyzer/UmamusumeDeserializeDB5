using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator.UmamusumeEventEditor
{
    internal class Events : GeneratorBase
    {
        public void Generate(List<Story> stories)
        {
            var TextData = new List<TextData>();
            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
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
            var NameToId = TextData.Where(x => (x.id == 4 && x.category == 4) || (x.id == 6 && x.category == 6) || (x.id == 75 && x.category == 75)).ToDictionary(x => x.text, x => x.index); //P-本名-S

            var events = new EditorEvents();
            var grouped = stories.GroupBy(x => x.IsSupportCard).ToDictionary(x => x.Key, x => x.ToList());
            foreach (var i in grouped[true])
            {
            }
            events.Supports = grouped[true].GroupBy(x => (int)NameToId[x.TriggerName]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());
            foreach (var i in grouped[false])
            {

            }
            events.Characters = grouped[false].Where(x => NameToId.ContainsKey(x.TriggerName)).GroupBy(x => (int)NameToId[x.TriggerName]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

            Save("editorevents", events);
        }

        public class EditorEvents
        {
            public Dictionary<int, List<Story>> Supports { get; set; } = new Dictionary<int, List<Story>>();
            public Dictionary<int, List<Story>> Characters { get; set; } = new Dictionary<int, List<Story>>();
        }
    }
}
