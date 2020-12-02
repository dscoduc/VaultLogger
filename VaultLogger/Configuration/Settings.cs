using VaultLogger.Extensions;
using System;
using System.Linq;
using System.Net;

namespace VaultLogger.Configuration
{
    public class Settings
    {
        internal IPAddress IPv4Address = IPAddress.Parse(Constants.DefaultAddress);
        internal int Port = Constants.DefaultPort;
        internal int SocketBufferSize = Constants.DefaultSocketBufferSize;
        internal int ConnectionQueue = Constants.DefaultConnectionQueue;

        /// <summary>
        /// Program settings to be used by the application
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

        private void evaluateArgs(string[] args)
        {
            if (Program.DebugLog.IsDebugEnabled)
            {
                Program.DebugLog.Debug("Startup arguments found: {0}", string.Join(", ", args));
            }

            string addressString = getArgValue(args, Constants.AddressParamName);
            if (!string.IsNullOrWhiteSpace(addressString))
            {
                if (!isValidIPv4Format(addressString))
                {
                    throw new Exception(Constants.AddressParamExceptionMessage);
                }

                IPv4Address = IPAddress.Parse(addressString);
            }

            string portString = getArgValue(args, Constants.PortParamName);
            if (!string.IsNullOrWhiteSpace(portString))
            {
                if (!int.TryParse(portString, out Port))
                {
                    throw new Exception(Constants.PortParamExceptionMessage);
                }

                if (!Enumerable.Range(1024, 65535).Contains(Port))
                {
                    throw new Exception(Constants.PortParamOutOfRangeExceptionMessage);
                }
            }

            string connectionQueueString = getArgValue(args, Constants.ConnectionQueueParamName);
            if (!string.IsNullOrWhiteSpace(connectionQueueString))
            {
                if (!int.TryParse(connectionQueueString, out ConnectionQueue))
                {
                    throw new Exception(Constants.ConnectionQueueParamExceptionMessage);
                }
            }

            string socketBufferSizeString = getArgValue(args, Constants.SocketBufferSizeParamName);
            if (!string.IsNullOrWhiteSpace(socketBufferSizeString))
            {
                if (!int.TryParse(socketBufferSizeString, out SocketBufferSize))
                {
                    throw new Exception(Constants.SocketBufferSizeParamExceptionMessage);
                }

                if (SocketBufferSize % 1024 != 0)
                {
                    throw new Exception(Constants.SocketBufferSizeParamOutOfRangeExceptionMessage);
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

        /// <summary>
        /// Check if valid IPv4 address format
        /// </summary>
        /// <param name="address">IPv4 address in string format (e.g. 127.0.0.1)</param>
        /// <returns>True if string is in correct IPv4 address format, else False</returns>
        private bool isValidIPv4Format(string address)
        {
            Program.DebugLog.Debug("Validating address format");

            string[] octets = address.Split('.');
            if (octets.Length != 4)
            {
                Program.DebugLog.Debug($"Validation failed - address contained an invalid number of octets [{octets.Length}]");
                return false;
            }

            if (octets.All(octet => byte.TryParse(octet, out _)) == false)
            {
                Program.DebugLog.Debug("Validation failed - not all octets in the address were integers");
                return false;
            }

            return true;
        }
    }
}
