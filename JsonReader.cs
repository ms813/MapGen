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
        private String ReadAllText(String filePath)
        {
            return System.IO.File.ReadAllText(filePath);
        }

        public T ReadJson<T>(String filePath)
        {
            T json = default(T);
            try
            {
                json = JsonConvert.DeserializeObject<T>(ReadAllText(filePath));
            }
            catch (JsonException e)
            {
                Console.Write(e.Message);
            }

            return json;
            
        }

        public List<T> ReadJsonArray<T>(String filePath)
        {         
            return JsonConvert.DeserializeObject<List<T>>(ReadAllText(filePath));
        }
    }
}
