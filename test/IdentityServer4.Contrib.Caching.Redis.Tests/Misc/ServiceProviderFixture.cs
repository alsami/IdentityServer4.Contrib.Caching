using System;
using Elders.RedLock;
using IdentityServer4.Contrib.Caching.Redis.Configuration;
using IdentityServer4.Contrib.Caching.Redis.Extensions;
using IdentityServer4.Contrib.Caching.Redis.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Moq;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace IdentityServer4.Contrib.Caching.Redis.Tests.Misc
{
    public class ServiceProviderFixture
    {
        public Mock<IDistributedCache> CreateDistributedCacheMock() => new Mock<IDistributedCache>();
        
        public Mock<IRedisLockManager> CreateRedisLockManagerMock() => new Mock<IRedisLockManager>();

        public IServiceProvider BuildMockServiceProvider(IDistributedCache cache, IRedisLockManager redisLockManager)
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddPersistedGrantStore<RedisCacheGrantStore>()
                .Services
                .AddSingleton(cache)
                .AddSingleton(redisLockManager)
                .Configure<RedisCacheGrantStoreConfiguration>(options => options.CachingKeyPrefix = "TEST_PREFIX")
                .BuildServiceProvider();
        
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