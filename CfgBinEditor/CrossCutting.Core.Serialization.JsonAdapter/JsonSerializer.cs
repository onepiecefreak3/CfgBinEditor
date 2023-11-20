using CrossCutting.Core.Contract.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCutting.Core.Serialization.JsonAdapter
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj);

        public T Deserialize<T>(string serializedText) => JsonConvert.DeserializeObject<T>(serializedText);
    }
}
