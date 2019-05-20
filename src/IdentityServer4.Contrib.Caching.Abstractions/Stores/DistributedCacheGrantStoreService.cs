using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Contrib.Caching.Abstractions.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable VirtualMemberNeverOverridden.Global

namespace IdentityServer4.Contrib.Caching.Abstractions.Stores
{
    public abstract class DistributedCacheGrantStoreService : IPersistedGrantStore
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly IdentityServerDistributedCacheConfiguration cacheConfiguration;
        private readonly IDistributedCache distributedCache;

        protected DistributedCacheGrantStoreService(IOptions<IdentityServerDistributedCacheConfiguration> options,
            IDistributedCache distributedCache)
        {
            this.cacheConfiguration =
                options.Value ?? throw new ArgumentNullException(nameof(options),
                    $"{nameof(IOptions<IdentityServerDistributedCacheConfiguration>)} must be configured!");

            this.distributedCache = distributedCache;
        }

        public virtual async Task StoreAsync(PersistedGrant grant)
        {
            var storingTasks = new[]
            {
                this.StoreAsync(grant, grant.Key),
                this.AppendAsync(grant, this.GetCombineSubjectKey(grant.SubjectId))
            };

            await Task.WhenAll(storingTasks);
        }

        public virtual Task<PersistedGrant> GetAsync(string key) => this.FindAsync(key);

        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
            => await this.FindAllAsync(this.GetCombineSubjectKey(subjectId));

        public virtual Task RemoveAsync(string key) => this.distributedCache.RemoveAsync(key);

        public virtual async Task RemoveAllAsync(string subjectId, string clientId)
        {
            var grants = await this.GetAllAsync(subjectId);

            var enumeratedGrants = grants as PersistedGrant[] ?? grants.ToArray();
            
            var deletationTasks = enumeratedGrants
                .Where(grant => grant.ClientId == clientId)
                .Select(async grant =>
                {
                    await this.distributedCache.RemoveAsync(grant.Key);
                }).ToList();
            
            var combinedSubjectKey = this.GetCombineSubjectKey(subjectId);

            deletationTasks.Add(this.distributedCache.RemoveAsync(combinedSubjectKey));

            await Task.WhenAll(deletationTasks);
            
            var grantsToStore = enumeratedGrants
                .Where(grant => grant.ClientId != clientId)
                .ToArray();

            foreach (var grant in grantsToStore)
            {
                await this.AppendAsync(grant, combinedSubjectKey);
            }
        }

        public virtual async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            var grants = await this.GetAllAsync(subjectId);
            
            var enumeratedGrants = grants as PersistedGrant[] ?? grants.ToArray();

            var deletationTasks = enumeratedGrants
                .Where(grant => grant.ClientId == clientId && grant.Type == type)
                .Select(async grant =>
                {
                    await this.distributedCache.RemoveAsync(grant.Key);
                })
                .ToList();
            
            var combinedSubjectKey = this.GetCombineSubjectKey(subjectId);
            
            deletationTasks.Add(this.distributedCache.RemoveAsync(combinedSubjectKey));

            await Task.WhenAll(deletationTasks);
            
            var grantsToStore = enumeratedGrants
                .Where(grant => grant.ClientId != clientId && grant.Type != type)
                .ToArray();

            foreach (var grant in grantsToStore)
            {
                await this.AppendAsync(grant, combinedSubjectKey);
            }
        }

        protected virtual async Task<ICollection<PersistedGrant>> FindAllAsync(string key)
        {
            var bytes = await this.distributedCache.GetAsync(key);

            if (bytes == null) return new List<PersistedGrant>();

            var @string = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<ICollection<PersistedGrant>>(@string,
                DistributedCacheGrantStoreService.SerializerSettings);
        }

        protected virtual async Task<PersistedGrant> FindAsync(string key)
        {
            var bytes = await this.distributedCache.GetAsync(key);

            if (bytes == null) return null;

            var @string = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<PersistedGrant>(@string,
                DistributedCacheGrantStoreService.SerializerSettings);
        }

        protected virtual Task StoreAsync(PersistedGrant grant, string key)
        {
            var cachingOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = grant.Expiration.GetValueOrDefault()
            };
            
            return this.distributedCache.SetAsync(key,
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(grant,
                DistributedCacheGrantStoreService.SerializerSettings)), cachingOptions);
        }
            

        protected virtual async Task AppendAsync(PersistedGrant grant, string key)
        {
            var existingGrantsForSubject = await this.FindAllAsync(key);

            existingGrantsForSubject.Add(grant);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = grant.Expiration.GetValueOrDefault()
            };
            
            await this.distributedCache.SetAsync(key,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(existingGrantsForSubject, DistributedCacheGrantStoreService.SerializerSettings)), cacheOptions);
        }

        protected string GetCombineSubjectKey(string subjectId)
            => $"{this.cacheConfiguration.CachingKeyPrefix}{subjectId}";
    }
}