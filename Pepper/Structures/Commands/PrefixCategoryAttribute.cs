namespace Pepper.Structures.Commands
{
    public class PrefixCategoryAttribute : System.Attribute
    {
        public string PrefixCategory { get; }
        public PrefixCategoryAttribute(string prefixCategory) => PrefixCategory = prefixCategory;
    }
}