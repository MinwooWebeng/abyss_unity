using AbyssCLI.ABI;
using Google.Protobuf;
using System;
using System.Collections.Concurrent;

#nullable enable
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
            _static_resource_loader.Dispose();

            RuntimeCout.Clear();
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

            RuntimeCout.Set(_ui_base.AppendConsole);
        }
        public void HostInterpretRequestStart()
        {
            _static_resource_loader.Start();
        }

        private void InterpretRequest(RenderAction render_action)
        {
            switch (render_action.InnerCase)
            {
            case RenderAction.InnerOneofCase.ConsolePrint: RuntimeCout.Print(render_action.ConsolePrint.Text);return;
            case RenderAction.InnerOneofCase.CreateElement: RenderingActionQueue.Enqueue(() => _renderer_base.CreateElement(render_action.CreateElement));return;
            case RenderAction.InnerOneofCase.MoveElement: RenderingActionQueue.Enqueue(() => _renderer_base.MoveElement(render_action.MoveElement));return;
            case RenderAction.InnerOneofCase.DeleteElement: RenderingActionQueue.Enqueue(() => _renderer_base.DeleteElement(render_action.DeleteElement));return;
            case RenderAction.InnerOneofCase.ElemSetActive: RenderingActionQueue.Enqueue(() => _renderer_base.ElemSetActive(render_action.ElemSetActive));return;
            case RenderAction.InnerOneofCase.ElemSetTransform: RenderingActionQueue.Enqueue(() => _renderer_base.ElemSetTransform(render_action.ElemSetTransform));return;
            case RenderAction.InnerOneofCase.ElemAttachResource: ElemAttachResource(render_action.ElemAttachResource);return;
            case RenderAction.InnerOneofCase.ElemDetachResource: ElemDetachResource(render_action.ElemDetachResource);return;
            case RenderAction.InnerOneofCase.CreateItem: RenderingActionQueue.Enqueue(CreateItem(render_action.CreateItem));return;
            case RenderAction.InnerOneofCase.DeleteItem: RenderingActionQueue.Enqueue(DeleteItem(render_action.DeleteItem));return;
            case RenderAction.InnerOneofCase.ItemSetTitle: RenderingActionQueue.Enqueue(ItemSetTitle(render_action.ItemSetTitle));return;
            case RenderAction.InnerOneofCase.ItemSetIcon: RenderingActionQueue.Enqueue(ItemSetIcon(render_action.ItemSetIcon));return;
            case RenderAction.InnerOneofCase.ItemSetActive: RenderingActionQueue.Enqueue(ItemSetActive(render_action.ItemSetActive));return;
            case RenderAction.InnerOneofCase.ItemAlert: RenderingActionQueue.Enqueue(ItemAlert(render_action.ItemAlert));return;
            case RenderAction.InnerOneofCase.OpenStaticResource: OpenStaticResource(render_action.OpenStaticResource);return;
            case RenderAction.InnerOneofCase.CreateCompositeResource: RenderingActionQueue.Enqueue(CreateCompositeResource(render_action.CreateCompositeResource));return;
            case RenderAction.InnerOneofCase.CloseResource: RenderingActionQueue.Enqueue(CloseResource(render_action.CloseResource));return;
            case RenderAction.InnerOneofCase.MemberInfo: RenderingActionQueue.Enqueue(MemberInfo(render_action.MemberInfo));return;
            case RenderAction.InnerOneofCase.MemberLeave: RenderingActionQueue.Enqueue(MemberLeave(render_action.MemberLeave));return;
            case RenderAction.InnerOneofCase.MemberSetProfile: RenderingActionQueue.Enqueue(MemberSetProfile(render_action.MemberSetProfile));return;
            case RenderAction.InnerOneofCase.LocalInfo: RenderingActionQueue.Enqueue(LocalInfo(render_action.LocalInfo));return;
            case RenderAction.InnerOneofCase.InfoContentShared: RenderingActionQueue.Enqueue(InfoContentShared(render_action.InfoContentShared));return;
            case RenderAction.InnerOneofCase.InfoContentDeleted: RenderingActionQueue.Enqueue(InfoContentDeleted(render_action.InfoContentDeleted));return;
            default: StderrQueue.Enqueue("Executor: invalid RenderAction: " + render_action.InnerCase);return;
            }
        }
        private void ElemAttachResource(RenderAction.Types.ElemAttachResource args)
        {
            if (!_static_resource_loader.TryGetValue(args.ResourceId, out var resource))
                throw new Exception("ElemAttachResource: resource not found");

            RenderingActionQueue.Enqueue(
                () => _renderer_base.GetElement(args.ElementId).AttachResource(args.ResourceId, resource, args.Role));
        }
        private void ElemDetachResource(RenderAction.Types.ElemDetachResource args) =>
            RenderingActionQueue.Enqueue(() => _renderer_base.GetElement(args.ElementId).DetachResource(args.ResourceId));
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
            if (args.ElementId == 0) //world environment
            {
                if (args.ResourceId == 0) //default icon
                    return _ui_base.ClearWorldIcon;

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
                MIME.ModelObj or MIME.ApplicationXTgif => new Mesh(args.FileName), //application/x-tgif is returned from legacy web servers that expect to serve x-11 .obj
                MIME.ImageJpeg or MIME.ImagePng => new Image(args.FileName),
                _ => new UnknownResource(args.FileName, args.Mime),
            };
            RenderingActionQueue.Enqueue(resource.Init);
            _static_resource_loader.Add(args.ResourceId, resource);
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