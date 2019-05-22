using System;
using IdentityServer4.Contrib.Caching.Abstractions.Configuration;
using IdentityServer4.Contrib.Caching.Abstractions.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Contrib.Caching.Abstractions.Extensions
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder
            AddDistributedCacheGrantStore<TDistributedGrantStrore>(this IIdentityServerBuilder builder,
                string cachingPrefix = "_IdentityServer_Distributed_Caching_")
            where TDistributedGrantStrore : DistributedCacheGrantStoreService
        {
            cachingPrefix.EnsureParamter(nameof(cachingPrefix));

            builder.AddPersistedGrantStore<TDistributedGrantStrore>()
                .Services.Configure<IdentityServerDistributedCacheConfiguration>(options =>
                    options.CachingKeyPrefix = cachingPrefix);

            return builder;
        }

        public static IIdentityServerBuilder AddDistributedCacheGrantStore<TDistributedGrantStrore>(
            this IIdentityServerBuilder builder,
            Action<IdentityServerDistributedCacheConfiguration> optionsBuilder)
            where TDistributedGrantStrore : DistributedCacheGrantStoreService
        {
            if (optionsBuilder == null)
                throw new ArgumentNullException(nameof(optionsBuilder),
                    $"{nameof(optionsBuilder)} must not be null!");

            builder.AddPersistedGrantStore<TDistributedGrantStrore>()
                .Services.Configure(optionsBuilder);

            return builder;
        }
    }
}