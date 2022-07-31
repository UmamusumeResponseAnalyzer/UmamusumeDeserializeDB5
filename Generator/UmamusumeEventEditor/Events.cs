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
        public void Generate(List<Story> stories, List<SingleModeStoryData> singleModeStoryData)
        {
            var events = new EditorEvents();
            var grouped = stories.GroupBy(x => x.IsSupportCard).ToDictionary(x => x.Key, x => x.ToList());
            foreach (var i in grouped[true])
            {
            }
            events.Supports = grouped[true].GroupBy(x => (int)Data.NameToId[x.TriggerName]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());
            for (var i = 0; i < grouped[false].Count; ++i)
            {
            }
            events.Characters = grouped[false].Where(x => Data.NameToId.ContainsKey(x.TriggerName))
                .GroupBy(x => (int)Data.NameToId[x.TriggerName])
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());
            events.Characters.Add(0, grouped[false].Where(x => !Data.NameToId.ContainsKey(x.TriggerName) && x.TriggerName != "URA" && x.TriggerName != "青春杯" && x.TriggerName != "アオハル杯" && x.TriggerName != "巅峰杯" && x.TriggerName != "クライマックス").ToList());
            events.Characters.Add(1, grouped[false].Where(x => x.TriggerName == "URA").ToList());
            events.Characters.Add(2, grouped[false].Where(x => x.TriggerName == "青春杯" || x.TriggerName == "アオハル杯").ToList());
            events.Characters.Add(4, grouped[false].Where(x => x.TriggerName == "巅峰杯" || x.TriggerName == "クライマックス").ToList());

            Save("editor/editorevents", events);
        }

        public class EditorEvents
        {
            public Dictionary<int, List<Story>> Supports { get; set; } = new Dictionary<int, List<Story>>();
            public Dictionary<int, List<Story>> Characters { get; set; } = new Dictionary<int, List<Story>>();
        }
    }
}
