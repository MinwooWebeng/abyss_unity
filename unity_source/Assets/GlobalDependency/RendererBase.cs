using AbyssCLI.ABI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalDependency
{
    public class RendererBase : MonoBehaviour
    {
        // in current implementation, element is GameObject
        [HideInInspector] public Dictionary<int, DomElement> _elements;
        [HideInInspector] public DomElement _nil_root;
        [HideInInspector] public DomElement _root;
        // I have no idea what type can resources inherit in common.
        [HideInInspector] public Dictionary<int, object> _resources;
        void OnEnable()
        {
            _nil_root = new DomElement("hidden");
            _nil_root.UnityGameObject.SetActive(false);
            _root = new DomElement("root");
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
            _nil_root.Dispose();
            _root.Dispose();
            _resources = null;
        }
        public DomElement GetElement(int index) => index switch
        {
            -1 => _nil_root,
            0 => _root,
            _ => _elements[index]
        };

        //rendering action handler
        public void CreateElement(RenderAction.Types.CreateElement args)
        {
            DomElement element = new(args.ElementId.ToString());
            element.SetParent(GetElement(args.ParentId));
            _elements[args.ElementId] = element;
        }
        public void MoveElement(RenderAction.Types.MoveElement args)
        {
            GetElement(args.ElementId).SetParent(GetElement(args.NewParentId));
        }
        public void DeleteElement(RenderAction.Types.DeleteElement args)
        {
            if(_elements.Remove(args.ElementId, out var old_elem))
                old_elem.Dispose();
        }
        public void ElemSetActive(RenderAction.Types.ElemSetActive args)
        {
            GetElement(args.ElementId).UnityGameObject.SetActive(args.Active);
        }
        public void ElemSetTransform(RenderAction.Types.ElemSetTransform args)
        {
            GetElement(args.ElementId).UnityGameObject.transform.SetLocalPositionAndRotation(
                new Vector3(args.Pos.X, args.Pos.Y, args.Pos.Z),
                new Quaternion(args.Rot.X, args.Rot.Y, args.Rot.Z, args.Rot.W)
            );
        }
    }
}
