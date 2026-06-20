using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// Owns the UI Toolkit DOM: a session bar, a home card grid, and per-module detail
    /// screens built generically from <see cref="ModuleDescriptor"/>s. Raises events for
    /// login / control invocation; the app layer owns the SDK calls.
    /// </summary>
    public sealed class SandboxView
    {
        private readonly VisualElement _root;
        private readonly List<ModuleDescriptor> _modules;

        private Label _status;
        private Button _loginBtn;
        private VisualElement _content;

        private Label _output;
        private readonly List<Button> _controlButtons = new List<Button>();

        public event Action LoginRequested;
        public event Action<ControlDescriptor> InvokeRequested;

        public SandboxView(VisualElement root, List<ModuleDescriptor> modules)
        {
            _root = root;
            _modules = modules;
            Build();
        }

        private void Build()
        {
            var screen = _root.Q<VisualElement>("screen-root") ?? _root;
            screen.Clear();

            var bar = New("session-bar");
            var brand = new Label("BRIDGE · SANDBOX");
            brand.AddToClassList("brand");
            var spacer = New("spacer");
            _status = new Label("…");
            _status.AddToClassList("status-pill");
            _loginBtn = new Button(() => LoginRequested?.Invoke()) { text = "Login Guest" };
            _loginBtn.AddToClassList("btn");
            _loginBtn.AddToClassList("btn--primary");
            bar.Add(brand);
            bar.Add(spacer);
            bar.Add(_status);
            bar.Add(_loginBtn);
            screen.Add(bar);

            _content = New("content");
            screen.Add(_content);
        }

        public void ShowHome()
        {
            _content.Clear();
            _output = null;
            _controlButtons.Clear();

            var panel = New("panel");
            var grid = New("card-grid");
            foreach (var m in _modules)
            {
                grid.Add(BuildCard(m));
            }
            panel.Add(grid);
            _content.Add(panel);
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
            card.RegisterCallback<ClickEvent>(_ => ShowModule(m));
            return card;
        }

        public void ShowModule(ModuleDescriptor m)
        {
            _content.Clear();
            _controlButtons.Clear();

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
                if (c.Destructive)
                {
                    b.AddToClassList("btn--danger");
                }
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

        public void SetStatus(string text, bool ok)
        {
            if (_status == null) return;
            _status.text = text;
            _status.EnableInClassList("status-pill--ok", ok);
            _status.EnableInClassList("status-pill--bad", !ok);
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
            if (_loginBtn != null) _loginBtn.SetEnabled(!busy);
            foreach (var b in _controlButtons)
            {
                b.SetEnabled(!busy);
            }
        }

        private static VisualElement New(string cls)
        {
            var e = new VisualElement();
            if (!string.IsNullOrEmpty(cls)) e.AddToClassList(cls);
            return e;
        }
    }
}
