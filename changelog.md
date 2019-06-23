# [1.0.2](https://www.nuget.org/packages/IdentityServer4.Contrib.Caching.Redis/1.0.2) (2019-06-23)

## IdentityServer4.Contrib.Caching.Redis

### Features

* Update `RedLock` to version 4.0.1
* Change default retry-delay of `RedLockOptions` to `100ms`

# [1.0.1](https://www.nuget.org/packages/IdentityServer4.Contrib.Caching.Redis/1.0.1) (2019-05-31)

## IdentityServer4.Contrib.Caching.Redis

### Fixes

* When connection-string is not available, the cache-options are used to build the connection-string. Fixes #1

# [1.0.0](https://www.nuget.org/packages/IdentityServer4.Contrib.Caching.Redis/1.0.0) (2019-05-25)

## IdentityServer4.Contrib.Caching.Redis

### Initial release

This is the first official release that uses `IDistributedCache` with the implementation of [Microsoft.Extensions.Caching.StackExchangeRedis](https://www.nuget.org/packages/Microsoft.Extensions.Caching.StackExchangeRedis/2.2.5) for storing grants of `IdentityServer4` in a distributed fashion. For more information on how to install and use this package, please checkout the [readme](readme.md).



