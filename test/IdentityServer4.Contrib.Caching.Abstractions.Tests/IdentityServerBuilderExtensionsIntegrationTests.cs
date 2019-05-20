using System;
using IdentityServer4.Contrib.Caching.Abstractions.Configuration;
using IdentityServer4.Contrib.Caching.Abstractions.Tests.Misc;
using IdentityServer4.Contrib.Caching.TestInfrastructure;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace IdentityServer4.Contrib.Caching.Abstractions.Tests
{
    public class IdentityServerBuilderExtensionsIntegrationTests : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture serviceProviderFixture;

        public IdentityServerBuilderExtensionsIntegrationTests(ServiceProviderFixture serviceProviderFixture)
        {
            this.serviceProviderFixture = serviceProviderFixture;
        }

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Fake_Store_Invalid_Key_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                this.serviceProviderFixture.BuildServiceProviderWithCacheKey<DummyStore>(string.Empty));

            Assert.Throws<ArgumentNullException>(() =>
                this.serviceProviderFixture.BuildServiceProviderWithCacheKey<DummyStore>(" "));

            Assert.Throws<ArgumentNullException>(() =>
                this.serviceProviderFixture.BuildServiceProviderWithCacheKey<DummyStore>(null));
        }

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Fake_Store_Null_Option_Builder_Throws()
            => Assert.Throws<ArgumentNullException>(() =>
                this.serviceProviderFixture.BuildServiceProviderWithOptionBuilder<DummyStore>(null));

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Fake_Store_Types_Resolveable()
        {
            var provider = this.serviceProviderFixture.BuildDefaultServiceProvider<DummyStore>();

            var options = provider.GetRequiredService<IOptions<IdentityServerDistributedCacheConfiguration>>();
            Assert.NotNull(options.Value);
            Assert.NotNull(options.Value.CachingKeyPrefix);
            Assert.NotEmpty(options.Value.CachingKeyPrefix);
            provider.GetRequiredService<IPersistedGrantStore>();
        }

        [Fact]
        public void IdentityServerBuilderExtensions_Register_Fake_Store_Custom_Option_Builder_Types_Resolveable()
        {
            const string cachingPrefix = "SOME_PREFIX_VALUE";

            var provider = this.serviceProviderFixture.BuildServiceProviderWithCacheKey<DummyStore>(cachingPrefix);

            var options = provider.GetRequiredService<IOptions<IdentityServerDistributedCacheConfiguration>>();
            Assert.NotNull(options.Value);
            Assert.Equal(options.Value.CachingKeyPrefix, cachingPrefix);
            provider.GetRequiredService<IPersistedGrantStore>();
        }
    }
}