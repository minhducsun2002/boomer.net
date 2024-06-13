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
                select progress_log.id, grouped_max.friend_id, class, name, dan, m, progress_log.rating as rating, m as timestamp from (
                      select friend_id, max(timestamp) as m from progress_log {whereQuery} group by friend_id
                ) grouped_max
                    join progress_log on grouped_max.friend_id = progress_log.friend_id and progress_log.timestamp = m
                    join target_friend_id tfi on grouped_max.friend_id = tfi.friend_id
                where tfi.enabled = 1
                group by grouped_max.friend_id;
            ";
            var res = DbSet.FromSqlRaw(query);
            return await res.ToListAsync();
        }

        public async Task<IEnumerable<ProgressRecord>> ListMaxAllTime()
        {
            var res = DbSet.FromSqlRaw(
                $@"
                select progress_log.id, grouped_max.friend_id, class, name, rating, dan, latest_time as timestamp from (
                     select friend_id, max(timestamp) as latest_time from progress_log where TRUE group by friend_id
                 ) grouped_max
                     join progress_log on grouped_max.friend_id = progress_log.friend_id and progress_log.timestamp = latest_time
                     join target_friend_id tfi on grouped_max.friend_id = tfi.friend_id
                where tfi.enabled = 1
                group by grouped_max.friend_id;
                "
            );
            return await res.ToListAsync();
        }
    }
}