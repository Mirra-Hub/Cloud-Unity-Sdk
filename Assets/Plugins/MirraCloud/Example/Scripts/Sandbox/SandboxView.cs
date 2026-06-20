using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// Owns the UI Toolkit DOM: a persistent status bar (config + auth state), an inline
    /// Auth panel, an auth-gated home card grid, per-module screens, toasts, and a history
    /// overlay. Raises intent events; the app layer owns the SDK calls.
    /// </summary>
    public sealed class SandboxView
    {
        private readonly VisualElement _root;
        private readonly List<ModuleDescriptor> _modules;
        private readonly string _project, _branch, _platform, _url;

        private VisualElement _content;
        private Label _authPill;
        private Button _logoutBtn;

        private VisualElement _toastHost;
        private VisualElement _historyOverlay;
        private bool _historyVisible;

        private bool _authed;

        private Label _output;
        private readonly List<Button> _controlButtons = new List<Button>();
        private readonly List<Button> _authButtons = new List<Button>();
        private readonly List<VisualElement> _cards = new List<VisualElement>();
        private Label _gateNote;

        private TextField _email, _password;

        public event Action GuestLoginRequested;
        public event Action DeviceLoginRequested;
        public event Action<string, string> EmailLoginRequested;
        public event Action LogoutRequested;
        public event Action HistoryToggleRequested;
        public event Action<ControlDescriptor> InvokeRequested;

        public SandboxView(VisualElement root, List<ModuleDescriptor> modules,
            string project, string branch, string platform, string url)
        {
            _root = root;
            _modules = modules;
            _project = project;
            _branch = branch;
            _platform = platform;
            _url = url;
            Build();
        }

        private void Build()
        {
            var screen = _root.Q<VisualElement>("screen-root") ?? _root;
            screen.Clear();

            screen.Add(BuildTopBar());

            _content = New("content");
            screen.Add(_content);

            _toastHost = New("toast-host");
            _toastHost.pickingMode = PickingMode.Ignore;
            screen.Add(_toastHost);

            _historyOverlay = BuildHistoryOverlay();
            _historyOverlay.style.display = DisplayStyle.None;
            screen.Add(_historyOverlay);
        }

        private VisualElement BuildTopBar()
        {
            var bar = New("topbar");

            var brand = new Label("BRIDGE · SANDBOX");
            brand.AddToClassList("brand");
            bar.Add(brand);

            bar.Add(Chip("project", _project));
            bar.Add(Chip("branch", _branch));
            bar.Add(Chip("platform", _platform));

            bar.Add(New("spacer"));

            _authPill = new Label("…");
            _authPill.AddToClassList("status-pill");
            bar.Add(_authPill);

            var historyBtn = new Button(() => HistoryToggleRequested?.Invoke()) { text = "History" };
            historyBtn.AddToClassList("btn");
            bar.Add(historyBtn);

            _logoutBtn = new Button(() => LogoutRequested?.Invoke()) { text = "Logout" };
            _logoutBtn.AddToClassList("btn");
            _logoutBtn.style.display = DisplayStyle.None;
            bar.Add(_logoutBtn);

            return bar;
        }

        private VisualElement Chip(string key, string value)
        {
            var chip = New("config-chip");
            bool empty = string.IsNullOrEmpty(value);
            var k = new Label(key);
            k.AddToClassList("config-chip__key");
            var v = new Label(empty ? "—" : value);
            v.AddToClassList("config-chip__val");
            if (empty) v.AddToClassList("config-chip__val--missing");
            chip.Add(k);
            chip.Add(v);
            return chip;
        }

        // ---- HOME ----

        public void ShowHome()
        {
            _content.Clear();
            _output = null;
            _controlButtons.Clear();
            _authButtons.Clear();
            _cards.Clear();

            _content.Add(BuildAuthPanel());

            var panel = New("panel");
            _gateNote = new Label("Login required — authenticate above to enable the modules.");
            _gateNote.AddToClassList("gate-note");
            panel.Add(_gateNote);

            var grid = New("card-grid");
            foreach (var m in _modules)
            {
                grid.Add(BuildCard(m));
            }
            panel.Add(grid);
            _content.Add(panel);

            ApplyGate();
        }

        private VisualElement BuildAuthPanel()
        {
            var p = New("auth-panel");

            var title = new Label("Authentication");
            title.AddToClassList("section-title");
            p.Add(title);

            var row1 = New("controls");
            row1.Add(AuthBtn("Login Guest", "btn--primary", () => GuestLoginRequested?.Invoke()));
            row1.Add(AuthBtn("Login Device", null, () => DeviceLoginRequested?.Invoke()));
            p.Add(row1);

            var row2 = New("auth-email-row");
            _email = new TextField("email");
            _email.AddToClassList("field");
            _password = new TextField("password") { isPasswordField = true };
            _password.AddToClassList("field");
            var emailBtn = AuthBtn("Login Email", null, () => EmailLoginRequested?.Invoke(_email.value, _password.value));
            row2.Add(_email);
            row2.Add(_password);
            row2.Add(emailBtn);
            p.Add(row2);

            return p;
        }

        private Button AuthBtn(string label, string extraClass, Action onClick)
        {
            var b = new Button(onClick) { text = label };
            b.AddToClassList("btn");
            if (!string.IsNullOrEmpty(extraClass)) b.AddToClassList(extraClass);
            _authButtons.Add(b);
            return b;
        }

        private VisualElement BuildCard(ModuleDescriptor m)
        {
            var card = New("card");

            var chip = New("card-icon");
            chip.style.backgroundColor = new Color(m.Accent.r, m.Accent.g, m.Accent.b, 0.16f);
            var glyph = new Label(m.Glyph);
            glyph.AddToClassList("card-glyph");
            glyph.style.color = m.Accent;
            chip.Add(glyph);

            var label = new Label(m.Title);
            label.AddToClassList("card-label");
            label.style.color = m.Accent;

            card.Add(chip);
            card.Add(label);
            card.RegisterCallback<ClickEvent>(_ =>
            {
                if (_authed) ShowModule(m);
            });
            _cards.Add(card);
            return card;
        }

        private void ApplyGate()
        {
            foreach (var c in _cards)
            {
                c.SetEnabled(_authed);
                c.EnableInClassList("card--disabled", !_authed);
            }
            if (_gateNote != null) _gateNote.style.display = _authed ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // ---- MODULE ----

        public void ShowModule(ModuleDescriptor m)
        {
            _content.Clear();
            _controlButtons.Clear();
            _cards.Clear();
            _gateNote = null;

            var panel = New("panel");

            var header = New("module-header");
            var back = new Button(ShowHome) { text = "‹" };
            back.AddToClassList("back-btn");
            var title = new Label(m.Title);
            title.AddToClassList("module-title");
            title.style.color = m.Accent;
            header.Add(back);
            header.Add(title);
            panel.Add(header);

            if (m.Info != null)
            {
                var info = new Label(m.Info());
                info.AddToClassList("info-block");
                panel.Add(info);
            }

            var controls = New("controls");
            foreach (var c in m.Controls)
            {
                var local = c;
                var b = new Button(() => InvokeRequested?.Invoke(local)) { text = c.Label };
                b.AddToClassList("btn");
                if (c.Destructive) b.AddToClassList("btn--danger");
                _controlButtons.Add(b);
                controls.Add(b);
            }
            panel.Add(controls);

            var outScroll = new ScrollView(ScrollViewMode.Vertical);
            outScroll.AddToClassList("output-scroll");
            _output = new Label("—");
            _output.AddToClassList("output");
            _output.enableRichText = false;
            outScroll.Add(_output);
            panel.Add(outScroll);

            _content.Add(panel);
        }

        // ---- history overlay ----

        private VisualElement BuildHistoryOverlay()
        {
            var overlay = New("history-overlay");
            var head = New("history-head");
            var t = new Label("Request history");
            t.AddToClassList("section-title");
            var close = new Button(() => HistoryToggleRequested?.Invoke()) { text = "✕" };
            close.AddToClassList("back-btn");
            head.Add(t);
            head.Add(New("spacer"));
            head.Add(close);
            overlay.Add(head);

            var list = new ScrollView(ScrollViewMode.Vertical);
            list.name = "history-list";
            list.AddToClassList("history-list");
            overlay.Add(list);
            return overlay;
        }

        public void ShowHistory(IReadOnlyList<OpResult> entries)
        {
            _historyVisible = !_historyVisible;
            _historyOverlay.style.display = _historyVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!_historyVisible) return;

            var list = _historyOverlay.Q<ScrollView>("history-list");
            list.Clear();
            if (entries == null || entries.Count == 0)
            {
                var empty = new Label("(no calls yet)");
                empty.AddToClassList("history-empty");
                list.Add(empty);
                return;
            }
            foreach (var e in entries)
            {
                var row = New("history-row");
                var dot = New("history-dot");
                dot.EnableInClassList("history-dot--ok", e.Ok);
                dot.EnableInClassList("history-dot--bad", !e.Ok);
                var label = new Label((e.Label ?? "call") + "  ·  " + (e.Status ?? ""));
                label.AddToClassList("history-row__label");
                var route = new Label(((e.Method ?? "") + " " + (e.Route ?? "")).Trim());
                route.AddToClassList("history-row__route");
                var col = New("history-row__col");
                col.Add(label);
                col.Add(route);
                row.Add(dot);
                row.Add(col);
                list.Add(row);
            }
        }

        // ---- state updates ----

        public void SetAuthed(bool authed)
        {
            _authed = authed;
            if (_authPill != null)
            {
                _authPill.text = authed ? "logged in" : "not logged in";
                _authPill.EnableInClassList("status-pill--ok", authed);
                _authPill.EnableInClassList("status-pill--bad", !authed);
            }
            if (_logoutBtn != null) _logoutBtn.style.display = authed ? DisplayStyle.Flex : DisplayStyle.None;
            ApplyGate();
        }

        public void SetOutput(OpResult r)
        {
            if (_output == null) return;
            _output.text = (r.Status ?? "") + "\n\n" + (r.Body ?? "");
            _output.EnableInClassList("output--ok", r.Ok);
            _output.EnableInClassList("output--bad", !r.Ok);
        }

        public void SetOutputBusy(string label)
        {
            if (_output == null) return;
            _output.text = "… running " + label;
            _output.EnableInClassList("output--ok", false);
            _output.EnableInClassList("output--bad", false);
        }

        public void SetBusy(bool busy)
        {
            foreach (var b in _authButtons) b.SetEnabled(!busy);
            foreach (var b in _controlButtons) b.SetEnabled(!busy);
        }

        public void Toast(string msg, bool ok)
        {
            if (_toastHost == null) return;
            var t = new Label(msg);
            t.AddToClassList("toast");
            t.EnableInClassList("toast--ok", ok);
            t.EnableInClassList("toast--bad", !ok);
            _toastHost.Add(t);
            t.schedule.Execute(() =>
            {
                if (t.parent != null) t.RemoveFromHierarchy();
            }).StartingIn(2800);
        }

        // ---- helpers ----

        private static VisualElement New(string cls)
        {
            var e = new VisualElement();
            if (!string.IsNullOrEmpty(cls)) e.AddToClassList(cls);
            return e;
        }
    }
}
