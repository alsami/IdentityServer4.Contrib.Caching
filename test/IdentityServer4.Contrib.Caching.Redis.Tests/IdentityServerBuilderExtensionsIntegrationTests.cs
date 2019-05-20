using System;
using System.Reflection;
using IdentityServer4.Contrib.Caching.Abstractions.Configuration;
using IdentityServer4.Contrib.Caching.Redis.Tests.Misc;
using IdentityServer4.Stores;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

            provider.GetRequiredService<IOptions<IdentityServerDistributedCacheConfiguration>>();
            provider.GetRequiredService<IOptions<RedisCacheOptions>>();
            provider.GetRequiredService<IDistributedCache>();
            var store = provider.GetRequiredService<IPersistedGrantStore>();
            Assert.True(typeof(IPersistedGrantStore).IsAssignableFrom(store.GetType().GetTypeInfo()));
        }

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Types_Options_Builder_Resolveable()
        {
            var provider = this.serviceProviderFixture.BuildDefaultServiceProvider(options =>
            {
                options.Configuration = this.configurationFixture.RedisCacheOptions.Configuration;
                options.InstanceName = this.configurationFixture.RedisCacheOptions.InstanceName;
            });

            provider.GetRequiredService<IOptions<IdentityServerDistributedCacheConfiguration>>();
            provider.GetRequiredService<IOptions<RedisCacheOptions>>();
            provider.GetRequiredService<IDistributedCache>();
            var store = provider.GetRequiredService<IPersistedGrantStore>();
            Assert.True(typeof(IPersistedGrantStore).IsAssignableFrom(store.GetType().GetTypeInfo()));
        }
    }
}