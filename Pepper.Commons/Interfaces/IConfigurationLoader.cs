namespace Pepper.Commons.Interfaces
{
    public interface IConfigurationLoader<TConfiguration>
    {
        public Task<TConfiguration> Load(string path);
    }
}