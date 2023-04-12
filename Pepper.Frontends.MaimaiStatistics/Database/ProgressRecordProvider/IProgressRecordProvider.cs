using Pepper.Frontends.MaimaiStatistics.Structures;

namespace Pepper.Frontends.MaimaiStatistics.Database.ProgressRecordProvider
{
    public interface IProgressRecordProvider
    {
        public Task<IEnumerable<ProgressRecord>> ListMaxInRange(DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null);

    }
}