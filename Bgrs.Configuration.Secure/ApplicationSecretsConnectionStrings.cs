using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bgrs.Configuration.Secure
{
    public interface IApplicationSecretsConnectionStrings
    {
        /// <summary>
        /// Name of the database
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Connection string value
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Specifies the category of connection string stored in the "Value" property
        /// </summary>
        String Category { get; set; }

        /// <summary>
        /// Describes the purpose of this connection string
        /// </summary>
        string Description { get; set; }


        /// <summary>
        /// Custom metadata associated with the connection string. Can be in any format
        /// that makes sense for your connection.
        /// </summary>
        List<SecretMetaData> MetaDataProperties { get; set; }

        /// <summary>
        /// Retrieves the metadata property by name
        /// </summary>
        /// <param name="propertyName">metadata property name</param>
        /// <returns>value of property</returns>
        string MetaDataProperty(string propertyName);

        /// <summary>
        /// Retrieves the metadata using the key as an indexer
        /// </summary>
        /// <param name="key">metadata property key</param>
        /// <returns>value of property</returns>
        string this[string key] { get; }

        /// <summary>
        /// Takes all the metadata properties and creates an object out of them.
        /// </summary>
        /// <typeparam name="T">Type of object to be created</typeparam>
        /// <returns>Object of type T, populated with values from the MetaDataProperties list</returns>
        T MetadataConverter<T>() where T : class, new();
    }

    /// <summary>
    /// Each connection string entry in the appsettings.json file is represented by a Json object
    /// that has the properties "Name" and "Value". The configuration file has a property called 
    /// "ConnectionStrings" that contains a JSON array of these items.
    /// </summary>
    public class ApplicationSecretsConnectionStrings : IApplicationSecretsConnectionStrings
    {
        /// <summary>
        /// Name of the connection string you want to access
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Connection string value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Specifies the category of connection string stored in the "Value" property
        /// </summary>
        public String Category { get; set; }

        /// <summary>
        /// Describes the purpose of this connection string
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Custom metadata associated with the connection string. Can be in any format
        /// that makes sense for your connection.
        /// </summary>
        public List<SecretMetaData> MetaDataProperties { get; set; }

        /// <summary>
        /// Retrieves the metadata property by name
        /// </summary>
        /// <param name="propertyName">metadata property name</param>
        /// <returns>value of property</returns>
        public string MetaDataProperty(string propertyName)
        {
            string retVal = "";
            ISecretMetaData found = MetaDataProperties?.Find(item => item.Name == propertyName);
            retVal = found?.Value;
            return (retVal);
        }

        /// <summary>
        /// Retrieves the metadata using the key as an indexer
        /// </summary>
        /// <param name="key">metadata property key</param>
        /// <returns>value of property</returns>
        public string this[string key]
        {
            get
            {
                return MetaDataProperty(key);
            }
        }

        /// <summary>
        /// Takes all the metadata properties and creates an object out of them.
        /// </summary>
        /// <typeparam name="T">Type of object to be created</typeparam>
        /// <returns>Object of type T, populated with values from the MetaDataProperties list</returns>
        public T MetadataConverter<T>() where T : class, new()
        {
            T retObj = default(T);

            try
            {
                JObject retVal = new JObject();
                foreach (SecretMetaData metaData in MetaDataProperties)
                {
                    JProperty property = new JProperty(metaData.Name, metaData.Value);
                    retVal.Add(property);
                }

                retObj = JsonConvert.DeserializeObject<T>(retVal.ToString(Formatting.None));
            }
            catch 
            {
            }

            return retObj;
        }
    }
}
