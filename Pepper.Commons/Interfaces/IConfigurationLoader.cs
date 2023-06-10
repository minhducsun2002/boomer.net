using Pepper.Commons.Structures.Configuration;

namespace Pepper.Commons.Interfaces
{
    public interface IConfigurationLoader
    {
        public Task<GlobalConfiguration> Load(string path);
    }
}