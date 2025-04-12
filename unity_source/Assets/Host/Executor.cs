using AbyssCLI.ABI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Executor : MonoBehaviour
{
    [SerializeField]
    private GameObject objHolder;
    [SerializeField]
    private CommonShaderLoader commonShaderLoader;
    [SerializeField]
    private int stepLimit;
    [SerializeField]
    private int currentStep;
    [SerializeField]
    private bool executeActions;

    public Action<string> SetLocalAddrCallback = (string _) => { };
    public Action<string> SetAdditionalInfoCallback = (string _) => { };

    public void MoveWorld(string url)
    {
        _abyss_host.CallFunc.MoveWorld(url);
    }
    public void LoadContent(string url, UnityEngine.Vector3 pos, UnityEngine.Quaternion rot)
    {
        _abyss_host.CallFunc.ShareContent(url, new Vec3 { X = pos.x, Y = pos.y, Z = pos.z }, new Vec4 { W = rot.w, X = rot.x, Y = rot.y, Z = rot.z });
    }
    public void ConnectPeer(string aurl)
    {
        _abyss_host.CallFunc.ConnectPeer(aurl);
    }

    void OnEnable()
    {
#if UNITY_EDITOR
#else
        // Get the directory of the executable
        string exeDirectory = System.IO.Directory.GetParent(Application.dataPath).FullName;

        // Define the log file path in the same directory as the executable
        var logFilePath = System.IO.Path.Combine(exeDirectory, "log_" + DateTime.Now.ToString("HH_mm_ss"));

        // Open the file for writing
        logWriter = new System.IO.StreamWriter(logFilePath)
        {
            AutoFlush = true // Auto flush so data is written immediately to the file
        }; // 'true' to append to the file if it exists

        // Subscribe to the log message event
        Application.logMessageReceived += LogToFile;
#endif
        _game_objects = new();
        _components = new();

        //root and nil-root object
        var nil_root = new GameObject("-1");
        var root = new GameObject("0");
        _game_objects[-1] = nil_root;
        _game_objects[0] = root;

        nil_root.SetActive(false);

        //read root key from file
        string[] pemFiles = Directory.GetFiles(".", "*.pem", SearchOption.TopDirectoryOnly);
        if (pemFiles.Length == 0)
        {
            Debug.LogError("no user key found");
            executeActions = false;
            return;
        }

        _abyss_host = new AbyssEngine.Host(pemFiles[0]);
        if (!_abyss_host.IsValid)
        {
            Debug.LogError("failed to open abyss host");
            executeActions = false;
        }
    }
    void OnDisable()
    {
        GameObject.Destroy(_game_objects[0]);
        GameObject.Destroy(_game_objects[-1]);

        foreach (var comp in _components)
        {
            comp.Value.Dispose();
        }

        _components = null;
        _game_objects = null;

        if (_abyss_host.IsValid)
        {
            _abyss_host.Close();
        }
        _abyss_host = null;

        //field reset
        currentStep = 0;

#if UNITY_EDITOR
#else
        Application.logMessageReceived -= LogToFile;

        // Close the file when the application quits or object is destroyed
        if (logWriter != null)
        {
            logWriter.Close();
            logWriter = null;
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (!executeActions) return;

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        int i = 0;
        while (currentStep < stepLimit && _abyss_host.TryPopRenderAction(out RenderAction render_action))
        {
            i++;
            currentStep++;

            try
            {
                //Debug.Log("render action case: " + render_action.InnerCase);
                ExecuteRequest(render_action);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                executeActions = false;
                break;
            }

            var execution_time_mS = stopwatch.Elapsed.TotalMilliseconds;
            if (execution_time_mS > 5)
            {
                break;
            }
        }
        //if (i != 0)
        //    Debug.Log("executed " + i + " calls, " + _abyss_host.GetLeftoverRenderActionCount() + " remaining");
    }
    void FixedUpdate()
    {
        if (_abyss_host.IsValid && _abyss_host.TryPopException(out Exception e))
        {
            Debug.Log(e.Message + "\nstacktrace: " + e.StackTrace);
        }
    }
    private void ExecuteRequest(RenderAction render_action)
    {
        switch (render_action.InnerCase)
        {
            case RenderAction.InnerOneofCase.CreateElement:
                CreateElement(render_action.CreateElement);
                return;
            case RenderAction.InnerOneofCase.MoveElement:
                MoveElement(render_action.MoveElement);
                return;
            case RenderAction.InnerOneofCase.DeleteElement:
                DeleteElement(render_action.DeleteElement);
                return;
            case RenderAction.InnerOneofCase.ElemSetPos:
                ElemSetPos(render_action.ElemSetPos);
                return;
            case RenderAction.InnerOneofCase.CreateImage:
                CreateImage(render_action.CreateImage);
                return;
            case RenderAction.InnerOneofCase.DeleteImage:
                DeleteImage(render_action.DeleteImage);
                return;
            case RenderAction.InnerOneofCase.CreateMaterialV:
                CreateMaterialV(render_action.CreateMaterialV);
                return;
            case RenderAction.InnerOneofCase.CreateMaterialF:
                CreateMaterialF(render_action.CreateMaterialF);
                return;
            case RenderAction.InnerOneofCase.MaterialSetParamV:
                MaterialSetParamV(render_action.MaterialSetParamV);
                return;
            case RenderAction.InnerOneofCase.MaterialSetParamC:
                MaterialSetParamC(render_action.MaterialSetParamC);
                return;
            case RenderAction.InnerOneofCase.DeleteMaterial:
                DeleteMaterial(render_action.DeleteMaterial);
                return;
            case RenderAction.InnerOneofCase.CreateStaticMesh:
                CreateStaticMesh(render_action.CreateStaticMesh);
                return;
            case RenderAction.InnerOneofCase.StaticMeshSetMaterial:
                StaticMeshSetMaterial(render_action.StaticMeshSetMaterial);
                return;
            case RenderAction.InnerOneofCase.ElemAttachStaticMesh:
                ElemAttachStaticMesh(render_action.ElemAttachStaticMesh);
                return;
            case RenderAction.InnerOneofCase.DeleteStaticMesh:
                DeleteStaticMesh(render_action.DeleteStaticMesh);
                return;
            case RenderAction.InnerOneofCase.CreateAnimation:
                CreateAnimation(render_action.CreateAnimation);
                return;
            case RenderAction.InnerOneofCase.DeleteAnimation:
                DeleteAnimation(render_action.DeleteAnimation);
                return;
            case RenderAction.InnerOneofCase.LocalInfo:
                SetLocalAddrCallback(render_action.LocalInfo.Aurl);
                return;
            case RenderAction.InnerOneofCase.InfoContentShared:
                InfoContentShared(render_action.InfoContentShared);
                return;
            case RenderAction.InnerOneofCase.InfoContentDeleted:
                InfoContentDeleted(render_action.InfoContentDeleted);
                return;
            default:
                Debug.LogError("Executor: invalid RenderAction: " + render_action.InnerCase);
                return;
        }
    }

    private void CreateElement(RenderAction.Types.CreateElement args)
    {
        GameObject newGO = new(args.ElementId.ToString());
        newGO.transform.SetParent(_game_objects[args.ParentId].transform, false);
        _game_objects[args.ElementId] = newGO;
    }
    private void MoveElement(RenderAction.Types.MoveElement args)
    {
        _game_objects[args.ElementId].transform.SetParent(_game_objects[args.NewParentId].transform, true);
    }
    private void DeleteElement(RenderAction.Types.DeleteElement args)
    {
        GameObject.Destroy(_game_objects[args.ElementId]);
        _game_objects.Remove(args.ElementId);
    }
    private void ElemSetPos(RenderAction.Types.ElemSetPos args)
    {
        _game_objects[args.ElementId].transform.SetLocalPositionAndRotation(new Vector3(args.Pos.X, args.Pos.Y, args.Pos.Z), new Quaternion(args.Rot.X, args.Rot.Y, args.Rot.Z, args.Rot.W));
    }
    private void CreateImage(RenderAction.Types.CreateImage args)
    {
        _components[args.ImageId] = new AbyssEngine.Component.Image(args.File);
    }
    private void DeleteImage(RenderAction.Types.DeleteImage args)
    {
        DeleteComponent(args.ImageId);
    }
    private void CreateMaterialV(RenderAction.Types.CreateMaterialV args)
    {
        _components[args.MaterialId] = new AbyssEngine.Component.Material(
            commonShaderLoader.Get(args.ShaderName),
            commonShaderLoader.GetParameterIDMap(args.ShaderName)
        );
    }
    private void CreateMaterialF(RenderAction.Types.CreateMaterialF args)
    {
        throw new NotImplementedException();
    }
    private void MaterialSetParamV(RenderAction.Types.MaterialSetParamV args)
    {
        var mat = _components[args.MaterialId] as AbyssEngine.Component.Material;
        switch (args.Param.ValCase)
        {
            case AnyVal.ValOneofCase.Int:
                mat.UnityMaterial.SetInteger(args.ParamName, args.Param.Int);
                break;
            case AnyVal.ValOneofCase.Double:
                mat.UnityMaterial.SetFloat(args.ParamName, (float)args.Param.Double);
                break;
            case AnyVal.ValOneofCase.Vec2:
                mat.UnityMaterial.SetVector(args.ParamName, new UnityEngine.Vector2((float)args.Param.Vec2.X, (float)args.Param.Vec2.Y));
                break;
            case AnyVal.ValOneofCase.Vec3:
                mat.UnityMaterial.SetVector(args.ParamName, new UnityEngine.Vector3((float)args.Param.Vec3.X, (float)args.Param.Vec3.Y, (float)args.Param.Vec3.Z));
                break;
            case AnyVal.ValOneofCase.Vec4:
                mat.UnityMaterial.SetVector(args.ParamName, new UnityEngine.Vector4((float)args.Param.Vec4.W, (float)args.Param.Vec4.X, (float)args.Param.Vec4.Y, (float)args.Param.Vec4.Z));
                break;
            default:
                throw new NotImplementedException();
        }
    }
    private void MaterialSetParamC(RenderAction.Types.MaterialSetParamC args)
    {
        var mat = _components[args.MaterialId] as AbyssEngine.Component.Material;
        var comp = _components[args.ComponentId];
        switch (comp)
        {
            case AbyssEngine.Component.Image image:
                mat.SetTexture(args.ParamName, image);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    private void DeleteMaterial(RenderAction.Types.DeleteMaterial args)
    {
        DeleteComponent(args.MaterialId);
    }
    private void CreateStaticMesh(RenderAction.Types.CreateStaticMesh args)
    {
        _components[args.MeshId] = new AbyssEngine.Component.StaticMesh(args.File, objHolder, "C" + args.MeshId.ToString());
    }
    private void StaticMeshSetMaterial(RenderAction.Types.StaticMeshSetMaterial args)
    {
        var mesh = _components[args.MeshId] as AbyssEngine.Component.StaticMesh;
        var mat = _components[args.MaterialId] as AbyssEngine.Component.Material;
        mesh.SetMaterial(args.MaterialSlot, mat);
    }
    private void ElemAttachStaticMesh(RenderAction.Types.ElemAttachStaticMesh args)
    {
        var parent = _game_objects[args.ElementId];
        var mesh = _components[args.MeshId] as AbyssEngine.Component.StaticMesh;
        mesh.InstantiateTracked(parent);
    }
    private void DeleteStaticMesh(RenderAction.Types.DeleteStaticMesh args)
    {
        DeleteComponent(args.MeshId);
    }
    private void CreateAnimation(RenderAction.Types.CreateAnimation args) { }
    private void DeleteAnimation(RenderAction.Types.DeleteAnimation args) { }


    //others
    private void DeleteComponent(int component_id)
    {
        _components[component_id].Dispose();
        _components.Remove(component_id);
    }

    private Dictionary<string, string> soms = new();
    private void InfoContentShared(RenderAction.Types.InfoContentShared args) 
    {
        soms[args.ContentUuid] = args.ContentUuid + " " + args.ContentUrl + " from " + args.SharerHash + " in " + args.WorldUuid;
        SetAdditionalInfoCallback(string.Join("\n", soms.Values));
    }
    private void InfoContentDeleted(RenderAction.Types.InfoContentDeleted args)
    {
        soms.Remove(args.ContentUuid);
        SetAdditionalInfoCallback(string.Join("\n", soms.Values));
    }

    private AbyssEngine.Host _abyss_host;
    private Dictionary<int, GameObject> _game_objects;
    private Dictionary<int, AbyssEngine.Component.IComponent> _components;


    //logger
#if UNITY_EDITOR
#else
    private System.IO.StreamWriter logWriter;

    // This will be called whenever a log message is generated
    private void LogToFile(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"{System.DateTime.Now}: [{type}] {logString}\n";

        if (type == LogType.Exception || type == LogType.Error)
        {
            logEntry += $"{stackTrace}\n";
        }

        // Write the log entry to the file
        logWriter.WriteLine(logEntry);
    }
#endif
}
