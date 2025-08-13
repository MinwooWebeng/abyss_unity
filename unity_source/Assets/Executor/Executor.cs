using System;
using UnityEngine;

/// <summary>
/// Executor runs abyss engine. This script must execute after RenderBase and UIBase.
/// </summary>
public class Executor : MonoBehaviour
{
    public RendererBase RendererBase;
    public UIBase UIBase;
    private Host.Host _host;

    void OnEnable()
    {
        _host = new();

        _host.InjectExecutorTarg(RendererBase, UIBase);
        _host.Start();
    }
    void Update()
    {
        DateTime time_begin = DateTime.Now; // Current time
        while (_host.RenderingActionQueue.TryDequeue(out var action) &&
            (DateTime.Now - time_begin) < TimeSpan.FromMilliseconds(10))
        {
            action();
        }
    }
    void OnDisable()
    {
        _host.Dispose();
        _host = null;
    }
}