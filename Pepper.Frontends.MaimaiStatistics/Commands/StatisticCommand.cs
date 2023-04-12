using Pepper.Commons.Structures;
using Pepper.Frontends.MaimaiStatistics.Database.ProgressRecordProvider;

namespace Pepper.Frontends.MaimaiStatistics.Commands
{
    public abstract class StatisticCommand : Command
    {
        protected readonly IProgressRecordProvider RecordProvider;
        protected StatisticCommand(IProgressRecordProvider progressRecordProvider)
        {
            RecordProvider = progressRecordProvider;
        }
    }
}