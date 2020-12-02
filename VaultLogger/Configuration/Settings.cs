using VaultLogger.Extensions;
using System;
using System.Linq;
using System.Net;

namespace VaultLogger.Configuration
{
    public class Settings
    {
        private string addressExceptionMessage = $"{Constants.AddressParamName} requires a properly formatted IPv4 address (Default: {Constants.DefaultAddress}).";
        private string portExceptionMessage = $"{Constants.PortParamName} requires an integer value within the acceptable port range [1024-65535] (Default: {Constants.DefaultPort}).";
        private string connectionQueueExceptionMessage = $"{Constants.ConnectionQueueParamName} requires an integer value (Default: {Constants.DefaultConnectionQueue}).";
        private string socketBufferSizeExceptionMessage = $"{Constants.SocketBufferSizeParamName} requires an integer value in kilobytes (e.g. 2048, 4096, 8192) (Default: {Constants.DefaultSocketBufferSize}).";

        public IPAddress IPv4Address = IPAddress.Parse(Constants.DefaultAddress);
        public int Port = Constants.DefaultPort;
        public int SocketBufferSize = Constants.DefaultSocketBufferSize;
        public int ConnectionQueue = Constants.DefaultConnectionQueue;

        /// <summary>
        /// Initialized new settings for the application
        /// </summary>
        /// <param name="args">Startup arguments provided at runtime</param>
        public Settings(string[] args)
        {
            Program.DebugLog.Debug("Initializing new configuration settings");

            if (args.Length > 0)
            {
                evaluateArgs(args);
            }
        }

        /// <summary>
        /// Evaluate each optional argument used by the application
        /// </summary>
        /// <param name="args">Arguments provied during the startup</param>
        private void evaluateArgs(string[] args)
        {
            if (Program.DebugLog.IsDebugEnabled)
            {
                Program.DebugLog.Debug("Startup arguments found: {0}", string.Join(", ", args));
            }

            string addressString = getArgValue(args, Constants.AddressParamName);
            if (!string.IsNullOrWhiteSpace(addressString))
            {
                try
                {
                    string[] octets = addressString.Split('.');
                    if (octets.Length != 4)
                    {
                        throw new Exception($"Validation failed - address contained an invalid number of octets [{octets.Length}]");
                    }

                    if (octets.All(octet => byte.TryParse(octet, out _)) == false)
                    {
                        throw new Exception("Validation failed - not all octets in the address were integers.");
                    }

                    IPv4Address = IPAddress.Parse(addressString);
                }
                catch (Exception ex)
                {
                    Program.DebugLog.Debug(ex);
                    throw new Exception(addressExceptionMessage);
                }
            }

            string portString = getArgValue(args, Constants.PortParamName);
            if (!string.IsNullOrWhiteSpace(portString))
            {
                try
                {
                    if (!int.TryParse(portString, out Port))
                    {
                        throw new Exception($"Validation failed - {Constants.PortParamName} was not a valid integer.");
                    }

                    if (!Enumerable.Range(1024, 65535).Contains(Port))
                    {
                        throw new Exception($"Validation failed - {Constants.PortParamName} was outside the acceptable integer range.");
                    }
                }
                catch (Exception ex)
                {
                    Program.DebugLog.Debug(ex);
                    throw new Exception(portExceptionMessage);
                }
            }

            string connectionQueueString = getArgValue(args, Constants.ConnectionQueueParamName);
            if (!string.IsNullOrWhiteSpace(connectionQueueString))
            {
                if (!int.TryParse(connectionQueueString, out ConnectionQueue))
                {
                    throw new Exception(connectionQueueExceptionMessage);
                }
            }

            string socketBufferSizeString = getArgValue(args, Constants.SocketBufferSizeParamName);
            if (!string.IsNullOrWhiteSpace(socketBufferSizeString))
            {
                try
                {
                    if (!int.TryParse(socketBufferSizeString, out SocketBufferSize))
                    {
                        throw new Exception($"Validation failed - {Constants.SocketBufferSizeParamName} was not a valid integer.");
                    }

                    if (SocketBufferSize % 1024 != 0)
                    {
                        throw new Exception($"Validation failed - {Constants.SocketBufferSizeParamName} was not in a valid kilobytes format (e.g. 2048, 4096, 8192).");
                    }
                }
                catch (Exception ex)
                {
                    Program.DebugLog.Debug(ex);
                    throw new Exception(socketBufferSizeExceptionMessage);
                }
            }
        }

        /// <summary>
        /// Extract the value from the name/value argument data
        /// </summary>
        /// <param name="args">Startup arguments provided at runtime</param>
        /// <param name="argName">Name of argument to perform extraction</param>
        private string getArgValue(string[] args, string argName)
        {
            string argValue = null;

            string arg = args.FirstOrDefault(x => x.Contains($"-{argName}=", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(arg))
            {
                Program.DebugLog.Debug($"Extracting {argName} from startup arguments");
                argValue = arg.Remove($"-{argName}=", StringComparison.OrdinalIgnoreCase);
            }

            return argValue;
        }
    }
}
