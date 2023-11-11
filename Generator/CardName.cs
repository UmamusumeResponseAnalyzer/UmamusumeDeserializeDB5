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
                    list.Add(new((long)reader["index"], (string)reader["text"]));
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
                    list.Add(new SupportCardName(id, Data.TextData.First(x => x.category == 76 && x.index == id).text, command_id, chara_id));
                }
            }
            // 马名
            foreach (var i in Data.TextData.Where(x => x.id == 5).ToDictionary(x => x.index, x => x.text))
            {
                list.Add(new UmaName(i.Key, i.Value));
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
