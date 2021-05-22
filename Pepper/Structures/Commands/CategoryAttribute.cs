namespace Pepper.Structures.Commands
{
    public class CategoryAttribute : System.Attribute
    {
        public string Category { get; }
        public CategoryAttribute(string category) => Category = category;
    }
}