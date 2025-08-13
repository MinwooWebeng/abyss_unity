using System.Collections.Generic;
using UnityEngine;

public class RendererBase : MonoBehaviour
{
    // in current implementation, element is GameObject
    [HideInInspector] public Dictionary<int, GameObject> _elements;
    [HideInInspector] public GameObject _nil_root;
    [HideInInspector] public GameObject _root;
    // I have no idea what type can resources inherit in common.
    [HideInInspector] public Dictionary<int, object> _resources;
    void OnEnable()
    {
        _nil_root = new GameObject("hidden");
        _nil_root.SetActive(false);
        _root = new GameObject("root");
        _elements = new()
        {
            [-1] = _nil_root,
            [0] = _root
        };
        _resources = new();
    }
    void OnDisable()
    {
        _elements = null;
        GameObject.Destroy(_nil_root);
        GameObject.Destroy(_root);
        _resources = null;
    }
}