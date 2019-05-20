using System;
using IdentityServer4.Contrib.Caching.Abstractions.Configuration;
using IdentityServer4.Contrib.Caching.Abstractions.Extensions;
using IdentityServer4.Contrib.Caching.Abstractions.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IdentityServer4.Contrib.Caching.TestInfrastructure
{
    public class ServiceProviderFixture
    {
        public Mock<IDistributedCache> CreateDistributedCacheMock() => new Mock<IDistributedCache>(MockBehavior.Loose);


        public IServiceProvider BuildServiceProviderWithCacheKey<TDistributedCache>(string key,
            IDistributedCache cache = null)
            where TDistributedCache : DistributedCacheGrantStoreService
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddDistributedCacheGrantStore<TDistributedCache>(key)
                .Services.AddSingleton(cache ?? this.CreateDistributedCacheMock().Object)
                .BuildServiceProvider();

        public IServiceProvider BuildServiceProviderWithOptionBuilder<TDistributedCache>(
            Action<IdentityServerDistributedCacheConfiguration> builder,
            IDistributedCache cache = null)
            where TDistributedCache : DistributedCacheGrantStoreService
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddDistributedCacheGrantStore<TDistributedCache>(builder)
                .Services.AddSingleton(cache ?? this.CreateDistributedCacheMock().Object)
                .BuildServiceProvider();

        public IServiceProvider BuildDefaultServiceProvider<TDistributedCache>(IDistributedCache cache = null)
            where TDistributedCache : DistributedCacheGrantStoreService
            => new ServiceCollection()
                .AddIdentityServerBuilder()
                .AddDistributedCacheGrantStore<TDistributedCache>()
                .Services.AddSingleton(cache ?? this.CreateDistributedCacheMock().Object)
                .BuildServiceProvider();
    }
}