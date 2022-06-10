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
            NameToId = TextData.Where(x => (x.id == 4 && x.category == 4) || (x.id == 6 && x.category == 6) || (x.id == 75 && x.category == 75)).ToDictionary(x => x.text, x => x.index); //P-本名-S
        }
    }
}
