using AbyssCLI.ABI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using UnityEngine;

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
        public readonly ConcurrentQueue<Action> RenderingActionQueue = new();
        
        //used in HostLogRequest.cs
        private readonly StreamWriter _renderlogwriter;

        //used in HostInterpretRequest.cs
        private readonly Dictionary<int, GameObject> _elements = new(); // in current implementation, element is GameObject
        GameObject _nil_root;
        GameObject _root;
        private readonly Dictionary<int, object> _resources = new(); // I have no idea what type can resources inherit in common.

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
            _renderlogwriter = new(fileName);

            //unity requirements setup
            _nil_root = new GameObject("hidden");
            _nil_root.SetActive(false);
            _root = new GameObject("root");
            _elements[-1] = _nil_root;
            _elements[0] = _root;
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

            GameObject.Destroy(_elements[-1]);
            GameObject.Destroy(_elements[0]);
        }
    }
}