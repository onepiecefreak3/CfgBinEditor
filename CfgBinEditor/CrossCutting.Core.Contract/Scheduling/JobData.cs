using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCutting.Core.Contract.Scheduling
{
    public sealed class JobData
    {
        public string Name { get; }
        public Guid Guid { get; }
        public object Object { get; }

        public JobData(string name, Guid guid) : this(name, guid, null) { }

        public JobData(string name, Guid guid, object obj)
        {
            Name = name;
            Guid = guid;
            Object = obj;
        }
    }
}
