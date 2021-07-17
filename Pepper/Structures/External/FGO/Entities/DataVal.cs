using System.Collections.Generic;

namespace Pepper.Structures.External.FGO.Entities
{
    public class DataVal : Dictionary<string, string>
    {
        public DataVal(IDictionary<string, string> dict) : base(dict) {}
    }
}