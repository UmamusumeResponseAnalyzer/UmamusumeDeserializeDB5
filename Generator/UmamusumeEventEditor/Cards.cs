using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5.Generator.UmamusumeEventEditor
{
    internal class Cards : GeneratorBase
    {
        public void Generate()
        {
            var characterIds = new List<(long, long)>();
            var characters = new List<Character>();
            using var conn = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = UmamusumeDeserializeDB5.UmamusumeDatabaseFilePath }.ToString());
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from card_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    characterIds.Add(((long)reader["id"], (long)reader["default_rarity"]));
                }
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select * from card_rarity_data";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var ch = new Character
                    {
                        Id = (long)reader["card_id"],
                        Rarity = (long)reader["rarity"],
                        ProperDistance =
                        {
                            {"短",(long)reader["proper_distance_short"] },
                            {"英",(long)reader["proper_distance_mile"] },
                            {"中",(long)reader["proper_distance_middle"] },
                            {"长",(long)reader["proper_distance_long"] },
                        },
                        ProperRunningStyle =
                        {
                            {"逃",(long)reader["proper_running_style_nige"] },
                            {"先",(long)reader["proper_running_style_senko"] },
                            {"差",(long)reader["proper_running_style_sashi"] },
                            {"追",(long)reader["proper_running_style_oikomi"] },
                        },
                        ProperGround =
                        {
                            {"芝",(long)reader["proper_ground_turf"] },
                            {"泥",(long)reader["proper_ground_dirt"] }
                        }
                    };
                    var RaceDressId = (long)reader["race_dress_id"];
                    ch.RaceDressId = RaceDressId.ToString("D6");
                    characters.Add(ch);
                }
            }
            characterIds = characterIds.Where(x => characters.Any(y => y.Id == x.Item1)).ToList();
            var result = characterIds.Select(x => characters.First(y => x.Item1 == y.Id && x.Item2 == y.Rarity));

            Save("editor/editorcards", result);
        }
    }
    public class Character
    {
        public long Id;
        public long Rarity;
        public string RaceDressId;
        public Dictionary<string, long> ProperDistance = new();
        public Dictionary<string, long> ProperRunningStyle = new();
        public Dictionary<string, long> ProperGround = new();
    }
}
