using System;
using UnityEngine.UIElements;

public class ItemIcon : VisualElement
{
    public ItemIcon(Guid uuid)
    {
        this.uuid = uuid;
        VisualElement closeButton = new();
        closeButton.AddToClassList("item-icon-close"); // Assigns a CSS-like class tag
        Add(closeButton);

        closeButton.RegisterCallback<ClickEvent>(evt =>
        {
            OnClose();
            RemoveFromHierarchy();
        });
    }
    public void RegisterCloseCallback(Action callback)
    {
        OnClose = callback;
    }
    public readonly Guid uuid;
    private Action OnClose;
}