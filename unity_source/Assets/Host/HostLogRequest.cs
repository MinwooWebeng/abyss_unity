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
            case RenderAction.InnerOneofCase.ConsolePrint: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ConsolePrint)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateElement: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateElement)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.MoveElement: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.MoveElement)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteElement: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.DeleteElement)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.ElemSetActive: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ElemSetActive)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.ElemSetTransform: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ElemSetTransform)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateItem: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateItem)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteItem: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.DeleteItem)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.ItemSetTitle: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ItemSetTitle)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.ItemSetIcon: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ItemSetIcon)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.ItemSetActive: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ItemSetActive)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.ItemAlert: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ItemAlert)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.OpenStaticResource: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.OpenStaticResource)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CloseResource: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CloseResource)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateCompositeResource: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateCompositeResource)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.MemberInfo: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.MemberInfo)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.MemberSetProfile: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.MemberSetProfile)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.MemberLeave: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.MemberLeave)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateImage: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateImage)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteImage: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.DeleteImage)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateMaterialV: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateMaterialV)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateMaterialF: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateMaterialF)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.MaterialSetParamV: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.MaterialSetParamV)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.MaterialSetParamC: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.MaterialSetParamC)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteMaterial: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.DeleteMaterial)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateStaticMesh: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateStaticMesh)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.StaticMeshSetMaterial: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.StaticMeshSetMaterial)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.ElemAttachStaticMesh: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.ElemAttachStaticMesh)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteStaticMesh: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.DeleteStaticMesh)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.CreateAnimation: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.CreateAnimation)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.DeleteAnimation: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.DeleteAnimation)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.LocalInfo: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.LocalInfo)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.InfoContentShared: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.InfoContentShared)); _render_log_writer.Flush(); return;
            case RenderAction.InnerOneofCase.InfoContentDeleted: _render_log_writer.WriteLine(FormatFlatLogLine(render_action.InfoContentDeleted)); _render_log_writer.Flush(); return;
            default: StderrQueue.Enqueue("Executor: invalid RenderAction: " + render_action.InnerCase); return;
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