using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elders.RedLock;
using IdentityServer4.Contrib.Caching.Redis.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IdentityServer4.Contrib.Caching.Redis.Stores
{
    public class RedisCacheGrantStore : IPersistedGrantStore
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly RedisCacheGrantStoreConfiguration cacheGrantStoreConfiguration;
        private readonly IDistributedCache distributedCache;
        private readonly IRedisLockManager redisLockManager;

        public RedisCacheGrantStore(IOptions<RedisCacheGrantStoreConfiguration> options,
            IDistributedCache distributedCache, IRedisLockManager redisLockManager)
        {
            this.cacheGrantStoreConfiguration =
                options.Value ?? throw new ArgumentNullException(nameof(options),
                    $"{nameof(IOptions<RedisCacheGrantStoreConfiguration>)} must be configured!");

            this.distributedCache = distributedCache;
            this.redisLockManager = redisLockManager;
        }

        public virtual Task StoreAsync(PersistedGrant grant)
            => this.StoreInternalAsync(grant);

        public virtual Task<PersistedGrant> GetAsync(string key)
            => this.FindAsync(this.GetCombinedKey(key));

        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
            => await this.FindAllAsync(this.GetCombinedKey(subjectId, true));

        public virtual Task RemoveAsync(string key)
            => this.distributedCache.RemoveAsync(this.GetCombinedKey(key));

        public virtual async Task RemoveAllAsync(string subjectId, string clientId)
        {
            var combinedSubjectClientKey = this.GetCombinedKey(subjectId, clientId);
            var grants = await this.FindAllAsync(combinedSubjectClientKey);

            var enumeratedGrants = grants as PersistedGrant[] ?? grants.ToArray();

            var deletationTasks = enumeratedGrants
                .Where(grant => grant.ClientId == clientId)
                .Select(async grant =>
                {
                    await this.distributedCache.RemoveAsync(this.GetCombinedKey(grant.Key));
                    await this.distributedCache.RemoveAsync(combinedSubjectClientKey);
                });

            await Task.WhenAll(deletationTasks);
        }

        public virtual async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            var combinedSubjectClientTypeKey = this.GetCombinedKey(subjectId, clientId, type);
            var grants = await this.FindAllAsync(combinedSubjectClientTypeKey);

            var enumeratedGrants = grants as PersistedGrant[] ?? grants.ToArray();

            var deletationTasks = enumeratedGrants
                .Select(async grant =>
                {
                    await this.distributedCache.RemoveAsync(this.GetCombinedKey(grant.Key));
                    await this.distributedCache.RemoveAsync(combinedSubjectClientTypeKey);
                });

            await Task.WhenAll(deletationTasks);
        }

        protected virtual async Task<ICollection<PersistedGrant>> FindAllAsync(string key)
        {
            var bytes = await this.distributedCache.GetAsync(key);

            if (bytes == null) return new List<PersistedGrant>();

            var @string = Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject<ICollection<PersistedGrant>>(@string,
                RedisCacheGrantStore.SerializerSettings);
        }

        protected virtual async Task<PersistedGrant> FindAsync(string combinedKey)
        {
            var bytes = await this.distributedCache.GetAsync(combinedKey);

            if (bytes == null) return null;

            var @string = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<PersistedGrant>(@string,
                RedisCacheGrantStore.SerializerSettings);
        }

        protected virtual async Task StoreInternalAsync(PersistedGrant grant)
        {
            var savingTasks = new[]
            {
                this.StoreAsync(grant, this.GetCombinedKey(grant.Key)),
                this.AppendAsync(grant, this.GetCombinedKey(grant.SubjectId, true)),
                this.AppendAsync(grant, this.GetCombinedKey(grant.SubjectId, grant.ClientId)),
                this.AppendAsync(grant, this.GetCombinedKey(grant.SubjectId, grant.ClientId, grant.Type)),
            };

            await Task.WhenAll(savingTasks);
        }

        protected virtual Task StoreAsync(PersistedGrant grant, string key)
        {
            var cachingOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = grant.Expiration.GetValueOrDefault()
            };

            return this.distributedCache.SetAsync(key,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(grant,
                    RedisCacheGrantStore.SerializerSettings)), cachingOptions);
        }

        protected virtual async Task AppendAsync(PersistedGrant grant, string key)
        {
            await this.redisLockManager.LockAsync(key, TimeSpan.FromSeconds(5));

            var existingGrantsForSubject = await this.FindAllAsync(key);

            existingGrantsForSubject.Add(grant);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = grant.Expiration.GetValueOrDefault(DateTime.UtcNow.AddHours(1))
            };

            await this.redisLockManager.UnlockAsync(key);

            await this.distributedCache.SetAsync(key,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(existingGrantsForSubject,
                    RedisCacheGrantStore.SerializerSettings)), cacheOptions);
        }

        protected virtual string GetCombinedKey(string grantKey, bool isSubjectId = false)
        {
            var suffix = isSubjectId ? "subject" : "key";
            return $"{this.cacheGrantStoreConfiguration.CachingKeyPrefix}{suffix}_{grantKey}";
        }

        protected virtual string GetCombinedKey(string subjectId, string clientId)
            => $"{this.cacheGrantStoreConfiguration.CachingKeyPrefix}subject_{subjectId}_client_{clientId}";

        protected virtual string GetCombinedKey(string subjectId, string clientId, string type)
            => $"{this.cacheGrantStoreConfiguration.CachingKeyPrefix}subject_{subjectId}_client_{clientId}_type_{type}";
    }
}