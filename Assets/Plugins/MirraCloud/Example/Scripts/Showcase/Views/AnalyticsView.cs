using System;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Analytics detail: a write-only service — fire tools for a custom event, session-started, and
    /// playtime. Each call returns the non-generic RestApiResult (no Data), so outcomes are driven
    /// manually (OK chip + round-trip stat, or ErrorState) rather than via ViewBind.
    /// </summary>
    public sealed class AnalyticsView : ServiceView
    {
        private TextField _metric;
        private VisualElement _eventResult;
        private TextField _minutes;
        private VisualElement _quickResult;

        public AnalyticsView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            // custom event
            Content.Add(new SectionHeader("Track a custom event"));
            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");
            _metric = new TextField { label = "Metric id" };
            _metric.AddToClassList("sc-field");
            _metric.AddToClassList("sc-chat-lookup__input");
            bar.Add(_metric);
            var track = new Button(TrackEvent) { text = "Track" };
            track.AddToClassList("sc-btn");
            track.AddToClassList("sc-btn--primary");
            bar.Add(track);
            Content.Add(bar);
            var hint = new Label("Sends an analytics event for this metric id.");
            hint.AddToClassList("sc-chat-hint");
            Content.Add(hint);
            _eventResult = AddSlot();

            // quick-fire demos
            Content.Add(new SectionHeader("Quick fire"));
            var quick = new VisualElement();
            quick.AddToClassList("sc-chat-lookup");
            var session = new Button(() => Fire(Sdk.Analytics.SendSessionStartedAsync(), _quickResult, "Session started")) { text = "Session started" };
            session.AddToClassList("sc-btn");
            quick.Add(session);
            _minutes = new TextField { label = "Minutes" };
            _minutes.AddToClassList("sc-field");
            quick.Add(_minutes);
            var playtime = new Button(SendPlaytime) { text = "Send playtime" };
            playtime.AddToClassList("sc-btn");
            quick.Add(playtime);
            Content.Add(quick);
            _quickResult = AddSlot();

            Replace(_eventResult, EmptyState.Build("≈", "Enter a metric id and press Track"));
        }

        private void TrackEvent()
        {
            var id = (_metric.value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(id))
            {
                Replace(_eventResult, new Chip("Enter a metric id", ChipTone.Warn));
                return;
            }
            Fire(Sdk.Analytics.SendEventAsync(id), _eventResult, "Event sent");
        }

        private void SendPlaytime()
        {
            int m;
            if (!int.TryParse(_minutes.value, out m))
            {
                Replace(_quickResult, new Chip("Enter minutes (integer)", ChipTone.Warn));
                return;
            }
            Fire(Sdk.Analytics.SendPlaytimeAsync(m), _quickResult, "Playtime sent");
        }

        private async void Fire(AsyncOperation<RestApiResult> op, VisualElement slot, string okLabel)
        {
            if (op == null)
            {
                return;
            }
            Skeleton.Into(slot);
            await op.Task();
            var r = op.Result;

            if (r != null && r.IsSuccess)
            {
                var card = new Card(new UnityEngine.Color(0.18f, 0.81f, 0.63f));
                card.WithTitle(okLabel);
                var chips = new VisualElement();
                chips.AddToClassList("sc-chip-row");
                chips.Add(new Chip("OK " + (r.HttpStatusCode.HasValue ? r.HttpStatusCode.Value.ToString() : ""), ChipTone.Ok));
                card.Body.Add(chips);
                var stats = new VisualElement();
                stats.AddToClassList("sc-stat-grid");
                stats.style.marginTop = 8;
                stats.Add(new StatTile("Round trip", "~").Set(r.DurationMs + " ms"));
                card.Body.Add(stats);
                Replace(slot, card);
            }
            else
            {
                Replace(slot, ErrorState.Build(r != null ? r.Error : null));
            }
        }
    }
}
