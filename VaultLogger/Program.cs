using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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

        #region Handle shutdown
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler(CtrlTypes sig);
        private static EventHandler _handler = new EventHandler(handler);
        #endregion

        public static Settings Config;
        public static readonly Logger DebugLog = LogManager.GetCurrentClassLogger();

        public static void Main(String[] args)
        {
            SetConsoleCtrlHandler(_handler, true);
            DebugLog.Info("Starting up...");

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

            StateObject state = new StateObject(handler);

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadCallback), state);
        }

        internal static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            try
            {
                int bytesRead = handler.EndReceive(ar);

                DebugLog.Debug($"[{state.Id}] Reading {bytesRead} bytes from socket handler");

                if (bytesRead > 0)
                {
                    state.AddStringContent(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    if (handler.Available > 0)
                    {
                        DebugLog.Debug($"[{state.Id}] Socket handler contains {handler.Available} bytes remaining to read");
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                    else
                    {
                        var content = state.ReadAndFlushStringContent();

                        Task.Run(() => logContent(content.Trim(), state.Id.ToString()));

                        DebugLog.Debug($"[{state.Id}] Socket handler connected: {handler.Connected}");
                        DebugLog.Debug($"[{state.Id}] Socket handler remaining bytes: {handler.Available}");

                        if (handler.Connected == false || handler.Available == 0)
                        {
                            DebugLog.Debug($"[{state.Id}] Shutting down socket connection");
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                            DebugLog.Info($"[{state.Id}] Socket connection closed");
                        }
                        else
                        {
                            DebugLog.Debug($"[{state.Id}] Sending back around to get remaining stream content");
                            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLog.Warn(ex, $"[{state.Id}] ReadCallback exception");
            }
        }

        private static Task logContent(string content, string id)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    DebugLog.Info($"[{id}] Sending {content.Length} bytes to logger [{auditLog.Name}]");

                    string[] entries = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string entry in entries)
                    {
                        auditLog.Info(entry.Trim());
                        DebugLog.Trace($"[{id}] {entry.Trim()}");
                    }
                }
                catch (Exception ex)
                {
                    DebugLog.Error(ex, $"[{id}] logContent exception");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle CTRL events to support shutdown tasks
        /// </summary>
        private static bool handler(CtrlTypes ctrlType)
        {
            DebugLog.Info("Shutting down...");
            LogManager.Flush();
            Environment.Exit(exitCode);
            return true;
        }
    }
}
