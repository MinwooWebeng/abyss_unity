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
        }
        public void Start()
        {
            _rx_thread.Start();
            _rx_stderr_thread.Start();
        }

        private void RxLoop()
        {
            while(true)
            {
                try
                {
                    var render_action = _engine_com.Rx.Read();
                    LogRequest(render_action);
                    InterpretRequest(render_action);
                }
                catch (Exception ex)
                {
                    StderrQueue.Enqueue("fatal:::RxLoop throwed an error: " + ex.ToString());
                    return;
                }
            }
        }
        private void RxStdErrLoop()
        {
            while (true)
            {
                try
                {
                    var err_msg = _engine_com.StdErr.ReadLine() ?? throw new Exception("StdErr.ReadLine() returned null");
                    StderrQueue.Enqueue(err_msg);
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