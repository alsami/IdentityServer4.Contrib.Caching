using IdentityServer4.Contrib.Caching.Abstractions.Configuration;
using IdentityServer4.Contrib.Caching.Abstractions.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Contrib.Caching.Abstractions.Tests.Misc
{
    public class DummyStore : DistributedCacheGrantStoreService
    {
        public DummyStore(IOptions<IdentityServerDistributedCacheConfiguration> options,
            IDistributedCache distributedCache) : base(options, distributedCache)
        {
        }
    }
}