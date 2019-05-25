# Dev Environment

## Running the tests

In order to run the tests, following user-secrets must be set and a redis-instance is required.

```
dotnet user-secrets set "RedisCacheOptions:Configuration" "<host-address>:<host-port>" --id IdentityServer4.Contrib.Caching
```

```
dotnet user-secrets set "RedisCacheOptions:InstanceName" "<your-db-name>" --id IdentityServer4.Contrib.Caching
```