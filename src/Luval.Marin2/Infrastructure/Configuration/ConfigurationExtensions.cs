using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.Marin2.Infrastructure.Configuration
{
    /// <summary>
    /// Extension methods for IConfiguration
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Retrieves the OAuthProvider configuration section from the appsettings.
        /// </summary>
        /// <param name="_config">The configuration instance.</param>
        /// <param name="name">The name of the OAuth provider.</param>
        /// <returns>An instance of OAuthProvider with the configuration values.</returns>
        public static OAuthProvider GetOAuthProvider(this IConfiguration _config, string name)
        {
            if(string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            var section = _config.GetSection($"OAuthProviders:{name}");
            return new OAuthProvider()
            {
                Name = name,
                ClientId = section["ClientId"] ?? "",
                ClientSecret = section["ClientSecret"] ?? ""
            };
        }
    }
}
