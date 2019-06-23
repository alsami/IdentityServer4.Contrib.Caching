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
        private static readonly RedLockOptions RedLockOptions = new RedLockOptions
        {
            LockRetryCount = 3,
            LockRetryDelay = TimeSpan.FromMilliseconds(100)
        };

        /// <summary>
        /// Register required services by passing the redis- configuration and instance-name. Optionally pass in a caching-prefix which has a default value.
        /// </summary>
        /// <param name="builder">The instance of the IIdentityServerBuilder</param>
        /// <param name="configuration">The redis-configuration</param>
        /// <param name="instanceName">The redis-connection instance name</param>
        /// <param name="cachingPrefix">Caching prefix for all values stored</param>
        /// <exception cref="ArgumentNullException">If any of the parameter is null, empty or whitespace.</exception>
        /// <returns>IIdentityServerBuilder</returns>
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

        /// <summary>
        /// Register required services by passing in required option-builder.
        /// </summary>
        /// <param name="builder">The instance of the IIdentityServerBuilder</param>
        /// <param name="redisCacheOptionsBuilder">Action to setup the cref="RedisCacheOptions"</param>
        /// <param name="redisCacheGrantStoreConfigurationBuilder">Action to setup the cref="RedisCacheGrantStoreConfiguration"</param>
        /// <returns>IIdentityServerBuilder</returns>
        /// <exception cref="ArgumentNullException">If any of the parameter is null.</exception>
        public static IIdentityServerBuilder AddDistributedRedisCache(this IIdentityServerBuilder builder,
            Action<RedisCacheOptions> redisCacheOptionsBuilder,
            Action<RedisCacheGrantStoreConfiguration> redisCacheGrantStoreConfigurationBuilder)
        {
            if (redisCacheOptionsBuilder == null)
                throw new ArgumentNullException(nameof(redisCacheOptionsBuilder), $"{nameof(redisCacheOptionsBuilder)} must not be null!");

            if (redisCacheGrantStoreConfigurationBuilder == null)
                throw new ArgumentNullException(nameof(redisCacheGrantStoreConfigurationBuilder),
                    $"{nameof(redisCacheGrantStoreConfigurationBuilder)} must not be null!");

            return builder.AddDistributedRedisCache(redisCacheOptionsBuilder, redisCacheGrantStoreConfigurationBuilder,
                options =>
                {
                    options.LockRetryDelay = IdentityServerBuilderExtensions.RedLockOptions.LockRetryDelay;
                    options.LockRetryCount = IdentityServerBuilderExtensions.RedLockOptions.LockRetryCount;
                    options.ClockDriveFactor = IdentityServerBuilderExtensions.RedLockOptions.ClockDriveFactor;
                });
        }

        /// <summary>
        /// Register required services by passing in required option-builder.
        /// </summary>
        /// <param name="builder">The instance of the IIdentityServerBuilder</param>
        /// <param name="redisCacheOptionsBuilder">Action to setup the cref="RedisCacheOptions"</param>
        /// <param name="redisCacheGrantStoreConfigurationBuilder">Action to setup the cref="RedisCacheGrantStoreConfiguration"</param>
        /// <param name="redLockOptionsBuilder">Action to setup the cref="RedLockOptions"</param>
        /// <returns>IIdentityServerBuilder</returns>
        /// <exception cref="ArgumentNullException">If any of the parameter is null.</exception>
        // ReSharper disable once MemberCanBePrivate.Global
        public static IIdentityServerBuilder AddDistributedRedisCache(this IIdentityServerBuilder builder,
            Action<RedisCacheOptions> redisCacheOptionsBuilder,
            Action<RedisCacheGrantStoreConfiguration> redisCacheGrantStoreConfigurationBuilder,
            Action<RedLockOptions> redLockOptionsBuilder)
        {
            if (redisCacheOptionsBuilder == null)
                throw new ArgumentNullException(nameof(redisCacheOptionsBuilder), $"{nameof(redisCacheOptionsBuilder)} must not be null!");

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
                .AddStackExchangeRedisCache(redisCacheOptionsBuilder);


            return builder;
        }

        private static IRedisLockManager CreateRedisLockManager(IServiceProvider provider)
        {
            var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

            var connection = string.IsNullOrWhiteSpace(options.Configuration)
                ? options.ConfigurationOptions.ToString(true)
                : options.Configuration;
            
            return new RedisLockManager(provider.GetRequiredService<IOptions<RedLockOptions>>().Value, connection);
        }
    }
}