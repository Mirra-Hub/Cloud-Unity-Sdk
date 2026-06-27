using System;
using MirraCloud.Core;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// WebView detail: an open-a-URL tool (sync LoadUrl + visibility/navigation) gated on IsReady, plus
    /// a live event log (page started/loaded, hooks, messages, errors). No RestApiResult here.
    /// </summary>
    public sealed class WebViewView : ServiceView
    {
        private TextField _url;
        private VisualElement _resultSlot;
        private Chip _readyChip;
        private VisualElement _log;
        private bool _logEmpty = true;

        private Action<string> _onStarted;
        private Action<string> _onLoaded;
        private Action<string> _onHooked;
        private Action<string> _onMessage;
        private Action<string> _onError;
        private Action<string> _onHttpError;

        public WebViewView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var card = new Card(Meta.Accent);
            card.WithTitle("Open a URL", Meta.Accent);

            var readyRow = new VisualElement();
            readyRow.AddToClassList("sc-chip-row");
            _readyChip = ReadyChip();
            readyRow.Add(_readyChip);
            card.Body.Add(readyRow);

            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");
            bar.style.marginTop = 8;
            _url = new TextField { label = "URL", value = "https://example.com" };
            _url.AddToClassList("sc-field");
            _url.AddToClassList("sc-chat-lookup__input");
            bar.Add(_url);
            var open = new Button(Open) { text = "Open" };
            open.AddToClassList("sc-btn");
            open.AddToClassList("sc-btn--primary");
            bar.Add(open);
            card.Body.Add(bar);

            var hint = new Label("Opens a native in-app webview overlay. Not available on WebGL / in the Editor.");
            hint.AddToClassList("sc-chat-hint");
            card.Body.Add(hint);

            var nav = new VisualElement();
            nav.AddToClassList("sc-chip-row");
            nav.Add(NavButton("Hide", () => Sdk.WebView.SetVisibility(false)));
            nav.Add(NavButton("Back", () => Sdk.WebView.GoBack()));
            nav.Add(NavButton("Forward", () => Sdk.WebView.GoForward()));
            card.Body.Add(nav);

            _resultSlot = new VisualElement();
            _resultSlot.style.marginTop = 8;
            card.Body.Add(_resultSlot);

            Content.Add(card);

            Content.Add(new SectionHeader("Events"));
            _log = AddSlot();
            Replace(_log, EmptyState.Build("WV", "No webview events yet"));

            RegisterCallback<AttachToPanelEvent>(_ => Subscribe());
            RegisterCallback<DetachFromPanelEvent>(_ => Unsubscribe());
        }

        private void Open()
        {
            _readyChip.SetText(Sdk.WebView.IsReady ? "Bridge ready" : "Webview unavailable (WebGL/Editor)");
            if (!Sdk.WebView.IsReady)
            {
                Replace(_resultSlot, new Chip("Webview not ready on this platform", ChipTone.Bad));
                return;
            }
            var url = (_url.value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(url))
            {
                Replace(_resultSlot, new Chip("Enter a URL", ChipTone.Warn));
                return;
            }
            Sdk.WebView.LoadUrl(url);
            Sdk.WebView.SetVisibility(true);
            var row = new ListRow();
            row.SetLead(new Chip("OPEN", ChipTone.Accent));
            row.SetTitle("Loading…");
            row.SetSubtitle(Fmt.Truncate(url, 60));
            Replace(_resultSlot, row);
        }

        private Chip ReadyChip()
        {
            bool ready = Sdk.WebView.IsReady;
            return new Chip(ready ? "Bridge ready" : "Webview unavailable (WebGL/Editor)", ready ? ChipTone.Ok : ChipTone.Warn);
        }

        private Button NavButton(string text, Action action)
        {
            var b = new Button(action) { text = text };
            b.AddToClassList("sc-btn");
            return b;
        }

        private void Subscribe()
        {
            _onStarted = u => AddLog("START", "Page started", u, ChipTone.Info);
            _onLoaded = u => AddLog("LOADED", "Page loaded", u, ChipTone.Ok);
            _onHooked = u => AddLog("HOOK", "URL hooked", u, ChipTone.Accent);
            _onMessage = m => AddLog("MSG", "Message", m, ChipTone.Neutral);
            _onError = e => AddLog("ERROR", "Error", e, ChipTone.Bad);
            _onHttpError = e => AddLog("HTTP", "HTTP error", e, ChipTone.Bad);

            Sdk.WebView.OnPageStarted += _onStarted;
            Sdk.WebView.OnPageLoaded += _onLoaded;
            Sdk.WebView.OnUrlHooked += _onHooked;
            Sdk.WebView.OnMessageReceived += _onMessage;
            Sdk.WebView.OnError += _onError;
            Sdk.WebView.OnHttpError += _onHttpError;
        }

        private void Unsubscribe()
        {
            Sdk.WebView.OnPageStarted -= _onStarted;
            Sdk.WebView.OnPageLoaded -= _onLoaded;
            Sdk.WebView.OnUrlHooked -= _onHooked;
            Sdk.WebView.OnMessageReceived -= _onMessage;
            Sdk.WebView.OnError -= _onError;
            Sdk.WebView.OnHttpError -= _onHttpError;
        }

        private void AddLog(string tag, string title, string payload, ChipTone tone)
        {
            // native callbacks may arrive off the main thread — marshal to the UI loop
            _log.schedule.Execute(() =>
            {
                if (_logEmpty)
                {
                    _log.Clear();
                    _logEmpty = false;
                }
                var row = new ListRow();
                row.SetLead(new Chip(tag, tone));
                row.SetTitle(title);
                row.SetSubtitle(Fmt.Truncate(payload ?? string.Empty, 70));
                _log.Insert(0, row);
            });
        }
    }
}
