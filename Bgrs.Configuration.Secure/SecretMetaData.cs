using System;
using System.Collections.Generic;
using System.Text;

namespace Bgrs.Configuration.Secure
{
    public interface ISecretMetaData
    {
        /// <summary>
        /// Name of the metadata property
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// metadata property value
        /// </summary>
        string Value { get; set; }
    }

    public class SecretMetaData : ISecretMetaData
    {
        /// <summary>
        /// Name of the metadata property
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// metadata property value
        /// </summary>
        public string Value { get; set; }
    }

}
