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
        public void Save(string filename, object content)
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
            File.WriteAllBytes(@$"./output/{filename}.br", Brotli.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content))));
            File.WriteAllText(@$"./output/json/{filename}.json", JsonConvert.SerializeObject(content, Formatting.Indented));
        }
    }
}
