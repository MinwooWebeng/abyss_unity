using System;
using UnityEngine;

/// <summary>
/// Executor runs abyss engine. This script must execute after RenderBase and UIBase.
/// </summary>
public class Executor : MonoBehaviour
{
    public GlobalDependency.RendererBase RendererBase;
    public GlobalDependency.UIBase UIBase;
    public GlobalDependency.InteractionBase InteractionBase;
    private Host.Host _host;

    void OnEnable()
    {
        UnityThreadChecker.Init();
        GlobalDependency.Logger.Init();
        _host = new();

        _host.InjectExecutorTarg(RendererBase, UIBase, InteractionBase);
        _host.Start();
    }
    void Update()
    {
        DateTime time_begin = DateTime.Now; // Current time
        while (_host.RenderingActionQueue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch(Exception ex)
            {
                RuntimeCout.Print("Fatal:::Executor failed to execute action::" + ex.ToString());
            }

            if ((DateTime.Now - time_begin) > TimeSpan.FromMilliseconds(10))
                break;
        }
        while (_host.StderrQueue.TryDequeue(out var err_msg))
        {
            RuntimeCout.Print("Engine:::StdErr>> " + err_msg);

            if ((DateTime.Now - time_begin) < TimeSpan.FromMilliseconds(16))
                break;
        }
    }
    void OnDisable()
    {
        _host.Dispose();
        _host = null;
        UnityThreadChecker.Clear();
    }
}
