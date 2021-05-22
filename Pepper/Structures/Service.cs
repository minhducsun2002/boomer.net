using System.Threading.Tasks;
using Serilog;

namespace Pepper.Structures
{
    public abstract class Service
    {
        public virtual async Task Initialize()
        {
            await Task.CompletedTask;
        }
    }
}