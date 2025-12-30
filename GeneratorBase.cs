using Newtonsoft.Json;
using System.Text;

namespace UmamusumeDeserializeDB5
{
    internal class GeneratorBase
    {
        public void Save(string filename, object content, bool typename = false)
        {
            Directory.CreateDirectory("./json/");
            filename = filename.Replace("\\", "/");
            if (filename.Contains("/"))
            {
                foreach (var i in filename.Split("/")[..1])
                {
                    Directory.CreateDirectory(@$"./{i}/");
                    Directory.CreateDirectory(@$"./json/{i}/");
                }
            }
            string text;
            if (typename)
            {
                text = JsonConvert.SerializeObject(content, typename ? new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All } : null);
                text = text.Replace("UmamusumeDeserializeDB5", "UmamusumeResponseAnalyzer");
            }
            else
            {
                text = JsonConvert.SerializeObject(content);
            }
            File.WriteAllBytes(@$"./{filename}.br", Brotli.Compress(Encoding.UTF8.GetBytes(text)));
            File.WriteAllText(@$"./json/{filename}.json", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(text), Formatting.Indented));
        }
    }
}
