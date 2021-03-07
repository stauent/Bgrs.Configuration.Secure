using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bgrs.Configuration.Secure
{
    public static class KeyVaultHelper
    {
        public static string ApplicationSecretsSectionName { get; set; } = "ApplicationSecrets";
        public static string InitialConfigurationSectionName { get; set; } = "InitialConfiguration";


        /// <summary>
        /// "https://<VAULT_NAME>.vault.azure.net/";
        /// </summary>
        /// <returns></returns>
        private static string GetKeyVaultEndpoint(string KeyVaultName) => $"https://{KeyVaultName}.vault.azure.net/";

        /// <summary>
        /// Adds Azure KeyVault as part of the app configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder to build up the configuration</param>
        /// <param name="KeyVaultName">Name of the key vault to connect to</param>
        public static void AddAzureKeyVaultClient(this IConfigurationBuilder builder, string KeyVaultName)
        {
            try
            {
                if (!string.IsNullOrEmpty(KeyVaultName))
                {
                    var keyVaultEndpoint = GetKeyVaultEndpoint(KeyVaultName);
                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                    {
                        // Refer to these sites for more information:
                        // https://blogs.aaddevsup.xyz/2020/01/exploring-azureservicetokenprovider-class-with-azure-key-vault/
                        // https://docs.microsoft.com/en-us/dotnet/api/overview/azure/service-to-service-authentication
                        //
                        // The constructor will use the environment variable "AzureServicesAuthConnectionString" IF it exists.
                        AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                        KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                        builder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                    }
                }
            }
            catch
            {
            }
        }


        /// <summary>
        /// The appsettings section "ApplicationSecrets" contains all connection string and sensitive information.
        /// To hide this information from source control and to allow individual developers to have their own settings
        /// we copy the section "ApplicationSecrets" into the secrets.json file for local development.
        /// In production this value will come from KeyVault. This method reads the appropriate values
        /// can generates the final IApplicationSecrets that will be used at runtime. 
        /// </summary>
        /// <param name="configuration">IConfigurationRoot</param>
        /// <param name="applicationSetupConfiguration">IApplicationSetupConfiguration</param>
        /// <returns>IApplicationSecrets containing contents of the "ApplicationSecrets" section of configuration</returns>
        public static IApplicationSecrets InitializeApplicationSecrets(this IConfigurationRoot configuration, IApplicationSetupConfiguration applicationSetupConfiguration)
        {
            ApplicationSecrets retVal = null;

            try
            {
                if (!string.IsNullOrEmpty(applicationSetupConfiguration.KeyVaultKey))
                {
                    string mySecret = configuration[applicationSetupConfiguration.KeyVaultKey];
                    string decoded = Base64Decode(mySecret);

                    JObject jo = JObject.Parse(decoded);
                    string val = jo.Properties().First(x => x.Name == ApplicationSecretsSectionName).Value.ToString();
                    retVal = JsonConvert.DeserializeObject<ApplicationSecrets>(val);
                }
            }
            catch
            {
            }

            // Bind the local configuration properties to the properties in the ApplicationSecrets object
            IConfigurationSection myConfiguration = configuration.GetSection(ApplicationSecretsSectionName);
            ApplicationSecrets localSecrets = new ApplicationSecrets();
            myConfiguration.Bind(localSecrets);

            // If the local configuration contains secrets that were not present in KeyVault, then include them
            if (retVal != null)
            {
                foreach (ApplicationSecretsConnectionStrings nextSecret in localSecrets.ConnectionStrings)
                {
                    // Try to find the local secret name in the KeyVault version. If not found in KeyVault, then insert it
                    // into final merge.
                    IApplicationSecretsConnectionStrings found = retVal.Secret(nextSecret.Name);
                    if (found == null)
                    {
                        retVal.ConnectionStrings.Add(nextSecret);
                    }
                }
            }
            else
            {
                retVal = localSecrets;
            }

            return (retVal);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

    }


}
