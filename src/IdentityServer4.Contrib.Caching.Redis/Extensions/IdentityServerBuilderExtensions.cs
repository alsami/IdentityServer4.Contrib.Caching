using System;
using Elders.RedLock;
using IdentityServer4.Contrib.Caching.Redis.Configuration;
using IdentityServer4.Contrib.Caching.Redis.Stores;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Contrib.Caching.Redis.Extensions
{
    public static class IdentityServerBuilderExtensions
    {
        private static readonly RedLockOptions redLockOptions = new RedLockOptions
        {
            LockRetryCount = 3,
            LockRetryDelay = TimeSpan.FromMilliseconds(500)
        };

        public static IIdentityServerBuilder AddDistributedRedisCache(this IIdentityServerBuilder builder,
            string configuration, string instanceName, string cachingPrefix = "_IdentityServer_Distributed_Caching_")

        {
            configuration.EnsureParamter(nameof(configuration));
            instanceName.EnsureParamter(nameof(instanceName));
            cachingPrefix.EnsureParamter(nameof(cachingPrefix));

            return builder.AddDistributedRedisCache(options =>
            {
                options.InstanceName = instanceName;
                options.Configuration = configuration;
            }, options => options.CachingKeyPrefix = cachingPrefix);
        }

        public static IIdentityServerBuilder AddDistributedRedisCache(this IIdentityServerBuilder builder,
            Action<RedisCacheOptions> optionsBuilder,
            Action<RedisCacheGrantStoreConfiguration> redisCacheGrantStoreConfigurationBuilder)
        {
            if (optionsBuilder == null)
                throw new ArgumentNullException(nameof(optionsBuilder), $"{nameof(optionsBuilder)} must not be null!");

            if (redisCacheGrantStoreConfigurationBuilder == null)
                throw new ArgumentNullException(nameof(redisCacheGrantStoreConfigurationBuilder),
                    $"{nameof(redisCacheGrantStoreConfigurationBuilder)} must not be null!");

            builder
                .AddPersistedGrantStore<RedisCacheGrantStore>()
                .Services
                .Configure(redisCacheGrantStoreConfigurationBuilder)
                .Configure<RedLockOptions>(options =>
                {
                    options.LockRetryDelay = redLockOptions.LockRetryDelay;
                    options.LockRetryCount = redLockOptions.LockRetryCount;
                    options.ClockDriveFactor = redLockOptions.ClockDriveFactor;
                })
                .AddTransient(CreateRedisLockManager)
                .AddStackExchangeRedisCache(optionsBuilder);

            return builder;
        }

        public static IIdentityServerBuilder AddDistributedRedisCache(this IIdentityServerBuilder builder,
            Action<RedisCacheOptions> optionsBuilder,
            Action<RedisCacheGrantStoreConfiguration> redisCacheGrantStoreConfigurationBuilder,
            Action<RedLockOptions> redLockOptionsBuilder)
        {
            if (optionsBuilder == null)
                throw new ArgumentNullException(nameof(optionsBuilder), $"{nameof(optionsBuilder)} must not be null!");

            if (redisCacheGrantStoreConfigurationBuilder == null)
                throw new ArgumentNullException(nameof(redisCacheGrantStoreConfigurationBuilder),
                    $"{nameof(redisCacheGrantStoreConfigurationBuilder)} must not be null!");

            if (redLockOptionsBuilder == null)
                throw new ArgumentNullException(nameof(redLockOptionsBuilder),
                    $"{nameof(redLockOptionsBuilder)} must not be null!");

            builder
                .AddPersistedGrantStore<RedisCacheGrantStore>()
                .Services
                .Configure(redisCacheGrantStoreConfigurationBuilder)
                .Configure(redLockOptionsBuilder)
                .AddTransient(CreateRedisLockManager)
                .AddStackExchangeRedisCache(optionsBuilder);


            return builder;
        }

        private static IRedisLockManager CreateRedisLockManager(IServiceProvider provider)
            => new RedisLockManager(provider.GetRequiredService<IOptions<RedLockOptions>>().Value,
                provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value.Configuration);
    }
}