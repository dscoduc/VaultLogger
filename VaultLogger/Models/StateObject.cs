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

        public StateObject() { }

        public StateObject(Socket handler)
        {
            workSocket = handler;

            System.Net.IPAddress remoteAddress = ((System.Net.IPEndPoint)workSocket.RemoteEndPoint).Address;
            Program.DebugLog.Info($"[{Id}] New socket connection established from {remoteAddress}");
        }

        public void AddStringContent(string content)
        {
            Program.DebugLog.Debug($"[{Id}] Adding content to socket handler log cache");

            lock (_syncLock)
            {
                _sb.Append(content);
            }
        }
        public string ReadAndFlushStringContent()
        {
            Program.DebugLog.Debug($"[{Id}] Retrieving and flushing all content from socket handler log cache");

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
