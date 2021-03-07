using System;
using System.Collections.Generic;
using System.Text;

namespace Bgrs.Configuration.Secure
{
    /// <summary>
    /// Interface used to access all properties in the "ApplicationSecrets" property of the appsettings.json file
    /// </summary>
    public interface IApplicationSecrets
    {
        /// <summary>
        /// Name of user using the application 
        /// </summary>
        string UserName { get; set; }


        /// <summary>
        /// Returns the connection string associated with the "ConnectionName"
        /// </summary>
        /// <param name="ConnectionName">Name of connection we want to get the connection string for</param>
        /// <returns>Connection string associated with the specified item</returns>
        string ConnectionString(string ConnectionName);

        /// <summary>
        /// Retrieves all the information related to the ConnectionName specified
        /// </summary>
        /// <param name="ConnectionName">Name of the secret/connection you want to get information for</param>
        /// <returns>ApplicationSecretsConnectionStrings</returns>
        IApplicationSecretsConnectionStrings Secret(string ConnectionName);
    }

    public class ApplicationSecrets : IApplicationSecrets
    {
        /// <summary>
        /// Name of user using the application 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Every Name:Value pair in the MyProjectSettings:ConnectionStrings
        /// appsettings is deserialized into this list.
        /// </summary
        public List<ApplicationSecretsConnectionStrings> ConnectionStrings { get; set; }

        /// <summary>
        /// Returns the connection string associated with the "ConnectionName"
        /// </summary>
        /// <param name="ConnectionName">Name of item we want to get the connection string for</param>
        /// <returns>Connection string associated with the specified item</returns>
        public string ConnectionString(string ConnectionName)
        {
            ApplicationSecretsConnectionStrings found = ConnectionStrings?.Find(item => item.Name == ConnectionName);
            return (found?.Value);
        }

        /// <summary>
        /// Retrieves all the information related to the ConnectionName specified
        /// </summary>
        /// <param name="ConnectionName">Name of the secret/connection you want to get information for</param>
        /// <returns>ApplicationSecretsConnectionStrings</returns>
        public IApplicationSecretsConnectionStrings Secret(string ConnectionName)
        {
            return ConnectionStrings?.Find(item => item.Name == ConnectionName);
        }
    }


}
