using AbyssCLI.ABI;
using Google.Protobuf;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Host
{
    partial class Host
    {
        public ConcurrentQueue<Action> RenderingActionQueue = new();
        private GlobalDependency.RendererBase _renderer_base;
        private GlobalDependency.UIBase _ui_base;
        private GlobalDependency.InteractionBase _interaction_base;
        private readonly StaticResourceLoader _static_resource_loader = new();
        public void HostInterpretRequestDispose()
        {
            RenderingActionQueue.Clear();
            _renderer_base = null;
            _ui_base = null;
            _interaction_base = null;
            _static_resource_loader.Dispose();
        }
        public void InjectExecutorTarg(
            GlobalDependency.RendererBase renderer_base,
            GlobalDependency.UIBase ui_base,
            GlobalDependency.InteractionBase interaction_base)
        {
            _renderer_base = renderer_base;
            _ui_base = ui_base;
            _interaction_base = interaction_base;

            _ui_base.OnAddressBarSubmit = (arg) => Tx.MoveWorld(arg);
            _ui_base.OnSubAddressBarSubmit = (arg) =>
            {
                if (arg.StartsWith("connect "))
                {
                    var conn_addr = arg["connect ".Length..];
                    Tx.ConnectPeer(conn_addr);
                    return;
                }
                var transform = _interaction_base.GetContentSpawnPos();
                var uuid = Guid.NewGuid();
                Tx.ShareContent(
                    ByteString.CopyFrom(uuid.ToByteArray()),
                    arg,
                    new Vec3 { X = transform.localPosition.x, Y = transform.localPosition.y, Z = transform.localPosition.z },
                    new Vec4 { W = transform.localRotation.w, X = transform.localRotation.x, Y = transform.localRotation.y, Z = transform.localRotation.z }
                );
            };
            _ui_base.OnConsoleCommand = (arg) => Tx.ConsoleInput(0, arg);
            _ui_base.LocalItemSection.OnCloseCallback = (uuid) =>
                Tx.UnshareContent(ByteString.CopyFrom(uuid.ToByteArray()));

            _static_resource_loader.SynchronizedActionEnqueueCallback =
                RenderingActionQueue.Enqueue;
        }
        public void HostInterpretRequestStart()
        {
            _static_resource_loader.Start();
        }

        private void InterpretRequest(RenderAction render_action)
        {
            switch (render_action.InnerCase)
            {
            case RenderAction.InnerOneofCase.ConsolePrint: RenderingActionQueue.Enqueue(ConsolePrint(render_action.ConsolePrint)); return;
            case RenderAction.InnerOneofCase.CreateElement: RenderingActionQueue.Enqueue(CreateElement(render_action.CreateElement)); return;
            case RenderAction.InnerOneofCase.MoveElement: RenderingActionQueue.Enqueue(MoveElement(render_action.MoveElement)); return;
            case RenderAction.InnerOneofCase.DeleteElement: RenderingActionQueue.Enqueue(DeleteElement(render_action.DeleteElement)); return;
            case RenderAction.InnerOneofCase.ElemSetActive: RenderingActionQueue.Enqueue(ElemSetActive(render_action.ElemSetActive)); return;
            case RenderAction.InnerOneofCase.ElemSetTransform: RenderingActionQueue.Enqueue(ElemSetTransform(render_action.ElemSetTransform)); return;
            case RenderAction.InnerOneofCase.CreateItem: RenderingActionQueue.Enqueue(CreateItem(render_action.CreateItem)); return;
            case RenderAction.InnerOneofCase.DeleteItem: RenderingActionQueue.Enqueue(DeleteItem(render_action.DeleteItem)); return;
            case RenderAction.InnerOneofCase.ItemSetTitle: RenderingActionQueue.Enqueue(ItemSetTitle(render_action.ItemSetTitle)); return;
            case RenderAction.InnerOneofCase.ItemSetIcon: RenderingActionQueue.Enqueue(ItemSetIcon(render_action.ItemSetIcon)); return;
            case RenderAction.InnerOneofCase.ItemSetActive: RenderingActionQueue.Enqueue(ItemSetActive(render_action.ItemSetActive)); return;
            case RenderAction.InnerOneofCase.ItemAlert: RenderingActionQueue.Enqueue(ItemAlert(render_action.ItemAlert)); return;
            case RenderAction.InnerOneofCase.OpenStaticResource: OpenStaticResource(render_action.OpenStaticResource); return;
            case RenderAction.InnerOneofCase.CloseResource: RenderingActionQueue.Enqueue(CloseResource(render_action.CloseResource)); return;
            case RenderAction.InnerOneofCase.CreateCompositeResource: RenderingActionQueue.Enqueue(CreateCompositeResource(render_action.CreateCompositeResource)); return;
            case RenderAction.InnerOneofCase.MemberInfo: RenderingActionQueue.Enqueue(MemberInfo(render_action.MemberInfo)); return;
            case RenderAction.InnerOneofCase.MemberSetProfile: RenderingActionQueue.Enqueue(MemberSetProfile(render_action.MemberSetProfile)); return;
            case RenderAction.InnerOneofCase.MemberLeave: RenderingActionQueue.Enqueue(MemberLeave(render_action.MemberLeave)); return;
            case RenderAction.InnerOneofCase.LocalInfo: RenderingActionQueue.Enqueue(LocalInfo(render_action.LocalInfo)); return;
            case RenderAction.InnerOneofCase.InfoContentShared: RenderingActionQueue.Enqueue(InfoContentShared(render_action.InfoContentShared)); return;
            case RenderAction.InnerOneofCase.InfoContentDeleted: RenderingActionQueue.Enqueue(InfoContentDeleted(render_action.InfoContentDeleted)); return;
            default: StderrQueue.Enqueue("Executor: invalid RenderAction: " + render_action.InnerCase); return;
            }
        }
        private Action ConsolePrint(RenderAction.Types.ConsolePrint args) => () => _ui_base.AppendConsole(args.Text);
        private Action CreateElement(RenderAction.Types.CreateElement args) => () =>
        {
            GameObject newGO = new(args.ElementId.ToString());
            newGO.transform.SetParent(_renderer_base.GetElement(args.ParentId).transform, false);
            _renderer_base._elements[args.ElementId] = newGO;
        };
        private Action MoveElement(RenderAction.Types.MoveElement args) => () =>
        {
            _renderer_base._elements[args.ElementId].transform.SetParent(
                _renderer_base.GetElement(args.NewParentId).transform, false);
        };
        private Action DeleteElement(RenderAction.Types.DeleteElement args) => () =>
        {
            GameObject.Destroy(_renderer_base.GetElement(args.ElementId));
            _ = _renderer_base._elements.Remove(args.ElementId);
        };
        private Action ElemSetActive(RenderAction.Types.ElemSetActive args) => () =>
        {
            _renderer_base.GetElement(args.ElementId).SetActive(args.Active);
        };
        private Action ElemSetTransform(RenderAction.Types.ElemSetTransform args) => () =>
        {
            _renderer_base.GetElement(args.ElementId).transform.SetLocalPositionAndRotation(
                new Vector3(args.Pos.X, args.Pos.Y, args.Pos.Z),
                new Quaternion(args.Rot.X, args.Rot.Y, args.Rot.Z, args.Rot.W)
            );
        };
        private Action CreateItem(RenderAction.Types.CreateItem args) => () =>
        {
            //if (args.SharerHash == GlobalDependency.UserInfo.LocalHash) { }
            ////TODO _ui_base.LocalItemSection.CreateItem(this, args.ElementId, new(args.Uuid.ToByteArray()));
            //else
            //    _ui_base.MemberItemSection.CreateItem(args.SharerHash, args.ElementId);
        };
        private Action DeleteItem(RenderAction.Types.DeleteItem args) => () => { };
        private Action ItemSetTitle(RenderAction.Types.ItemSetTitle args) => () => { };
        private Action ItemSetIcon(RenderAction.Types.ItemSetIcon args)
        {
            if (args.ElementId == 0)
            {
                //world environment
                if (!_static_resource_loader.TryGetValue(args.ResourceId, out var icon_resource))
                    throw new InvalidOperationException("resource not found");

                return icon_resource switch
                {
                    Image image => () => _ui_base.SetWorldIcon(image.Texture),
                    _ => () => { }
                };
            }

            return () => { };
        }
        private Action ItemSetActive(RenderAction.Types.ItemSetActive args) => () => { };
        private Action ItemAlert(RenderAction.Types.ItemAlert args) => () => { };
        private void OpenStaticResource(RenderAction.Types.OpenStaticResource args)
        {
            StaticResource resource = args.Mime switch
            {
                MIME.ImageJpeg or MIME.ImagePng => new Image(args.FileName),
                _ => throw new NotImplementedException("not implemented MIMEType"),
            };
            if (!_static_resource_loader.TryAdd(args.ResourceId, resource))
            {
                throw new InvalidOperationException("duplicate resource");
            }
        }
        private Action CloseResource(RenderAction.Types.CloseResource args) => () => { };
        private Action CreateCompositeResource(RenderAction.Types.CreateCompositeResource args) => () => { };
        private Action MemberInfo(RenderAction.Types.MemberInfo args) => () => { };
        private Action MemberSetProfile(RenderAction.Types.MemberSetProfile args) => () => { };
        private Action MemberLeave(RenderAction.Types.MemberLeave args) => () => { };
        private Action LocalInfo(RenderAction.Types.LocalInfo args) => () =>
        {
            GlobalDependency.UserInfo.LocalHash = args.LocalHash;
            GlobalDependency.UserInfo.LocalHostAurl = args.Aurl;
        };
        private Action InfoContentShared(RenderAction.Types.InfoContentShared args) => () => { };
        private Action InfoContentDeleted(RenderAction.Types.InfoContentDeleted args) => () => { };

    }
}