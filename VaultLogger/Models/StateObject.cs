using System;
using System.Net.Sockets;
using System.Text;

namespace VaultLogger.Models
{
    public class StateObject
    {
        public static readonly int BufferSize = Program.Config.SocketBufferSize;

        private object _syncLock = new object();
        private StringBuilder _sb = new StringBuilder();

        public Guid Id = Guid.NewGuid();
        public byte[] buffer = new byte[BufferSize];
        public Socket workSocket = null;

        public StateObject()
        {
            Program.DebugLog.Info("[{0}] New socket connection established", Id);
        }

        public void AddStringContent(string content)
        {
            Program.DebugLog.Debug("[{0}] Adding content to socket handler log cache", Id);

            lock (_syncLock)
            {
                _sb.Append(content);
            }
        }
        public string ReadAndFlushStringContent()
        {
            Program.DebugLog.Debug("[{0}] Retrieving and flushing all content from socket handler log cache", Id);

            string rValue;

            lock (_syncLock)
            {
                rValue = _sb.ToString();
                _sb.Clear();                
            }

            return rValue;
        }
    }
}
