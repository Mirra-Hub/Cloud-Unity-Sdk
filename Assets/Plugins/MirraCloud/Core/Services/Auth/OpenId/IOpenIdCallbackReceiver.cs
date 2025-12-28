using System;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Core.Auth.OpenId
{
    internal interface IOpenIdCallbackReceiver : IDisposable
    {
        string SuccessUrl { get; }
        AsyncOperation<string> WaitForKeyAsync();
    }

    internal static class OpenIdCallbackUrlParser
    {
        private const string KeyParamName = "mirra_openid_key";

        public static bool TryGetOpenIdKey(string callbackUrl, out string openIdKey)
        {
            openIdKey = null;
            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                return false;
            }

            var questionIndex = callbackUrl.IndexOf('?');
            if (questionIndex < 0 || questionIndex == callbackUrl.Length - 1)
            {
                return false;
            }

            var query = callbackUrl.Substring(questionIndex + 1);
            var hashIndex = query.IndexOf('#');
            if (hashIndex >= 0)
            {
                query = query.Substring(0, hashIndex);
            }

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                if (string.IsNullOrEmpty(pair))
                {
                    continue;
                }

                var equalIndex = pair.IndexOf('=');
                if (equalIndex <= 0)
                {
                    continue;
                }

                var name = Uri.UnescapeDataString(pair.Substring(0, equalIndex));
                if (!string.Equals(name, KeyParamName, StringComparison.Ordinal))
                {
                    continue;
                }

                var value = Uri.UnescapeDataString(pair.Substring(equalIndex + 1));
                if (string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }

                openIdKey = value;
                return true;
            }

            return false;
        }
    }
}

