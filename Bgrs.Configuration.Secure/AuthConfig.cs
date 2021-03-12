using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace Bgrs.Configuration.Secure
{
    public class AuthConfig
    {
        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string Authority
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, Instance, TenantId);
            }
        }
        public string ClientSecret { get; set; }
        public string BaseAddress { get; set; }
        public string Endpoint { get; set; }
        public string ResourceID { get; set; }

        public string EndpointAddress => $"{BaseAddress}{Endpoint}";

        public static AuthConfig ReadFromJsonFile(string path)
        {
            IConfiguration Configuration;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path);

            Configuration = builder.Build();

            return Configuration.Get<AuthConfig>();
        }

        public AuthenticationResult authenticationResult { get; set; }

        /// <summary>
        /// Based on the properties in this object, the identity provider is contacted
        /// to get an access token.
        /// </summary>
        /// <returns>JWT providing authorization to access the specified resource</returns>
        public async Task<string> GetAccessToken()
        {
            string AcquiredAccessToken = "";

            if (authenticationResult == null)
            {
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(ClientId)
                    .WithClientSecret(ClientSecret)
                    .WithAuthority(new Uri(Authority))
                    .Build();

                try
                {
                    string[] ResourceIds = new string[] { ResourceID };

                    authenticationResult = await app.AcquireTokenForClient(ResourceIds).ExecuteAsync();
                    AcquiredAccessToken = authenticationResult.AccessToken;
                }
                catch (MsalClientException ex)
                {
                }
            }
            else
            {
                AcquiredAccessToken = authenticationResult.AccessToken;
            }

            return AcquiredAccessToken;
        }
    }
}
