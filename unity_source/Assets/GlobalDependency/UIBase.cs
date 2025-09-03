using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GlobalDependency
{
    public class UIBase : MonoBehaviour
    {
        private bool _is_active;

        //editor
        [SerializeField] private UIDocument uiDocument;
        public Texture2D defaultItemIcon;
        public Texture2D defaultMemberProfile;

        //OnEnable
        private VisualElement root;
        private TextField addressBar;
        private TextField sub_addressBar;
        private Label localAddrLabel;
        private Label extraLabel; //TODO
        private TextField consoleInputBar;

        [HideInInspector] public LocalItemSection LocalItemSection;
        [HideInInspector] public MemberItemSection MemberItemSection;
        [HideInInspector] public MemberProfileSection MemberProfileSection;

        //callback reservation
        [HideInInspector] public Action<string> OnAddressBarSubmit;
        [HideInInspector] public Action<string> OnSubAddressBarSubmit;
        [HideInInspector] public Action<string> OnConsoleCommand;

        //console
        private LinkedList<string> _console_lines;
        private bool _is_console_updated;

        void OnEnable()
        {
            //locate all elements
            root = uiDocument.rootVisualElement;

            addressBar = UQueryExtensions.Q<TextField>(root, "address-bar");
            addressBar.RegisterCallback<KeyDownEvent>(x =>
            {
                if (x.keyCode == KeyCode.Return)
                    OnAddressBarSubmit(addressBar.value);
            });

            sub_addressBar = UQueryExtensions.Q<TextField>(root, "sub-address-bar");
            sub_addressBar.RegisterCallback<KeyDownEvent>(x =>
            {
                if (x.keyCode == KeyCode.Return)
                    OnSubAddressBarSubmit(sub_addressBar.value);
            });

            localAddrLabel = UQueryExtensions.Q<Label>(root, "info");

            extraLabel = UQueryExtensions.Q<Label>(root, "info-more");

            consoleInputBar = UQueryExtensions.Q<TextField>(root, "console-input-bar");
            consoleInputBar.RegisterCallback<KeyDownEvent>(x =>
            {
                if (x.keyCode == KeyCode.Return)
                    OnConsoleCommand(consoleInputBar.value);
            });

            LocalItemSection = new(UQueryExtensions.Q(root, "itembar"), defaultItemIcon);

            MemberItemSection = new(UQueryExtensions.Q(root, "memberitemsection"), defaultItemIcon);

            MemberProfileSection = new(UQueryExtensions.Q(root, "memberprofilesection"), defaultMemberProfile);
            MemberProfileSection.RegisterClickCallback(peer_hash =>
            {
                MemberItemSection.Show(peer_hash);
            });

            if (localAddrLabel == null || extraLabel == null)
            {
                Debug.LogError("UI components not found!");
            }

            //default event handler - this should not be called.
            OnAddressBarSubmit = (arg) => { };
            OnSubAddressBarSubmit = (arg) => { };
            OnConsoleCommand = (arg) => { };

            _console_lines = new();
            _is_console_updated = false;

            var null_mem = FirstNullMemberName();
            if (null_mem != string.Empty)
                Debug.Log("haha " + null_mem + " is null");

            Deactivate();
        }
        public string FirstNullMemberName()
        {
            var fields = this.GetType().GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (var field in fields)
                if (field.GetValue(this) == null)
                    return field.Name;

            return string.Empty;
        }
        private void Update()
        {
            if (_is_active && _is_console_updated)
            {
                lock (_console_lines)
                {
                    extraLabel.text = string.Join("\n", _console_lines);
                }
                _is_console_updated = false;
            }
        }
        void OnDisable()
        {
            root = null;
            addressBar = null;
            sub_addressBar = null;
            localAddrLabel = null;
            extraLabel = null;
            consoleInputBar = null;

            LocalItemSection = null;
            MemberItemSection = null;
            MemberProfileSection = null;

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
            lock (_console_lines)
            {
                _ = _console_lines.AddLast(line);
                if (_console_lines.Count == 100)
                {
                    _console_lines.RemoveFirst();
                }
                _is_console_updated = true;
            }
        }
        public void SetWorldIcon(Texture2D texture)
        {
            root.style.backgroundImage = texture;
        }
        public void ClearWorldIcon()
        {
            root.style.backgroundImage = null;
        }
    }
}