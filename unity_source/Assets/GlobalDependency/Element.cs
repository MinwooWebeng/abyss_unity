using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalDependency
{
    public class DomElement : IDisposable
    {
        public GameObject UnityGameObject;
        private Dictionary<int, (object, ResourceRole)> _attached_resource_roles = new();
        public DomElement(string editor_name)
        {
            UnityGameObject = new(editor_name);
        }
        public void SetParent(DomElement parent) =>
            UnityGameObject.transform.SetParent(parent.UnityGameObject.transform, false);
        
        
        public void Dispose()
        {
            GameObject.Destroy(UnityGameObject);
        }

        public void RecordActiveResourceRole(int resource_id, ResourceRole role)
        {
            //if (!_attached_resource_roles.TryAdd(resource_id, role))
            //    throw new Exception("tried to attach resource twice");
        }
        public void RemoveRole(int resource_id)
        {
            //switch (_attached_resource_roles[resource_id])
            //{
            //case ResourceRole.Mesh:
            //    UnityEngine.Object.Destroy(UnityGameObject.GetComponent<MeshFilter>());
            //    UnityEngine.Object.Destroy(UnityGameObject.GetComponent<MeshRenderer>());
            //    break;
            //default:
            //    break;
            //}
        }
    }
}