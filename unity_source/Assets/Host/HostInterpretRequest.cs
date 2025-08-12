using AbyssCLI.ABI;

namespace Host
{
    partial class Host
    {
        private void InterpretRequest(RenderAction render_action)
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
            default: StderrQueue.Enqueue("fatal:::invalid RenderAction: " + render_action.InnerCase); return;
            }
        }
        private void ConsolePrint(RenderAction.Types.ConsolePrint args)
        {

        }
        private void CreateElement(RenderAction.Types.CreateElement args) { }
        private void MoveElement(RenderAction.Types.MoveElement args) { }
        private void DeleteElement(RenderAction.Types.DeleteElement args) { }
        private void ElemSetActive(RenderAction.Types.ElemSetActive args) { }
        private void ElemSetTransform(RenderAction.Types.ElemSetTransform args) { }
        private void CreateItem(RenderAction.Types.CreateItem args) { }
        private void DeleteItem(RenderAction.Types.DeleteItem args) { }
        private void ItemSetTitle(RenderAction.Types.ItemSetTitle args) { }
        private void ItemSetIcon(RenderAction.Types.ItemSetIcon args) { }
        private void ItemSetActive(RenderAction.Types.ItemSetActive args) { }
        private void ItemAlert(RenderAction.Types.ItemAlert args) { }
        private void OpenStaticResource(RenderAction.Types.OpenStaticResource args) { }
        private void CloseResource(RenderAction.Types.CloseResource args) { }
        private void CreateCompositeResource(RenderAction.Types.CreateCompositeResource args) { }
        private void MemberInfo(RenderAction.Types.MemberInfo args) { }
        private void MemberSetProfile(RenderAction.Types.MemberSetProfile args) { }
        private void MemberLeave(RenderAction.Types.MemberLeave args) { }
        private void CreateImage(RenderAction.Types.CreateImage args) { }
        private void DeleteImage(RenderAction.Types.DeleteImage args) { }
        private void CreateMaterialV(RenderAction.Types.CreateMaterialV args) { }
        private void CreateMaterialF(RenderAction.Types.CreateMaterialF args) { }
        private void MaterialSetParamV(RenderAction.Types.MaterialSetParamV args) { }
        private void MaterialSetParamC(RenderAction.Types.MaterialSetParamC args) { }
        private void DeleteMaterial(RenderAction.Types.DeleteMaterial args) { }
        private void CreateStaticMesh(RenderAction.Types.CreateStaticMesh args) { }
        private void StaticMeshSetMaterial(RenderAction.Types.StaticMeshSetMaterial args) { }
        private void ElemAttachStaticMesh(RenderAction.Types.ElemAttachStaticMesh args) { }
        private void DeleteStaticMesh(RenderAction.Types.DeleteStaticMesh args) { }
        private void CreateAnimation(RenderAction.Types.CreateAnimation args) { }
        private void DeleteAnimation(RenderAction.Types.DeleteAnimation args) { }
        private void LocalInfo(RenderAction.Types.LocalInfo args) { }
        private void InfoContentShared(RenderAction.Types.InfoContentShared args) { }
        private void InfoContentDeleted(RenderAction.Types.InfoContentDeleted args) { }

    }
}