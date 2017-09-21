using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneRosterProviderDemo.Exceptions
{
    public abstract class OneRosterException : Exception
    {
        public abstract void AsJson(JsonWriter writer, string operation);
    }
}
