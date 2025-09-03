using AbyssCLI.ABI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalDependency
{
    public class RendererBase : MonoBehaviour
    {
        public CommonShaderLoader ShaderLoader;
        // in current implementation, element is GameObject
        [HideInInspector] public Dictionary<int, DOM.DomElement> _elements;
        [HideInInspector] public DOM.DomElement _nil_root;
        [HideInInspector] public DOM.DomElement _root;
        // I have no idea what type can resources inherit in common.
        [HideInInspector] public Dictionary<int, object> _resources;
        void OnEnable()
        {
            _nil_root = new DOM.Hidden(this, -1);
            _nil_root.GetThing<GameObject>().SetActive(false);
            _root = new DOM.O(this, 0);
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
        public DOM.DomElement GetElement(int index) => index switch
        {
            -1 => _nil_root,
            0 => _root,
            _ => _elements[index]
        };

        //rendering action handler
        public void CreateElement(RenderAction.Types.CreateElement args)
        {
            DOM.DomElement element = args.Tag switch
            {
                ElementTag.O => new DOM.O(this, args.ElementId),
                ElementTag.Obj => new DOM.Obj(this, args.ElementId),
                ElementTag.Pbrm => new DOM.Pbrm(this, args.ElementId),
                _ => throw new NotImplementedException("ElementTag not implemented")
            };
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
            GetElement(args.ElementId).GetThing<GameObject>().SetActive(args.Active);
        }
        public void ElemSetTransform(RenderAction.Types.ElemSetTransform args)
        {
            GetElement(args.ElementId).GetThing<GameObject>().transform.SetLocalPositionAndRotation(
                new Vector3(args.Pos.X, args.Pos.Y, args.Pos.Z),
                new Quaternion(args.Rot.X, args.Rot.Y, args.Rot.Z, args.Rot.W)
            );
        }
    }
}
