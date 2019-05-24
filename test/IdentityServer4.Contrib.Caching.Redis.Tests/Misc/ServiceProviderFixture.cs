using System;
using IdentityServer4.Contrib.Caching.Redis.Extensions;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Contrib.Caching.Redis.Tests.Misc
{
    public class ServiceProviderFixture
    {
        public IServiceProvider BuildDefaultServiceProvider(RedisCacheOptions options)
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddDistributedRedisCache(options.Configuration, options.InstanceName)
                .Services
                .BuildServiceProvider();

        public IServiceProvider BuildDefaultServiceProvider(Action<RedisCacheOptions> options)
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddDistributedRedisCache(options,
                    cacheOptions => cacheOptions.CachingKeyPrefix = "_IdentityServer_Redis_Cache_Grant_Store_")
                .Services
                .BuildServiceProvider();
    }
}