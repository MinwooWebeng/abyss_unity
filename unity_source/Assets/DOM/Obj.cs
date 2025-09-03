using GlobalDependency;
using Host;
using UnityEngine;

#nullable enable
namespace DOM
{
    public sealed class Obj : DomElement
    {
        public readonly GameObject GameObject;
        public readonly MeshFilter MeshFilter;
        public readonly MeshRenderer MeshRenderer;
        public Obj(RendererBase renderer_base, int element_id) : base(renderer_base, element_id)
        {
            GameObject = new GameObject(element_id.ToString());
            MeshFilter = GameObject.AddComponent<MeshFilter>();
            MeshRenderer = GameObject.AddComponent<MeshRenderer>();
        }
        public override T? GetThing<T>() where T : class
        {
            object? result = typeof(T) switch
            {
                var t when t == typeof(GameObject) => GameObject,
                var t when t == typeof(MeshFilter) => MeshFilter,
                var t when t == typeof(MeshRenderer) => MeshRenderer,
                _ => null
            };
            return (T?)result;
        }
        protected override void AfterAppendingChild(DomElement child)
        {
            MeshRenderer.material = (child as Pbrm)!.Material;
        }
        protected override void AfterRemovingChild(DomElement child)
        {
            MeshRenderer.material = null;
        }
        protected override void ResourceAttachingCallback(ResourceRole role, StaticResource resource)
        {
            Host.Mesh mesh = (resource as Host.Mesh)!;
            MeshFilter.mesh = mesh.UnityMesh;
        }
        protected override void ResourceReplacingCallback(ResourceRole role, StaticResource resource) =>
            ResourceAttachingCallback(role, resource);
        protected override void ResourceDetachingCallback(ResourceRole role)
        {
            MeshFilter.mesh = null;
        }
    }
}