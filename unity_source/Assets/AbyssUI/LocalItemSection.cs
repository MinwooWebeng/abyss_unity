using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalItemSection
{
    [HideInInspector] public readonly VisualElement _icon_container;
    private readonly Dictionary<int, ItemIcon> _items;
    private readonly Texture2D _default_icon;
    public Action<Guid> OnCloseCallback;
    public LocalItemSection(VisualElement visual_element, Texture2D default_icon)
    {
        _icon_container = visual_element;
        _items = new();
        _default_icon = default_icon;
    }
    public void AddItem(int element_id, Guid uuid)
    {
        var item = new ItemIcon(uuid, _default_icon)
        {
            OnClose = OnCloseCallback,
        };
        _items[element_id] = item;
        _icon_container.Add(item);
    }
    public void RemoveItem(int element_id)
    {
        _ = _items.Remove(element_id, out var old);
        old.RemoveFromHierarchy();
    }
    public void UpdateIcon(int element_id, Texture2D icon)
    {
        var existing_item = _items[element_id];
        existing_item.style.backgroundImage = icon;
    }
}