using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace TestAsyncConsole
{
    public static class Configuration
    {
        private static readonly IConfigurationRoot _config;
        static Configuration()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddUserSecrets(Assembly.GetExecutingAssembly());

            _config = builder.Build();
        }

        public static string Get(string sectionKey) => _config.GetSection(sectionKey).Value!;
    }
}