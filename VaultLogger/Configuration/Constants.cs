
namespace VaultLogger.Configuration
{
    internal class Constants
    {
        internal const string DefaultAuditLoggerName = "AuditLog";

        internal const string AddressParamName = "Address";
        internal const string DefaultAddress = "127.0.0.1";
        internal static string AddressParamExceptionMessage = $"{AddressParamName} requires a properly formatted IPv4 address (Default: {DefaultAddress})";

        internal const string PortParamName = "Port";
        internal const int DefaultPort = 11000;
        internal static string PortParamExceptionMessage = $"{PortParamName} requires a valid integer value (Default: {DefaultPort})";
        internal static string PortParamOutOfRangeExceptionMessage = $"{PortParamName} must be between (1024-65535) (e.g. {DefaultPort})";

        internal const string ConnectionQueueParamName = "ConnectionQueue";
        internal const int DefaultConnectionQueue = 100;
        internal static string ConnectionQueueParamExceptionMessage = $"{ConnectionQueueParamName} requires a valid integer value (Default: {DefaultConnectionQueue})";

        internal const string SocketBufferSizeParamName = "SocketBufferSize";
        internal const int DefaultSocketBufferSize = 8192;
        internal static string SocketBufferSizeParamExceptionMessage = $"{SocketBufferSizeParamName} requires a valid integer (Default: {DefaultSocketBufferSize})";
        internal static string SocketBufferSizeParamOutOfRangeExceptionMessage = $"{SocketBufferSizeParamName} must be in kilobytes (e.g. 2048, 4096, 8192)";
    }
}
