# [3.0.1](https://www.nuget.org/packages/IdentityServer4.Contrib.Caching.Redis/3.0.1) (2019-09-25)

## IdentityServer4.Contrib.Caching.Redis

### Features

* Update `IdentityServer4` to 3.0.1

# [3.0.0](https://www.nuget.org/packages/IdentityServer4.Contrib.Caching.Redis/3.0.0) (2019-09-23)

## IdentityServer4.Contrib.Caching.Redis

### Breaking changes

* .NET-Core 3 is now required!

### Features

* Support for .NET-Core 3!

# [1.1.0](https://www.nuget.org/packages/IdentityServer4.Contrib.Caching.Redis/1.1.0) (2019-07-19)

## IdentityServer4.Contrib.Caching.Redis

### Chore

* Update `IdentityServer4` to version 2.5.0

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



