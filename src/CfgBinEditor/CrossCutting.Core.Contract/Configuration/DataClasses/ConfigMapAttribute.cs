using System;

namespace CrossCutting.Core.Contract.Configuration.DataClasses
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigMapAttribute : Attribute
    {
        public string Category { get; }
        public string[] Keys { get; }
        public bool Persist { get; }

        public ConfigMapAttribute(string category, string key, bool persist = false)
            : this(category, new[] { key }, persist)
        {
        }

        public ConfigMapAttribute(string category, string[] keys, bool persist = false)
        {
            Category = category;
            Keys = keys;
            Persist = persist;
        }
    }
}
