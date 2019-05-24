using System;

namespace IdentityServer4.Contrib.Caching.Redis.Extensions
{
    public static class StringExtensions
    {
        public static void EnsureParamter(this string value, string parameterName)
        {
            if (!string.IsNullOrWhiteSpace(value)) return;

            throw new ArgumentNullException(parameterName, $"{parameterName} must not be null or empty! Was: {value}");
        }
    }
}