using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIBase : MonoBehaviour
{
    private bool _is_active;

    [SerializeField] private UIDocument uiDocument;
    public Texture2D defaultItemIcon;
    public Texture2D defaultMemberProfile;

    public Func<UnityEngine.Transform> GetContentSpawnPos;

    private VisualElement root;
    private TextField addressBar;
    private TextField sub_addressBar;
    private Label localAddrLabel;
    private Label extraLabel; //TODO
    private TextField consoleInputBar;

    public LocalItemSection LocalItemSection;
    public MemberItemSection MemberItemSection;
    public MemberProfileSection MemberProfileSection;

    //callback reservation
    public Action<string> OnAddressBarSubmit;
    public Action<string> OnSubAddressBarSubmit;
    public Action<string> OnConsoleCommand;

    //console
    private LinkedList<string> _console_lines;
    private bool _is_console_updated;

    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        addressBar = UQueryExtensions.Q<TextField>(root, "address-bar");
        addressBar.RegisterCallback<KeyDownEvent>((x) =>
        {
            if (x.keyCode == KeyCode.Return)
            {
                OnAddressBarSubmit(addressBar.value);
            }
        });

        sub_addressBar = UQueryExtensions.Q<TextField>(root, "sub-address-bar");
        sub_addressBar.RegisterCallback<KeyDownEvent>((x) =>
        {
            if (x.keyCode == KeyCode.Return)
            {
                OnSubAddressBarSubmit(sub_addressBar.value);
            }
        });

        localAddrLabel = UQueryExtensions.Q<Label>(root, "info");

        extraLabel = UQueryExtensions.Q<Label>(root, "info-more");

        consoleInputBar = UQueryExtensions.Q<TextField>(root, "console-input-bar");
        consoleInputBar.RegisterCallback<KeyDownEvent>((x) =>
        {
            if (x.keyCode == KeyCode.Return)
            {
                OnConsoleCommand(consoleInputBar.value);
            }
        });

        LocalItemSection = new(UQueryExtensions.Q(root, "itembar"), defaultItemIcon);

        MemberItemSection = new(UQueryExtensions.Q(root, "memberitemsection"), defaultItemIcon);

        MemberProfileSection = new(UQueryExtensions.Q(root, "memberprofilesection"), defaultMemberProfile);
        MemberProfileSection.RegisterClickCallback((string peer_hash) =>
        {
            MemberItemSection.Show(peer_hash);
        });

        if (localAddrLabel == null || extraLabel == null)
        {
            Debug.LogError("UI components not found!");
        }

        OnAddressBarSubmit = (arg) => { };
        OnSubAddressBarSubmit = (arg) => { };
        OnConsoleCommand = (arg) => { };

        _console_lines = new();
        _is_console_updated = false;

        Deactivate();
    }
    private void Update()
    {
        if (_is_active && _is_console_updated)
        {
            extraLabel.text = string.Join("\n", _console_lines);
            _is_console_updated = false;
        }
    }
    void OnDisable()
    {
        OnAddressBarSubmit = null;
        OnSubAddressBarSubmit = null;
        OnConsoleCommand = null;

        _console_lines = null;
    }
    public void Activate()
    {
        _is_active = true;
        root.visible = true;
        addressBar.focusable = true;
    }
    public void Deactivate()
    {
        _is_active = false;
        MemberItemSection.Hide();
        root.visible = false;
        addressBar.focusable = false;
    }
    public void AppendConsole(string line)
    {
        _ = _console_lines.AddLast(line);
        if (_console_lines.Count == 100)
        {
            _console_lines.RemoveFirst();
        }
    }
}