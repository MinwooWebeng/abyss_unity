using UnityEngine;
public class Executor : MonoBehaviour
{
    private Host.Host _host;
    void OnEnable()
    {
        _host = new();
    }
    void Update()
    {
        
    }
    void OnDisable()
    {
        _host.Dispose();
        _host = null;
    }
}