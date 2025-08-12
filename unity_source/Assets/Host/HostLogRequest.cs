using AbyssCLI.ABI;
using System;
using System.Reflection;
using System.Text;

namespace Host
{
    partial class Host
    {
        private void LogRequest(RenderAction render_action)
        {
            switch (render_action.InnerCase)
            {
            case RenderAction.InnerOneofCase.ConsolePrint: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ConsolePrint)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateElement: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateElement)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.MoveElement: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MoveElement)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteElement: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteElement)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.ElemSetActive: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ElemSetActive)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.ElemSetTransform: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ElemSetTransform)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateItem: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateItem)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteItem: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteItem)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.ItemSetTitle: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemSetTitle)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.ItemSetIcon: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemSetIcon)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.ItemSetActive: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemSetActive)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.ItemAlert: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ItemAlert)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.OpenStaticResource: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.OpenStaticResource)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CloseResource: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CloseResource)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateCompositeResource: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateCompositeResource)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.MemberInfo: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MemberInfo)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.MemberSetProfile: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MemberSetProfile)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.MemberLeave: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MemberLeave)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateImage: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateImage)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteImage: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteImage)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateMaterialV: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateMaterialV)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateMaterialF: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateMaterialF)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.MaterialSetParamV: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MaterialSetParamV)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.MaterialSetParamC: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.MaterialSetParamC)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteMaterial: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteMaterial)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateStaticMesh: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateStaticMesh)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.StaticMeshSetMaterial: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.StaticMeshSetMaterial)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.ElemAttachStaticMesh: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.ElemAttachStaticMesh)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteStaticMesh: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteStaticMesh)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.CreateAnimation: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.CreateAnimation)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteAnimation: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.DeleteAnimation)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.LocalInfo: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.LocalInfo)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.InfoContentShared: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.InfoContentShared)); _renderlogwriter.Flush(); return;
            case RenderAction.InnerOneofCase.InfoContentDeleted: _renderlogwriter.WriteLine(FormatFlatLogLine(render_action.InfoContentDeleted)); _renderlogwriter.Flush(); return;
            default: _renderlogwriter.WriteLine("Executor: invalid RenderAction: " + render_action.InnerCase); _renderlogwriter.Flush(); return;
            }
        }
        string FormatFlatLogLine(object obj)
        {
            var sb = new StringBuilder();
            _ = sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {obj.GetType().Name} |");

            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                if (!IsSimple(field.FieldType)) continue;
                _ = sb.Append($" {field.Name}={FormatValue(field.GetValue(obj))}");
            }

            foreach (var prop in properties)
            {
                if (!prop.CanRead || !IsSimple(prop.PropertyType)) continue;
                _ = sb.Append($" {prop.Name}={FormatValue(prop.GetValue(obj))}");
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
    }
}