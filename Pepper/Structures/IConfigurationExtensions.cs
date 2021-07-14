using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Pepper.Structures
{
    public static class ConfigurationExtensions
    {
        public static string[] GetCommandPrefixes(this IConfiguration configuration, string prefix)
        {
            var section = configuration.GetSection("command").GetSection("prefix").GetSection(prefix);
            return section.Exists() ? section.Get<Dictionary<string, string>>().Values.ToArray() : System.Array.Empty<string>();
        }

        public static Dictionary<string, string[]> GetAllCommandPrefixes(this IConfiguration configuration)
        {
            var children = configuration.GetSection("command").GetSection("prefix").GetChildren();
            return children.ToDictionary(
                section => section.Key,
                section => section.Get<Dictionary<string, string>>().Values.ToArray()
            );
        }
    }
}