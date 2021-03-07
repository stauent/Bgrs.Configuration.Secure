using System;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Bgrs.Configuration.Secure
{
    /// <summary>
    /// Extension method that will allow ANY object to log it's information
    /// with a very simple syntax. Just append one of the ".TraceXXX" methods
    /// to ANY object, and the contents of that object will be output in the
    /// specified log locations.
    /// </summary>
    public static class TraceLoggerExtension
    {
        private static ILogger _logger = null;
        private static IHostEnvironment _hostingEnvironment = null;
        private static string _applicationName = "";
        public static string _environmentName = "";

        public static ILogger _Logger
        {
            get { return (_logger); }
            set
            {
                if (_logger == null)
                    _logger = value;
            }
        }

        public static IHostEnvironment _HostEnvironment
        {
            get { return (_hostingEnvironment); }
            set
            {
                if (_hostingEnvironment == null)
                    _hostingEnvironment = value;
            }
        }


        public static string GetApplicationName()
        {
            if (string.IsNullOrEmpty(_applicationName))
            {
                if (_hostingEnvironment != null)
                    _applicationName = _hostingEnvironment.ApplicationName;
            }
            return (_applicationName);
        }

        public static string GetEnvironmentName()
        {
            if (string.IsNullOrEmpty(_environmentName))
            {
                if (_hostingEnvironment != null)
                    _environmentName = _hostingEnvironment.EnvironmentName;
            }
            return (_environmentName);
        }

        public static ObjectSerializationFormat _SerializationFormat { get; set; } = ObjectSerializationFormat.Json;

        public static void TraceInformation(this object objectToTrace, string message = null, [CallerLineNumber] int LineNumber = 0, [CallerMemberName] string MethodName = null, [CallerFilePath] string FileName = null)
        {
            _Logger?.LogInformation($"\r\n\t{Environment.MachineName}:{GetApplicationName()}:{GetEnvironmentName()}:{ExtractFileName(FileName)}:{MethodName}:{LineNumber} {message ?? ""}\r\n\t{ConvertToString(objectToTrace)}");
        }
        public static void TraceCritical(this object objectToTrace, string message = null, [CallerLineNumber] int LineNumber = 0, [CallerMemberName] string MethodName = null, [CallerFilePath] string FileName = null)
        {
            _Logger?.LogCritical($"\r\n\t{Environment.MachineName}:{GetApplicationName()}:{GetEnvironmentName()}:{ExtractFileName(FileName)}:{MethodName}:{LineNumber} {message ?? ""}\r\n\t{ConvertToString(objectToTrace)}");
        }
        public static void TraceDebug(this object objectToTrace, string message = null, [CallerLineNumber] int LineNumber = 0, [CallerMemberName] string MethodName = null, [CallerFilePath] string FileName = null)
        {
            _Logger?.LogDebug($"\r\n\t{Environment.MachineName}:{GetApplicationName()}:{GetEnvironmentName()}:{ExtractFileName(FileName)}:{MethodName}:{LineNumber} {message ?? ""}\r\n\t{ConvertToString(objectToTrace)}");
        }
        public static void TraceError(this object objectToTrace, string message = null, [CallerLineNumber] int LineNumber = 0, [CallerMemberName] string MethodName = null, [CallerFilePath] string FileName = null)
        {
            _Logger?.LogError($"\r\n\t{Environment.MachineName}:{GetApplicationName()}:{GetEnvironmentName()}:{ExtractFileName(FileName)}:{MethodName}:{LineNumber} {message ?? ""}\r\n\t{ConvertToString(objectToTrace)}");
        }
        public static void TraceWarning(this object objectToTrace, string message = null, [CallerLineNumber] int LineNumber = 0, [CallerMemberName] string MethodName = null, [CallerFilePath] string FileName = null)
        {
            _Logger?.LogWarning($"\r\n\t{Environment.MachineName}:{GetApplicationName()}:{GetEnvironmentName()}:{ExtractFileName(FileName)}:{MethodName}:{LineNumber} {message ?? ""}\r\n\t{ConvertToString(objectToTrace)}");
        }

        public static string ExtractFileName(string FilePath)
        {
            string retVal = FilePath;

            try
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    string[] parts = FilePath.Split("\\");
                    retVal = parts[parts.Length - 1];
                }
            }
            catch
            {
            }
            return (retVal);
        }

        static string ConvertToString(object objectToTrace)
        {
            string retVal = "";
            if (objectToTrace != null)
            {
                bool IsString = objectToTrace is string;

                JsonSerializerSettings jSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 1
                };

                switch (_SerializationFormat)
                {
                    case ObjectSerializationFormat.Json:
                        if (IsString)
                            retVal = (string)objectToTrace;
                        else
                            retVal = JsonConvert.SerializeObject(objectToTrace, Formatting.Indented, jSettings);
                        break;

                    case ObjectSerializationFormat.String:
                        if (IsString)
                            retVal = (string) objectToTrace;
                        else
                            retVal = objectToTrace.ToString();
                        break;
                }
            }

            return (retVal);
        }
    }

}


