using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.IO;

namespace ReSource
{
    class JsonReader
    {        
        private string ReadAllText(String filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            } catch(IOException e)
            {
                throw;
            } 
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
