using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.Contrib.Caching.Redis.Stores;
using IdentityServer4.Contrib.Caching.Redis.Tests.Misc;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace IdentityServer4.Contrib.Caching.Redis.Tests
{
    public class RedisCacheGrantServiceIntegrationTests : IClassFixture<ConfigurationFixture>,
        IClassFixture<ServiceProviderFixture>
    {
        private readonly ConfigurationFixture configurationFixture;
        private readonly ServiceProviderFixture serviceProviderFixture;

        public RedisCacheGrantServiceIntegrationTests(ConfigurationFixture configurationFixture,
            ServiceProviderFixture serviceProviderFixture)
        {
            this.configurationFixture = configurationFixture;
            this.serviceProviderFixture = serviceProviderFixture;
        }

        [Fact]
        public async Task RedisCacheGrantStore_StoreAsync_Valid_Grant_Succeeds()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var grant = new PersistedGrant
            {
                ClientId = "1",
                CreationTime = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddHours(1),
                Key = Guid.NewGuid().ToString(),
                SubjectId = Guid.NewGuid().ToString(),
                Type = "Some-Type",
                Data = "Some-Data"
            };

            await cacheService.StoreAsync(grant);
        }

        [Fact]
        public async Task RedisCacheGrantStore_FindAsync_Valid_Key_Succeeds()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var grant = new PersistedGrant
            {
                ClientId = "1",
                CreationTime = DateTime.UtcNow,
                Expiration = DateTime.UtcNow.AddHours(1),
                Key = Guid.NewGuid().ToString(),
                SubjectId = Guid.NewGuid().ToString(),
                Type = "Some-Type",
                Data = "Some-Data"
            };

            await cacheService.StoreAsync(grant);

            var foundGrant = await cacheService.GetAsync(grant.Key);

            Assert.NotNull(foundGrant);
            Assert.Equal(GetBytes(Serialize(grant)), GetBytes(Serialize(foundGrant)));
        }

        [Fact]
        public async Task RedisCacheGrantStore_FindAsync_Invalid_Key_Returns_Null()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var foundGrant = await cacheService.GetAsync("some-irrelevant-key");

            Assert.Null(foundGrant);
        }

        [Fact]
        public async Task RedisCacheGrantStore_RemoveAsync_Valid_Key_Succeeds()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();

            var grants = new[]
            {
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = key1,
                    SubjectId = Guid.NewGuid().ToString(),
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = key2,
                    SubjectId = Guid.NewGuid().ToString(),
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
            };

            var storeTasks = grants.Select(async grant => await cacheService.StoreAsync(grant));

            await Task.WhenAll(storeTasks);

            await cacheService.RemoveAsync(key1);

            var grant2 = await cacheService.GetAsync(key2);

            Assert.NotNull(grant2);
            Assert.Equal(GetBytes(Serialize(grants[1])), GetBytes(Serialize(grant2)));
        }

        [Fact]
        public async Task RedisCacheGrantStore_RemoveAsync_Invalid_Key_Succeeds()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            await cacheService.RemoveAsync("some-irrelevant-key");
        }

        [Fact]
        public async Task RedisCacheGrantStore_GetAllAsync_Valid_SubjectId_Succeeds()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var subjectId1 = Guid.NewGuid().ToString();
            var subjectId2 = Guid.NewGuid().ToString();

            var grants = new[]
            {
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId1,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId1,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId1,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId2,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId2,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId1,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
            };

            await Task.WhenAll(grants.Select(async grant => await cacheService.StoreAsync(grant)));

            var foundGrants = await cacheService.GetAllAsync(subjectId1);

            var enumeratedGrants = foundGrants.OrderBy(x => x.Expiration).ToArray();

            Assert.NotEmpty(enumeratedGrants);
            Assert.Equal(4, enumeratedGrants.Length);

            var relevantStoreGrants = grants
                .Where(grant => grant.SubjectId == subjectId1)
                .OrderBy(g => g.Expiration)
                .ToArray();
            Assert.Equal(GetBytes(Serialize(relevantStoreGrants)), GetBytes(Serialize(enumeratedGrants)));
        }

        [Fact]
        public async Task RedisCacheGrantStore_GetAllAsync_Invalid_SubjectId_Returns_Empty_Enumeration()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();


            var foundGrants = await cacheService.GetAllAsync("some-key-that-is-not-relevant");
            Assert.NotNull(foundGrants);
            Assert.Empty(foundGrants);
        }

        [Fact]
        public async Task RedisCacheGrantStore_GetAsync_Valid_Key_Succeeds()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var grant = new PersistedGrant
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = Guid.NewGuid().ToString(),
                ClientId = "1",
                Data = "some-data",
                Expiration = DateTime.UtcNow.AddHours(1),
                CreationTime = DateTime.UtcNow,
                Type = "some-type"
            };

            await cacheService.StoreAsync(grant);

            var foundGrant = await cacheService.GetAsync(grant.Key);

            Assert.NotNull(foundGrant);
            Assert.Equal(GetBytes(Serialize(grant)), GetBytes(Serialize(foundGrant)));
        }

        [Fact]
        public async Task RedisCacheGrantStore_GetAsync_Invalid_Key_Returns_Null()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var foundGrant = await cacheService.GetAsync("some-key-not-relevant-123324-google-facebook-instagram");

            Assert.Null(foundGrant);
        }

        [Fact]
        public async Task RedisCacheGrantStore_RemoveAllAsync_SubjectId_ClientId_Removes_Only_Wanted()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var subjectId = Guid.NewGuid().ToString();

            var grantsToStore = new[]
            {
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "2",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Type = "Some-Type",
                    Data = "Some-Data"
                },
            };

            await Task.WhenAll(grantsToStore.Select(async grant => await cacheService.StoreAsync(grant)));

            await cacheService.RemoveAllAsync(subjectId, "1");

            var foundGrants = await cacheService.GetAllAsync(subjectId);
            var enumeratedGrants = foundGrants.Where(grant => grant.ClientId != "1").ToArray();
            Assert.NotEmpty(enumeratedGrants);
            Assert.Single(enumeratedGrants);
            Assert.Equal(GetBytes(Serialize(grantsToStore[2])), GetBytes(Serialize(enumeratedGrants.First())));
        }

        [Fact]
        public async Task RedisCacheGrantStore_RemoveAllAsync_SubjectId_ClientId_Type_Removes_Only_Wanted()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            var cacheService = provider.GetRequiredService<IPersistedGrantStore>();

            var subjectId = Guid.NewGuid().ToString();
            const string tokenType = "token";
            const string refreshTokenType = "refreshToken";

            var grantsToStore = new[]
            {
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Data = "Some-Data",
                    Type = tokenType
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Data = "Some-Data",
                    Type = tokenType
                },
                new PersistedGrant
                {
                    ClientId = "1",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Data = "Some-Data",
                    Type = tokenType
                },
                new PersistedGrant
                {
                    ClientId = "2",
                    CreationTime = DateTime.UtcNow,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Key = Guid.NewGuid().ToString(),
                    SubjectId = subjectId,
                    Data = "Some-Data",
                    Type = refreshTokenType
                },
            };

            await Task.WhenAll(grantsToStore.Select(async grant => await cacheService.StoreAsync(grant)));

            await cacheService.RemoveAllAsync(subjectId, "1", tokenType);

            var foundGrantTypes = await cacheService.GetAllAsync(subjectId);

            var enumeratedGrants = foundGrantTypes
                .Where(grant => grant.ClientId != "1" && grant.Type != tokenType)
                .ToArray();

            Assert.NotEmpty(enumeratedGrants);
            Assert.Single(enumeratedGrants);
            Assert.Equal(GetBytes(Serialize(grantsToStore[3])), GetBytes(Serialize(enumeratedGrants.First())));
        }

        private static string Serialize(object @object) =>
            JsonConvert.SerializeObject(@object, RedisCacheGrantStore.SerializerSettings);

        private static byte[] GetBytes(string value) => Encoding.UTF8.GetBytes(value);
    }
}