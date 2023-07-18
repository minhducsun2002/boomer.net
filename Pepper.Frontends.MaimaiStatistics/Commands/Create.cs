using Pepper.Frontends.MaimaiStatistics.Database.ProgressRecordProvider;

namespace Pepper.Frontends.MaimaiStatistics.Commands
{
    public class Create : StatisticCommand
    {
        public Create(IProgressRecordProvider p) : base(p) { }
    }
}