using IdentityServer4.Contrib.Caching.Abstractions.Configuration;
using IdentityServer4.Contrib.Caching.Abstractions.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Contrib.Caching.Redis.Stores
{
    public sealed class RedisCacheGrantStoreService : DistributedCacheGrantStoreService
    {
        public RedisCacheGrantStoreService(IOptions<IdentityServerDistributedCacheConfiguration> options,
            IDistributedCache distributedCache) : base(options, distributedCache)
        {
        }
    }
}