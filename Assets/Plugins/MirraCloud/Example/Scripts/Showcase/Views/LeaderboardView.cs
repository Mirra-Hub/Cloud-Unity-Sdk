using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.Leaderboard.Dto;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Leaderboard detail: a tab per configured board (from InitializeAsync), then the selected
    /// board's config chips and a ranked entries table (medal-tinted ranks, avatar + score).
    /// </summary>
    public sealed class LeaderboardView : ServiceView
    {
        private readonly List<Button> _tabs = new List<Button>();
        private VisualElement _boardSlot;

        public LeaderboardView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var pickerSlot = AddSlot(12f);
            _boardSlot = AddSlot();

            ViewBind.Load(
                Sdk.Leaderboard.InitializeAsync(),
                pickerSlot,
                BuildPicker,
                isEmpty: c => c == null || c.Length == 0,
                emptyView: () => EmptyState.Build("▲", "No leaderboards configured for this project"));
        }

        private VisualElement BuildPicker(LeaderboardConfigDto[] configs)
        {
            _tabs.Clear();

            var tabs = new VisualElement();
            tabs.AddToClassList("sc-tabs");

            foreach (var cfg in configs)
            {
                var captured = cfg;
                var btn = new Button { text = string.IsNullOrEmpty(cfg.name) ? cfg.key : cfg.name };
                btn.AddToClassList("sc-tab");
                btn.clicked += () => Select(captured, btn);
                _tabs.Add(btn);
                tabs.Add(btn);
            }

            if (_tabs.Count > 0)
            {
                Select(configs[0], _tabs[0]);
            }
            return tabs;
        }

        private void Select(LeaderboardConfigDto cfg, Button active)
        {
            foreach (var b in _tabs)
            {
                b.EnableInClassList("sc-tab--active", b == active);
            }
            RenderBoard(cfg);
        }

        private void RenderBoard(LeaderboardConfigDto cfg)
        {
            _boardSlot.Clear();

            var meta = new VisualElement();
            meta.AddToClassList("sc-chip-row");
            meta.style.marginBottom = 12;
            meta.Add(new Chip(cfg.orderType.ToString(), ChipTone.Info));
            meta.Add(new Chip(cfg.type.ToString(), ChipTone.Neutral));
            meta.Add(new Chip(cfg.updateStrategy.ToString(), ChipTone.Neutral));
            if (cfg.isReset)
            {
                meta.Add(new Chip("Resets " + cfg.resetIntervalType, ChipTone.Warn));
                if (cfg.nextResetDate.HasValue)
                {
                    meta.Add(new CountdownChip(cfg.nextResetDate.Value.ToUniversalTime()));
                }
            }
            _boardSlot.Add(meta);

            var tableSlot = new VisualElement();
            _boardSlot.Add(tableSlot);

            ViewBind.Load(
                Sdk.Leaderboard.GetLeaderboardTopEntries(cfg.id, 100),
                tableSlot,
                BuildTable,
                isEmpty: d => d == null || d.entries == null || d.entries.Length == 0,
                emptyView: () => EmptyState.Build("▲", "No scores submitted yet"));
        }

        private VisualElement BuildTable(LeaderboardEntriesDto data)
        {
            var cols = new[]
            {
                new DataColumn { Header = "#", FixedWidth = true, Px = 52, Align = "center", Cell = RankCell },
                new DataColumn { Header = "PLAYER", Grow = 1f, Cell = PlayerCell },
                new DataColumn { Header = "SCORE", FixedWidth = true, Px = 120, Align = "right", Cell = ScoreCell },
            };

            var table = new DataTable(cols);
            table.Bind(data.entries);
            return table;
        }

        private static VisualElement RankCell(object row)
        {
            var e = (LeaderboardEntryDto)row;
            var l = new Label("#" + e.position);
            l.AddToClassList("sc-rank");
            if (e.position == 1)
            {
                l.style.color = new Color(0.91f, 0.78f, 0.32f);
            }
            else if (e.position == 2)
            {
                l.style.color = new Color(0.76f, 0.78f, 0.84f);
            }
            else if (e.position == 3)
            {
                l.style.color = new Color(0.82f, 0.54f, 0.32f);
            }
            return l;
        }

        private VisualElement PlayerCell(object row)
        {
            var e = (LeaderboardEntryDto)row;

            var wrap = new VisualElement();
            wrap.style.flexDirection = FlexDirection.Row;
            wrap.style.alignItems = Align.Center;

            var label = string.IsNullOrEmpty(e.playerName) ? e.playerId : e.playerName;
            var av = new Avatar(26f);
            av.SetInitialsFor(label);
            av.style.marginRight = 10;
            wrap.Add(av);

            wrap.Add(new Label(string.IsNullOrEmpty(label) ? "—" : label));
            return wrap;
        }

        private static VisualElement ScoreCell(object row)
        {
            var e = (LeaderboardEntryDto)row;
            var l = new Label(e.value.ToString("#,0.##"));
            l.AddToClassList("sc-score");
            return l;
        }
    }
}
