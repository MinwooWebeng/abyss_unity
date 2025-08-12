using AbyssCLI.ABI;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

public partial class ExecutorDepr : MonoBehaviour
{
    [SerializeField] private GameObject objHolder;
    [SerializeField] private CommonShaderLoader commonShaderLoader;
    [SerializeField] private int stepLimit;
    [SerializeField] private int currentStep;
    [SerializeField] private bool executeActions;
    [SerializeField] private UIHandler uiHandler;
    [SerializeField] private bool runTest;

    public Action<string> SetAdditionalInfoCallback = _ => { };

    private AbyssEngine.HostDepr _abyss_host;
    public string _local_aurl;
    public string _local_hash;
    private Dictionary<string, string> soms = new(); //TODO: Deprecate

    private StreamWriter renderlogwriter;

    public void MoveWorld(string url)
    {
        _abyss_host.CallFunc.MoveWorld(url);
    }
    public void ShareContent(Guid uuid, string url, UnityEngine.Vector3 pos, UnityEngine.Quaternion rot)
    {
        _abyss_host.CallFunc.ShareContent(ByteString.CopyFrom(uuid.ToByteArray()), url, new Vec3 { X = pos.x, Y = pos.y, Z = pos.z }, new Vec4 { W = rot.w, X = rot.x, Y = rot.y, Z = rot.z });
    }
    public void UnshareContent(Guid uuid)
    {
        _abyss_host.CallFunc.UnshareContent(ByteString.CopyFrom(uuid.ToByteArray()));
    }
    public void ConsoleCommand(int element_id, string command)
    {
        _abyss_host.CallFunc.ConsoleInput(element_id, command);
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
        _elements = new();
        _components = new();

        //root and nil-root object
        var nil_root = new GameObject("hidden");
        var root = new GameObject("root");
        _elements[-1] = nil_root;
        _elements[0] = root;

        nil_root.SetActive(false);

        //read root key from file
        string[] pemFiles = Directory.GetFiles(".", "*.pem", SearchOption.TopDirectoryOnly);
        if (pemFiles.Length == 0)
        {
            Debug.LogError("no user key found");
            executeActions = false;
            return;
        }

        _abyss_host = new AbyssEngine.HostDepr(pemFiles[0]);
        if (!_abyss_host.IsValid)
        {
            Debug.LogError("failed to open abyss host");
            executeActions = false;
        }

        //logger
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"render_log{timestamp}.json";
        renderlogwriter = new(fileName);
    }
    void OnDisable()
    {
        GameObject.Destroy(_elements[0]);
        GameObject.Destroy(_elements[-1]);

        foreach (var comp in _components)
        {
            comp.Value.Dispose();
        }

        _components = null;
        _elements = null;

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
        if (runTest)
        {
            TestAction();
            runTest = false;
        }

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
                LogRequest(render_action);
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
        case RenderAction.InnerOneofCase.ConsolePrint: ConsolePrint(render_action.ConsolePrint); return;
        case RenderAction.InnerOneofCase.CreateElement: CreateElement(render_action.CreateElement); return;
        case RenderAction.InnerOneofCase.MoveElement: MoveElement(render_action.MoveElement); return;
        case RenderAction.InnerOneofCase.DeleteElement: DeleteElement(render_action.DeleteElement); return;
        case RenderAction.InnerOneofCase.ElemSetActive: ElemSetActive(render_action.ElemSetActive); return;
        case RenderAction.InnerOneofCase.ElemSetTransform: ElemSetTransform(render_action.ElemSetTransform); return;
        case RenderAction.InnerOneofCase.CreateItem: CreateItem(render_action.CreateItem); return;
        case RenderAction.InnerOneofCase.DeleteItem: DeleteItem(render_action.DeleteItem); return;
        case RenderAction.InnerOneofCase.ItemSetTitle: ItemSetTitle(render_action.ItemSetTitle); return;
        case RenderAction.InnerOneofCase.ItemSetIcon: ItemSetIcon(render_action.ItemSetIcon); return;
        case RenderAction.InnerOneofCase.ItemSetActive: ItemSetActive(render_action.ItemSetActive); return;
        case RenderAction.InnerOneofCase.ItemAlert: ItemAlert(render_action.ItemAlert); return;
        case RenderAction.InnerOneofCase.OpenStaticResource: OpenStaticResource(render_action.OpenStaticResource); return;
        case RenderAction.InnerOneofCase.CloseResource: CloseResource(render_action.CloseResource); return;
        case RenderAction.InnerOneofCase.CreateCompositeResource: CreateCompositeResource(render_action.CreateCompositeResource); return;
        case RenderAction.InnerOneofCase.MemberInfo: MemberInfo(render_action.MemberInfo); return;
        case RenderAction.InnerOneofCase.MemberSetProfile: MemberSetProfile(render_action.MemberSetProfile); return;
        case RenderAction.InnerOneofCase.MemberLeave: MemberLeave(render_action.MemberLeave); return;
        case RenderAction.InnerOneofCase.CreateImage: CreateImage(render_action.CreateImage); return;
        case RenderAction.InnerOneofCase.DeleteImage: DeleteImage(render_action.DeleteImage); return;
        case RenderAction.InnerOneofCase.CreateMaterialV: CreateMaterialV(render_action.CreateMaterialV); return;
        case RenderAction.InnerOneofCase.CreateMaterialF: CreateMaterialF(render_action.CreateMaterialF); return;
        case RenderAction.InnerOneofCase.MaterialSetParamV: MaterialSetParamV(render_action.MaterialSetParamV); return;
        case RenderAction.InnerOneofCase.MaterialSetParamC: MaterialSetParamC(render_action.MaterialSetParamC); return;
        case RenderAction.InnerOneofCase.DeleteMaterial: DeleteMaterial(render_action.DeleteMaterial); return;
        case RenderAction.InnerOneofCase.CreateStaticMesh: CreateStaticMesh(render_action.CreateStaticMesh); return;
        case RenderAction.InnerOneofCase.StaticMeshSetMaterial: StaticMeshSetMaterial(render_action.StaticMeshSetMaterial); return;
        case RenderAction.InnerOneofCase.ElemAttachStaticMesh: ElemAttachStaticMesh(render_action.ElemAttachStaticMesh); return;
        case RenderAction.InnerOneofCase.DeleteStaticMesh: DeleteStaticMesh(render_action.DeleteStaticMesh); return;
        case RenderAction.InnerOneofCase.CreateAnimation: CreateAnimation(render_action.CreateAnimation); return;
        case RenderAction.InnerOneofCase.DeleteAnimation: DeleteAnimation(render_action.DeleteAnimation); return;
        case RenderAction.InnerOneofCase.LocalInfo: LocalInfo(render_action.LocalInfo); return;
        case RenderAction.InnerOneofCase.InfoContentShared: InfoContentShared(render_action.InfoContentShared); return;
        case RenderAction.InnerOneofCase.InfoContentDeleted: InfoContentDeleted(render_action.InfoContentDeleted); return;
        default: Debug.LogError("Executor: invalid RenderAction: " + render_action.InnerCase); return;
        }
    }
    private void LogRequest(RenderAction render_action)
    {
        switch (render_action.InnerCase)
        {
        case RenderAction.InnerOneofCase.ConsolePrint: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ConsolePrint)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateElement: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateElement)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.MoveElement: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MoveElement)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.DeleteElement: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteElement)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.ElemSetActive: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ElemSetActive)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.ElemSetTransform: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ElemSetTransform)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateItem: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateItem)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.DeleteItem: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteItem)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.ItemSetTitle: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemSetTitle)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.ItemSetIcon: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemSetIcon)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.ItemSetActive: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemSetActive)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.ItemAlert: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemAlert)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.OpenStaticResource: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.OpenStaticResource)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CloseResource: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CloseResource)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateCompositeResource: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateCompositeResource)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.MemberInfo: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MemberInfo)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.MemberSetProfile: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MemberSetProfile)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.MemberLeave: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MemberLeave)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateImage: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateImage)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.DeleteImage: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteImage)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateMaterialV: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateMaterialV)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateMaterialF: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateMaterialF)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.MaterialSetParamV: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MaterialSetParamV)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.MaterialSetParamC: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MaterialSetParamC)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.DeleteMaterial: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteMaterial)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateStaticMesh: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateStaticMesh)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.StaticMeshSetMaterial: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.StaticMeshSetMaterial)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.ElemAttachStaticMesh: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ElemAttachStaticMesh)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.DeleteStaticMesh: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteStaticMesh)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.CreateAnimation: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateAnimation)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.DeleteAnimation: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteAnimation)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.LocalInfo: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.LocalInfo)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.InfoContentShared: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.InfoContentShared)); renderlogwriter.Flush(); return;
        case RenderAction.InnerOneofCase.InfoContentDeleted: renderlogwriter.WriteLine(FormatFlatLogLine(render_action.InfoContentDeleted)); renderlogwriter.Flush(); return;
        default: renderlogwriter.WriteLine("Executor: invalid RenderAction: " + render_action.InnerCase); renderlogwriter.Flush(); return;
        }
    }
    string FormatFlatLogLine(object obj)
    {
        var sb = new StringBuilder();
        sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {obj.GetType().Name} |");

        var type = obj.GetType();
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (!IsSimple(field.FieldType)) continue;
            sb.Append($" {field.Name}={FormatValue(field.GetValue(obj))}");
        }

        foreach (var prop in properties)
        {
            if (!prop.CanRead || !IsSimple(prop.PropertyType)) continue;
            sb.Append($" {prop.Name}={FormatValue(prop.GetValue(obj))}");
        }

        return sb.ToString();
    }
    bool IsSimple(Type type)
    {
        return type.IsPrimitive || type == typeof(string) || type == typeof(byte[]);
    }

    string FormatValue(object value)
    {
        if (value == null) return "null";

        return value switch
        {
            string s => s,
            byte[] bytes => BitConverter.ToString(bytes).Replace("-", ""), // Hex string
            bool b => b ? "true" : "false",
            float f => f.ToString("R"),
            double d => d.ToString("R"),
            _ => value.ToString()
        };
    }

    private int test_counter = 0;
    private void TestAction()
    {
        Debug.Log("*** Test ***");

        switch(test_counter)
        {
            case 0:
                uiHandler.MemberProfileSection.CreateProfile("mem188");
                uiHandler.MemberItemSection.CreateMember("mem188");
                uiHandler.MemberItemSection.CreateItem("mem188", 17);
                break;
            case 1:
                break;
        }
        test_counter++;
    }

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
