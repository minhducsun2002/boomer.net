using Microsoft.EntityFrameworkCore;
using Pepper.Frontends.MaimaiStatistics.Structures;

namespace Pepper.Frontends.MaimaiStatistics.Database.ProgressRecordProvider
{
    public class MariaDbProgressRecordProvider : DbContext, IProgressRecordProvider
    {
        public MariaDbProgressRecordProvider(DbContextOptions<MariaDbProgressRecordProvider> o) : base(o) { }

        private DbSet<ProgressRecord> DbSet { get; set; } = null!;
        public async Task<IEnumerable<ProgressRecord>> ListMaxInRange(DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            var whereQuery = "where TRUE";
            if (fromDate is not null)
            {
                whereQuery += $" and timestamp >= '{fromDate.Value:u}'";
            }
            if (toDate is not null)
            {
                whereQuery += $" and timestamp <= '{toDate.Value:u}'";
            }
            var query = $@"
                select id, grouped_max.friend_id, class, name, rating, dan, min(timestamp) as timestamp from (
                      select friend_id, max(rating) as m from progress_log {whereQuery} group by friend_id
                ) grouped_max
                      join progress_log on grouped_max.friend_id = progress_log.friend_id and progress_log.rating = m
                group by grouped_max.friend_id;
            ";
            var res = DbSet.FromSqlRaw(query);
            return await res.ToListAsync();
        }

        public async Task<IEnumerable<ProgressRecord>> ListMaxAllTime()
        {
            var res = DbSet.FromSqlRaw(
                $@"
                select id, grouped_max.friend_id, class, name, rating, dan, latest_time as timestamp from (
                     select friend_id, max(timestamp) as latest_time from progress_log where TRUE group by friend_id
                 ) grouped_max
                     join progress_log on grouped_max.friend_id = progress_log.friend_id and progress_log.timestamp = latest_time
                group by grouped_max.friend_id;
                "
            );
            return await res.ToListAsync();
        }
    }
}