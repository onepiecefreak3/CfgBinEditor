using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCutting.Core.Contract.Serialization
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);

        T Deserialize<T>(string serializedText);
    }
}