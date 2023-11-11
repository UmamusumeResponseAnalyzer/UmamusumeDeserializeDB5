using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmamusumeDeserializeDB5
{
    internal class GeneratorBase
    {
        public void Save(string filename, object content, bool typename = false)
        {
            Directory.CreateDirectory("./output");
            Directory.CreateDirectory("./output/json/");
            filename = filename.Replace("\\", "/");
            if (filename.Contains("/"))
            {
                foreach (var i in filename.Split("/")[..1])
                {
                    Directory.CreateDirectory(@$"./output/{i}/");
                    Directory.CreateDirectory(@$"./output/json/{i}/");
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
            File.WriteAllBytes(@$"./output/{filename}.br", Brotli.Compress(Encoding.UTF8.GetBytes(text)));
            File.WriteAllText(@$"./output/json/{filename}.json", JsonConvert.SerializeObject(JsonConvert.DeserializeObject(text), Formatting.Indented));
        }
    }
}
