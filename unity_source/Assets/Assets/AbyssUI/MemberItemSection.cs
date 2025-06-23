using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MemberItemSection
{
    public readonly VisualElement visualElement;
    private readonly Dictionary<string, Dictionary<int, Texture2D>> remoteItems; // peer hash - element id - icon image
    private readonly Texture2D defaultIcon;
    private readonly Dictionary<int, string> element_id_to_sharer; //element id - peer hash
    private string current_showing_peer = "";
    public MemberItemSection(VisualElement visualElement, Texture2D defaultIcon)
    {
        this.visualElement = visualElement;
        remoteItems = new();
        this.defaultIcon = defaultIcon;
        element_id_to_sharer = new();
    }
    public void CreateItem(string peer_hash, int element_id)
    {
        if (!remoteItems.TryGetValue(peer_hash, out Dictionary<int, Texture2D> itemdict))
        {
            itemdict = new();
            remoteItems[peer_hash] = itemdict;
        }
        itemdict[element_id] = defaultIcon;
        element_id_to_sharer[element_id] = peer_hash;

        Show(current_showing_peer);
    }
    public void UpdateIcon(int element_id, Texture2D icon)
    {
        if (!element_id_to_sharer.TryGetValue(element_id, out var peer_hash))
            return;

        remoteItems[peer_hash][element_id] = icon;

        Show(current_showing_peer);
    }
    public void RemoveItem(int element_id)
    {
        if (!element_id_to_sharer.TryGetValue(element_id, out var peer_hash))
            return;
        remoteItems[peer_hash].Remove(element_id);
        element_id_to_sharer.Remove(element_id);

        Show(current_showing_peer);
    }
    public void RemovePeer(string peer_hash)
    {
        if(!remoteItems.Remove(peer_hash, out var itemdict))
        {
            return;
        }
        
        foreach (var element_id in itemdict.Keys)
        {
            element_id_to_sharer.Remove(element_id);
        }

        Show(current_showing_peer);
    }
    public void Show(string peer_hash)
    {
        visualElement.Clear();
        if (!remoteItems.TryGetValue(peer_hash, out var itemdict)) return;
        foreach (var icon in itemdict.Values)
        {
            VisualElement iconElement = new();
            iconElement.AddToClassList("member-item-icon");
            iconElement.style.backgroundImage = icon;
            visualElement.Add(iconElement);
        }
        current_showing_peer = peer_hash;
    }
    public void Hide()
    {
        Show("");
    }
}