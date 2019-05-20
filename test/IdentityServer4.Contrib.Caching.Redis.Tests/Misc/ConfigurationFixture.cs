using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;

namespace IdentityServer4.Contrib.Caching.Redis.Tests.Misc
{
    public class ConfigurationFixture
    {
        private readonly IConfiguration configuration;

        public ConfigurationFixture()
        {
            this.configuration = new ConfigurationBuilder()
                .AddUserSecrets("IdentityServer4.Contrib.Caching")
                .AddEnvironmentVariables()
                .Build();
        }

        public RedisCacheOptions RedisCacheOptions
            => this.configuration.GetSection(nameof(this.RedisCacheOptions))
                .Get<RedisCacheOptions>();
    }
}