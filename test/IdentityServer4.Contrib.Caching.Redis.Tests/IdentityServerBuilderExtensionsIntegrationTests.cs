using System;
using System.Net;
using Elders.RedLock;
using IdentityServer4.Contrib.Caching.Redis.Configuration;
using IdentityServer4.Contrib.Caching.Redis.Tests.Misc;
using IdentityServer4.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Xunit;

namespace IdentityServer4.Contrib.Caching.Redis.Tests
{
    public class IdentityServerBuilderExtensionsIntegrationTests : IClassFixture<ServiceProviderFixture>,
        IClassFixture<ConfigurationFixture>
    {
        private readonly ServiceProviderFixture serviceProviderFixture;
        private readonly ConfigurationFixture configurationFixture;

        public IdentityServerBuilderExtensionsIntegrationTests(ServiceProviderFixture serviceProviderFixture,
            ConfigurationFixture configurationFixture)
        {
            this.serviceProviderFixture = serviceProviderFixture;
            this.configurationFixture = configurationFixture;
        }

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Null_Options_Builder_Throws()
            => Assert.Throws<ArgumentNullException>(() =>
                this.serviceProviderFixture.BuildDefaultServiceProvider((Action<RedisCacheOptions>) null));

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Types_Resolveable()
        {
            var provider =
                this.serviceProviderFixture.BuildDefaultServiceProvider(this.configurationFixture.RedisCacheOptions);

            provider.GetRequiredService<IOptions<RedisCacheGrantStoreConfiguration>>();
            provider.GetRequiredService<IOptions<RedisCacheOptions>>();
            provider.GetRequiredService<IOptions<RedLockOptions>>();
            provider.GetRequiredService<IDistributedCache>();
            provider.GetRequiredService<IPersistedGrantStore>();
            provider.GetRequiredService<IRedisLockManager>();
        }

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Types_Options_Builder_Resolveable()
        {
            var provider = this.serviceProviderFixture.BuildDefaultServiceProvider(options =>
            {
                options.Configuration = this.configurationFixture.RedisCacheOptions.Configuration;
                options.InstanceName = this.configurationFixture.RedisCacheOptions.InstanceName;
            });

            provider.GetRequiredService<IOptions<RedisCacheGrantStoreConfiguration>>();
            provider.GetRequiredService<IOptions<RedisCacheOptions>>();
            provider.GetRequiredService<IOptions<RedLockOptions>>();
            provider.GetRequiredService<IDistributedCache>();
            provider.GetRequiredService<IPersistedGrantStore>();
            provider.GetRequiredService<IRedisLockManager>();
        }
        
        [Fact]
        public void IdentityServerBuilderExtensions_Register_Types_Options_Builder_For_Connection_Resolveable()
        {
            var provider = this.serviceProviderFixture.BuildDefaultServiceProvider(options =>
            {
                options.InstanceName = this.configurationFixture.RedisCacheOptions.InstanceName;
                options.ConfigurationOptions = new ConfigurationOptions
                {
                    EndPoints =
                    {
                        new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6379)
                    }
                };
            });

            provider.GetRequiredService<IOptions<RedisCacheGrantStoreConfiguration>>();
            provider.GetRequiredService<IOptions<RedisCacheOptions>>();
            provider.GetRequiredService<IOptions<RedLockOptions>>();
            provider.GetRequiredService<IDistributedCache>();
            provider.GetRequiredService<IPersistedGrantStore>();
            provider.GetRequiredService<IRedisLockManager>();
        }
    }
}