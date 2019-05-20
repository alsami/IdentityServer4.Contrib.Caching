using System;
using IdentityServer4.Contrib.Caching.Abstractions.Extensions;
using IdentityServer4.Contrib.Caching.Redis.Extensions;
using IdentityServer4.Contrib.Caching.Redis.Stores;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Contrib.Caching.Redis.Tests.Misc
{
    public class ServiceProviderFixture
    {
        public IServiceProvider BuildDefaultServiceProvider(RedisCacheOptions options)
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddDistributedCacheGrantStore<RedisCacheGrantStoreService>()
                .AddDistributedRedisCache(options.Configuration, options.InstanceName)
                .Services
                .BuildServiceProvider();

        public IServiceProvider BuildDefaultServiceProvider(Action<RedisCacheOptions> options)
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddDistributedCacheGrantStore<RedisCacheGrantStoreService>()
                .AddDistributedRedisCache(options)
                .Services
                .BuildServiceProvider();
    }
}