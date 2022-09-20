using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5
{
    public static class Data
    {
        public static List<TextData> TextData;
        public static Dictionary<string, long> NameToId;
        public static List<SupportCardData> SupportCardData;
        static Data()
        {
            TextData = new List<TextData>();
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
            NameToId = TextData.Where(x => x.index != 9100101 && x.index != 9101101).Where(x => (x.id == 4 && x.category == 4) || (x.id == 6 && x.category == 6) || (x.id == 75 && x.category == 75)).ToDictionary(x => x.text, x => x.index);

            SupportCardData = new();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from support_card_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var scd = new SupportCardData
                    {
                        id = (long)reader["id"],
                        chara_id = (long)reader["chara_id"],
                        rarity = (long)reader["rarity"],
                        effect_table_id = (long)reader["effect_table_id"],
                        unique_effect_id = (long)reader["unique_effect_id"],
                        command_type = (long)reader["command_type"],
                        command_id = (long)reader["command_id"],
                        support_card_type = (long)reader["support_card_type"]
                    };
                    SupportCardData.Add(scd);
                }
            }
        }
    }
    public struct SupportCardData
    {
        public long id;
        public long chara_id;
        public long rarity;
        public long effect_table_id;
        public long unique_effect_id;
        public long command_type;
        public long command_id;
        public long support_card_type;
    }
}
