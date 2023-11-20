using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCutting.Core.Contract.Aspects
{
    [AttributeUsage(AttributeTargets.Constructor, Inherited = true)]
    public class RequestIdentifierAttribute : Attribute
    {
        public string PropertyName { get; }

        public RequestIdentifierAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
