using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai
{
    public interface ISong
    {
        public int Id { get; }

        public string Name { get; }
        public ChartVersion Version { get; }

        public int AddVersionId { get; }
    }
}