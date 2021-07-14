using System;

namespace Pepper.Structures.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CategoryAttribute : Attribute
    {
        public string Category { get; }
        public CategoryAttribute(string category) => Category = category;
    }
}