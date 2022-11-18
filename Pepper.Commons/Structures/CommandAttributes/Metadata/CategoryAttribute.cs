using System;

namespace Pepper.Commons.Structures.CommandAttributes.Metadata
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CategoryAttribute : Attribute
    {
        public string Category { get; }
        public CategoryAttribute(string category) => Category = category;
    }
}