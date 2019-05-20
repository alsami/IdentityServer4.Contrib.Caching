using System;
using IdentityServer4.Contrib.Caching.Abstractions.Extensions;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Contrib.Caching.Redis.Extensions
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddDistributedRedisCache(this IIdentityServerBuilder builder,
            string configuration, string instanceName)

        {
            configuration.EnsureParamter(nameof(configuration));
            instanceName.EnsureParamter(nameof(instanceName));

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = instanceName;
                options.Configuration = configuration;
            });

            return builder;
        }

        public static IIdentityServerBuilder AddDistributedRedisCache(this IIdentityServerBuilder builder,
            Action<RedisCacheOptions> optiosBuilder)
        {
            if (optiosBuilder == null)
                throw new ArgumentNullException(nameof(optiosBuilder), $"{nameof(optiosBuilder)} must not be null!");

            builder.Services.AddStackExchangeRedisCache(optiosBuilder);

            return builder;
        }
    }
}