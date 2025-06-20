using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Executor executor;

    public Func<UnityEngine.Transform> GetContentSpawnPos;

    private VisualElement root;
    private TextField addressBar;
    private Label localAddrLabel;
    private TextField sub_addressBar;
    private Label extraLabel; //TODO
    private VisualElement itemBar;
    void Awake()
    {
        root = uiDocument.rootVisualElement;

        addressBar = UQueryExtensions.Q<TextField>(root, "address-bar");
        addressBar.RegisterCallback<KeyDownEvent>((x) =>
        {
            if (x.keyCode == KeyCode.Return)
            {
                AddressBarSubmit(addressBar.value);
            }
        });

        sub_addressBar = UQueryExtensions.Q<TextField>(root, "sub-address-bar");
        sub_addressBar.RegisterCallback<KeyDownEvent>((x) =>
        {
            if (x.keyCode == KeyCode.Return)
            {
                SubAddressBarSubmit(sub_addressBar.value);
            }
        });

        localAddrLabel = UQueryExtensions.Q<Label>(root, "info");
        if (localAddrLabel == null)
        {
            Debug.LogError("addr label not found!");
        }
        executor.SetLocalAddrCallback = (addr) => { localAddrLabel.text = addr; };

        extraLabel = UQueryExtensions.Q<Label>(root, "info-more");
        if (extraLabel == null)
        {
            Debug.LogError("additional info label not found!");
        }
        executor.SetAdditionalInfoCallback = (info) => { extraLabel.text = info; };

        itemBar = UQueryExtensions.Q(root, "itembar");
        if (itemBar == null)
        {
            Debug.LogError("item bar not found!");
        }

        Deactivate();
    }
    public void Activate()
    {
        root.visible = true;
        addressBar.focusable = true;
    }
    public void Deactivate()
    {
        root.visible = false;
        addressBar.focusable = false;
    }
    void AddressBarSubmit(string address)
    {
        executor.MoveWorld(address);
    }
    void SubAddressBarSubmit(string address)
    {
        if (address.StartsWith("connect "))
        {
            var conn_addr = address["connect ".Length..];
            executor.ConnectPeer(conn_addr);
            return;
        }
        var transform = GetContentSpawnPos();
        var uuid = Guid.NewGuid();
        executor.ShareContent(uuid, address, transform.position, transform.rotation);
        AddItemElement(uuid);
    }
    private void AddItemElement(Guid uuid)
    {
        Debug.Log("adding icon!");
        ItemIcon newElement = new(uuid);
        newElement.RegisterCloseCallback(() =>
        {
            executor.UnshareContent(uuid);
            Debug.Log("icon close callback; TODO: retrieve the content");
        });
        newElement.AddToClassList("item-icon"); // Assigns a CSS-like class tag
        itemBar.Add(newElement);
    }
}
