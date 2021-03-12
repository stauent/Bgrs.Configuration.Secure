using System;
using System.IO;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Linq;

namespace Bgrs.Configuration.Secure
{
    public interface IAuthConfig
    {
        string Instance { get; set; }
        string TenantId { get; set; }
        string ClientId { get; set; }
        string Authority { get; }
        string ClientSecret { get; set; }
        string BaseAddress { get; set; }
        string Endpoint { get; set; }
        string ResourceID { get; set; }
        string EndpointAddress { get; }
        AuthenticationResult authenticationResult { get; set; }

        /// <summary>
        /// Current JWT being used as the bearer token
        /// </summary>
        string JWT { get; set; }

        /// <summary>
        /// Based on the properties in this object, the identity provider is contacted
        /// to get an access token.
        /// </summary>
        /// <param name="ForceRenew">Bool indicating if we should ask for a new JWT. If true, a new one is produced no matter what.</param>
        /// <returns>JWT providing authorization to access the specified resource</returns>
        Task<string> GetAccessToken(bool ForceRenew = false);

        /// <summary>
        /// Gets a JWT to authorize the caller to access the specified API.
        /// A HttpClient is created and the "Authorization" header is set
        /// to be a bearer token. All calls with this HttpClient will contain this header.
        /// </summary>
        /// <returns>HttpClient authorized to call the specified API</returns>
        Task<HttpClient> GetAuthorizedClient();
    }

    public class AuthConfig : IAuthConfig
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
        /// Current JWT being used as the bearer token
        /// </summary>
        public string JWT { get; set; }


        /// <summary>
        /// Based on the properties in this object, the identity provider is contacted
        /// to get an access token.
        /// </summary>
        /// <param name="ForceRenew">Bool indicating if we should ask for a new JWT. If true, a new one is produced no matter what.</param>
        /// <returns>JWT providing authorization to access the specified resource</returns>
        public async Task<string> GetAccessToken(bool ForceRenew = false)
        {
            if (authenticationResult == null || ForceRenew)
            {
                IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(ClientId)
                    .WithClientSecret(ClientSecret)
                    .WithAuthority(new Uri(Authority))
                    .Build();

                try
                {
                    string[] ResourceIds = new string[] { ResourceID };

                    authenticationResult = await app.AcquireTokenForClient(ResourceIds).ExecuteAsync();
                    JWT = authenticationResult.AccessToken;
                }
                catch (MsalClientException ex)
                {
                }
            }
            else
            {
                JWT = authenticationResult.AccessToken;
            }

            return JWT;
        }

        /// <summary>
        /// Gets a JWT to authorize the caller to access the specified API.
        /// A HttpClient is created and the "Authorization" header is set
        /// to be a bearer token. All calls with this HttpClient will contain this header.
        /// </summary>
        /// <returns>HttpClient authorized to call the specified API</returns>
        public async Task<HttpClient> GetAuthorizedClient()
        {
            HttpClient httpClient = new HttpClient();
            HttpRequestHeaders defaultRequestHeaders = httpClient.DefaultRequestHeaders;

            if (defaultRequestHeaders.Accept == null ||
                !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new
                    MediaTypeWithQualityHeaderValue("application/json"));
            }

            string accessToken = await GetAccessToken();
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            return(httpClient);
        }
    }


    public interface IAuthorizedClientDetails
    {
        string apiSecretName { get; set; }
        IApplicationSecretsConnectionStrings secret { get; set; }
        IAuthConfig authConfig { get; set; }
        HttpClient authorizedApiClient { get; set; }
    }

    public class AuthorizedClientDetails : IAuthorizedClientDetails
    {
        public AuthorizedClientDetails()
        {

        }
        public string apiSecretName { get; set; }
        public IApplicationSecretsConnectionStrings secret { get; set; }
        public IAuthConfig authConfig { get; set; }
        public HttpClient authorizedApiClient { get; set; }
    }

    public static class AuthorizedApiClientFactory
    {
        public static async Task<AuthorizedClientDetails> CreateAuthorizedApiClient(this IApplicationSecrets ApplicationSecrets, IConfiguration Configuration, string SubscriptionName = null)
        {
            AuthorizedClientDetails details = new AuthorizedClientDetails();

            try
            {
                // Construct the name of the secret to pull from KeyVault. If a subscription name is not specified
                // as a parameter, then get it from the "SubscriptionName" property in the configuration file
                if (!string.IsNullOrEmpty(SubscriptionName))
                {
                    details.apiSecretName = SubscriptionName + ".Api";
                }
                else
                {
                    details.apiSecretName = Configuration["SubscriptionName"] + ".Api";
                }

                // Get the secret
                details.secret = ApplicationSecrets.Secret(details.apiSecretName);

                // Convert the metadata in the secret into an AuthConfig object.
                details.authConfig = details.secret.MetadataConverter<AuthConfig>();

                // Use the information in AuthConfig to retrieve the JWT required to call the 
                // [Authorize] secured API 
                details.authorizedApiClient = await details.authConfig.GetAuthorizedClient();
            }
            catch (Exception e)
            {
            }
            return (details);

        }
    }
}
