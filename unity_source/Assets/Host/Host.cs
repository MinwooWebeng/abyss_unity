using AbyssCLI.ABI;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Host
{
    /// <summary>
    /// This host handles engine IO and action interpreting.
    /// Construction and Dispose() must be called from Unity main thread.
    /// It MUST be disposed.
    /// </summary>
    public partial class Host : IDisposable
    {
        private readonly EngineCom.EngineCom _engine_com;
        private readonly Thread _rx_thread;
        private readonly Thread _rx_stderr_thread;

        public UIActionWriter Tx => _engine_com.Tx;
        public readonly ConcurrentQueue<string> StderrQueue = new();
        
        //used in HostLogRequest.cs
        private readonly StreamWriter _render_log_writer;

        public Host()
        {
            //find root key from current directory
            string[] pemFiles = Directory.GetFiles(".", "*.pem", SearchOption.TopDirectoryOnly);
            if (pemFiles.Length == 0)
                throw new Exception("fatal:::no user key found");

            //main setup
            _engine_com = new(pemFiles[0]);
            _rx_thread = new(RxLoop);
            _rx_stderr_thread = new(RxStdErrLoop);

            //logger setup
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"render_log{timestamp}.json";
            _render_log_writer = new(fileName);
        }
        public void Start()
        {
            _rx_thread.Start();
            _rx_stderr_thread.Start();
        }

        private void RxLoop()
        {
            while(_engine_com.Rx.TryRead(out var render_action))
            {
                try
                {
                    LogRequest(render_action);
                    InterpretRequest(render_action);
                }
                catch (Exception ex)
                {
                    StderrQueue.Enqueue("fatal:::RxLoop throwed an error: " + ex.ToString());
                }
            }
        }
        private void RxStdErrLoop()
        {
            while (true)
            {
                try
                {
                    StderrQueue.Enqueue(_engine_com.StdErr.ReadLine());
                }
                catch
                {
                    StderrQueue.Enqueue("===== stderr closed =====");
                    return;
                }
            }
        }

        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _engine_com.Stop();

            _rx_thread.Join();
            _rx_stderr_thread.Join();
            _engine_com.Dispose();
        }
    }
}