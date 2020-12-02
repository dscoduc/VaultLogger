using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VaultLogger.Configuration;
using VaultLogger.Models;

namespace VaultLogger
{
    public class Program
    {
        private static readonly Logger auditLog = LogManager.GetLogger(Constants.DefaultAuditLoggerName);
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static int exitCode = 0;

        internal static Settings Config;
        internal static readonly Logger DebugLog = LogManager.GetCurrentClassLogger();

        public static int Main(String[] args)
        {
            try
            {
                Config = new Settings(args);
                StartListening();
            }
            catch (Exception ex)
            {
                DebugLog.Fatal(ex.Message);
                DebugLog.Debug(ex);
                exitCode = 1;
            }

            return exitCode;
        }

        internal static void StartListening()
        {
            Socket listener = new Socket(Config.IPv4Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint localEndPoint = new IPEndPoint(Config.IPv4Address, Config.Port);
            listener.Bind(localEndPoint);
            listener.Listen(Config.ConnectionQueue);

            DebugLog.Info("Listening using the following settings:");
            DebugLog.Info($"-{Constants.AddressParamName}={Config.IPv4Address}");
            DebugLog.Info($"-{Constants.PortParamName}={Config.Port}");
            DebugLog.Info($"-{Constants.ConnectionQueueParamName}={Config.ConnectionQueue}");
            DebugLog.Info($"-{Constants.SocketBufferSizeParamName}={Config.SocketBufferSize}");

            while (true)
            {
                allDone.Reset();
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                allDone.WaitOne();
            }
        }

        internal static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();

            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
        }

        internal static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            try
            {
                int bytesRead = handler.EndReceive(ar);

                DebugLog.Debug("[{0}] Reading {1} bytes from socket handler", state.Id, bytesRead);

                if (bytesRead > 0)
                {
                    state.AddStringContent(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    if (handler.Available > 0)
                    {
                        DebugLog.Debug("[{0}] Socket handler contains {1} bytes remaining to read", state.Id, handler.Available);
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                    else
                    {
                        var content = state.ReadAndFlushStringContent();
                        
                        Task.Run(() => logContent(content.Trim(), state.Id.ToString()));

                        DebugLog.Debug("[{0}] Socket handler connected: {1}", state.Id, handler.Connected);
                        DebugLog.Debug("[{0}] Socket handler remaining bytes: {1}", state.Id, handler.Available);

                        if (handler.Connected == false || handler.Available == 0)
                        {
                            DebugLog.Debug("[{0}] Shutting down socket connection", state.Id);
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            DebugLog.Info("[{0}] Socket connection closed", state.Id);
                        }
                        else
                        {
                            DebugLog.Debug("[{0}] Sending back around to get remaining stream content", state.Id);
                            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLog.Warn(ex, "[{0}] ReadCallback exception", state.Id);
            }
        }

        private static Task logContent(string content, string id)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Task.CompletedTask;
            }

            try
            {
                DebugLog.Info("[{0}] Sending {1} bytes to logger [{2}]", id, content.Length, auditLog.Name);

                string[] entries = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string entry in entries)
                {
                    auditLog.Info(entry.Trim());
                }
            }
            catch (Exception ex)
            {
                DebugLog.Error(ex, "[{0}] logContent exception", id);
            }

            return Task.CompletedTask;
        }
    }
}
