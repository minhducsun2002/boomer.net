using System;

namespace Pepper.Structures.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PrefixCategoryAttribute : Attribute
    {
        public string PrefixCategory { get; }
        public PrefixCategoryAttribute(string prefixCategory) => PrefixCategory = prefixCategory;
    }
}