using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ReSource
{
    class JsonReader
    {
        private String filePath;
        public JsonReader(String filePath)
        {
            this.filePath = filePath;
        }

        public String ReadAllText()
        {
            return System.IO.File.ReadAllText(filePath);
        }

        public dynamic ReadJson()
        {
            return JsonConvert.DeserializeObject(ReadAllText());
        }

        public List<T> ReadJson<T>()
        {           

            return JsonConvert.DeserializeObject<List<T>>(ReadAllText());
        }
    }
}
