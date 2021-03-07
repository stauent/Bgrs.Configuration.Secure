using System.Collections.Generic;
using System.Linq;

namespace Bgrs.Configuration.Secure
{
    public enum EnabledLoggersEnum
    {
        None,
        File,
        Console,
        Debug
    }

    public enum ObjectSerializationFormat
    {
        String,
        Json
    }

    public interface IApplicationSetupConfiguration
    {
        /// <summary>
        /// When objects are being serialized for logging, the format can be:
        ///     String
        ///     Json
        /// </summary>
        ObjectSerializationFormat SerializationFormat { get; set; }

        /// <summary>
        /// Specifies the name of the key vault key we want to use for configuration.
        /// If this contains a non-null value, then Key will be used to access Azure KeyVault.
        /// The data returned will override all the values in "ApplicationSecrets". This
        /// is where all secret/sensitive password/connection information is stored.
        /// The format of this key should be:
        ///         KeyName-Environment
        /// e.g. ReloAccessSecrets-DFX
        ///
        /// Every environment should have InitialConfiguration__KeyVaultKey environment variable
        /// set for their local setup. This way appsettings.json can leave this empty
        /// and every environment in which the code runs will take the value from the
        /// InitialConfiguration__KeyVaultKey environment variable.
        /// </summary>
        string KeyVaultKey { get; set; }

        /// <summary>
        /// Specifies the name of the Azure key vault we want to use to pull the key from
        /// </summary>
        string KeyVaultName { get; set; }

        /// <summary>
        /// If the name of the logger was specified in the configuration
        /// then true is returned
        /// </summary>
        bool IsLoggerEnabled(EnabledLoggersEnum LoggerType);

        /// <summary>
        /// Returns true if any kind of logging is enabled
        /// </summary>
        /// <returns>true or false</returns>
        bool IsLoggingEnabled { get; }


        /// <summary>
        /// Allows you to specify which loggers (if any) are to be used at runtime.
        /// The available options are  "File", "Console", "Debug", "None". If "None"
        /// is specified or no option is provided at all, then no logging will occur.
        /// Otherwise one or more of the options  "File", "Console", "Debug" can be used
        /// together. "Console" logs to the console window. "Debug" logs to the visual studio
        /// debug output window. "File" logs to a file. 
        /// </summary>
        List<string> EnabledLoggers { get; set; }

        /// <summary>
        /// Specifies the runtime environment name
        /// </summary>
        string RTE { get; set; }
    }

    public class InitialConfiguration : IApplicationSetupConfiguration
    {
        /// <summary>
        /// When objects are being serialized for logging, the format can be:
        ///     String
        ///     Json
        /// </summary>
        public ObjectSerializationFormat SerializationFormat { get; set; }

        /// <summary>
        /// Specifies the name of the key vault key we want to use for configuration.
        /// If this contains a non-null value, then Key will be used to access Azure KeyVault.
        /// The data returned will override all the values in "ApplicationSecrets". This
        /// is where all secret/sensitive password/connection information is stored.
        /// The format of this key should be:
        ///         KeyName-Environment
        /// e.g. ReloAccessSecrets-DFX
        ///
        /// Every environment should have InitialConfiguration__KeyVaultKey environment variable
        /// set for their local setup. This way appsettings.json can leave this empty
        /// and every environment in which the code runs will take the value from the
        /// InitialConfiguration__KeyVaultKey environment variable.
        /// </summary>
        public string KeyVaultKey { get; set; }

        /// <summary>
        /// Specifies the name of the Azure key vault we want to use to pull the key from
        /// </summary>
        public string KeyVaultName { get; set; }

        /// <summary>
        /// If the name of the logger was specified in the configuration
        /// then true is returned
        /// </summary>
        public bool IsLoggerEnabled(EnabledLoggersEnum LoggerType)
        {
            if (EnabledLoggers != null)
            {
                string found = EnabledLoggers.Find(loggerName => loggerName == LoggerType.ToString());
                return (found != null);
            }

            return false;

        }

        /// <summary>
        /// Returns true if any kind of logging is enabled
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsLoggingEnabled
        {
            get
            {
                bool enabled = true;
                if (EnabledLoggers != null && EnabledLoggers.Count() > 0)
                {
                    if (IsLoggerEnabled(EnabledLoggersEnum.None))
                        enabled = false;
                }

                return (enabled);
            }
        }

        /// <summary>
        /// Allows you to specify which loggers (if any) are to be used at runtime.
        /// The available options are  "File", "Console", "Debug", "None". If "None"
        /// is specified or no option is provided at all, then no logging will occur.
        /// Otherwise one or more of the options  "File", "Console", "Debug" can be used
        /// together. "Console" logs to the console window. "Debug" logs to the visual studio
        /// debug output window. "File" logs to a file. 
        /// </summary>
        public List<string> EnabledLoggers { get; set; }

        protected string _RTE { get; set; }

        /// <summary>
        /// Specifies the runtime environment name
        /// </summary>
        public string RTE
        {
            get
            {
                if (string.IsNullOrEmpty(_RTE))
                {
                    _RTE = "DEFAULT";
                }
                return (_RTE);
            }
            set { _RTE = value; }
        }
    }
}
