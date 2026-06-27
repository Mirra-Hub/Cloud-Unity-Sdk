using System;
using MirraCloud.Core;
using MirraCloud.Core.Leaderboard.Dto;
using Plugins.MirraCloud.Core.Services.Tournaments.Dto;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Tournaments detail: a tab per configured tournament (InitializeAsync); each shows a config
    /// header, its leagues with rewards-for-places, the active league's standings table (top + your
    /// highlight), and your pending rewards (peeked read-only with reset=false).
    /// </summary>
    public sealed class TournamentsView : ServiceView
    {
        private readonly List<Button> _tabs = new List<Button>();
        private VisualElement _body;

        public TournamentsView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var pickerSlot = AddSlot(12f);
            _body = AddSlot();

            ViewBind.Load(
                Sdk.Tournaments.InitializeAsync(),
                pickerSlot,
                BuildPicker,
                isEmpty: c => c == null || c.Length == 0,
                emptyView: () => EmptyState.Build("TR", "No tournaments configured for this project"));
        }

        private VisualElement BuildPicker(TournamentConfigDto[] configs)
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

        private void Select(TournamentConfigDto cfg, Button active)
        {
            foreach (var b in _tabs)
            {
                b.EnableInClassList("sc-tab--active", b == active);
            }
            RenderDetail(cfg);
        }

        private void RenderDetail(TournamentConfigDto cfg)
        {
            _body.Clear();

            _body.Add(BuildHeader(cfg));

            _body.Add(new SectionHeader("Leagues", cfg.tables != null ? cfg.tables.Length.ToString() : "0"));
            if (cfg.tables != null)
            {
                foreach (var t in cfg.tables)
                {
                    _body.Add(BuildLeague(t));
                }
            }

            _body.Add(new SectionHeader("Standings"));
            var standingsSlot = AddInto(_body);
            RenderStandings(cfg, standingsSlot);

            _body.Add(new SectionHeader("Your rewards"));
            var rewardsSlot = AddInto(_body);
            ViewBind.Load(
                Sdk.Tournaments.GetRewardsAsync(false),
                rewardsSlot,
                pr => RewardRow(pr.rewards),
                isEmpty: pr => pr == null || pr.rewards == null || pr.rewards.Length == 0,
                emptyView: () => EmptyState.Build("★", "No rewards pending"));
        }

        private VisualElement BuildHeader(TournamentConfigDto cfg)
        {
            var card = new Card(Meta.Accent);
            card.WithTitle(string.IsNullOrEmpty(cfg.name) ? cfg.key : cfg.name, Meta.Accent);

            var chips = new VisualElement();
            chips.AddToClassList("sc-chip-row");
            chips.Add(new Chip(cfg.orderType.ToString(), ChipTone.Info));
            chips.Add(new Chip(cfg.type.ToString(), ChipTone.Neutral));
            chips.Add(new Chip(cfg.updateStrategy.ToString(), ChipTone.Neutral));
            chips.Add(new Chip(cfg.rewardDistributionType.ToString(), ChipTone.Accent));
            if (cfg.isReset)
            {
                chips.Add(new Chip(cfg.resetIntervalType + " x" + cfg.resetIntervalValue, ChipTone.Warn));
                chips.Add(new CountdownChip(cfg.nextResetDate.HasValue ? cfg.nextResetDate.Value.ToUniversalTime() : (DateTime?)null));
            }
            card.Body.Add(chips);

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginTop = 12;
            stats.Add(new StatTile("Leagues", "#").Set(cfg.tables != null ? cfg.tables.Length.ToString() : "0"));
            stats.Add(new StatTile("Last reset", "~").Set(cfg.lastResetDate.HasValue ? cfg.lastResetDate.Value.ToLocalTime().ToString("yyyy-MM-dd") : "—"));
            stats.Add(new StatTile("Updated", "*").Set(cfg.updatedDate.ToLocalTime().ToString("yyyy-MM-dd")));
            card.Body.Add(stats);

            return card;
        }

        private VisualElement BuildLeague(TournamentTableDto t)
        {
            var card = new Card();
            card.style.marginBottom = 10;
            card.WithTitle(string.IsNullOrEmpty(t.name) ? t.id : t.name);

            var thresholds = new VisualElement();
            thresholds.AddToClassList("sc-chip-row");
            thresholds.Add(new Chip("Up ≥ " + t.leagueUpThreshold, ChipTone.Ok));
            thresholds.Add(new Chip("Down ≤ " + t.leagueDownThreshold, ChipTone.Bad));
            card.Body.Add(thresholds);

            if (t.rewardsForPlaces != null && t.rewardsForPlaces.Length > 0)
            {
                var list = new VisualElement();
                list.style.marginTop = 8;
                foreach (var r in t.rewardsForPlaces)
                {
                    var row = new ListRow();
                    row.SetTitle(r.pLaceInLeaderboardMin == r.pLaceInLeaderboardMax
                        ? "#" + r.pLaceInLeaderboardMin
                        : "#" + r.pLaceInLeaderboardMin + "–" + r.pLaceInLeaderboardMax);
                    row.SetTrailing(RewardRow(r.rewards));
                    list.Add(row);
                }
                card.Body.Add(list);
            }

            return card;
        }

        private async void RenderStandings(TournamentConfigDto cfg, VisualElement slot)
        {
            Skeleton.Into(slot);

            string tableId = cfg.tables != null && cfg.tables.Length > 0 ? cfg.tables[0].id : null;
            var metaOp = Sdk.Tournaments.GetPlayerLeagueMetaAsync(cfg.key);
            await metaOp.Task();
            var mr = metaOp.Result;
            if (mr != null && mr.IsSuccess && mr.Data != null && !string.IsNullOrEmpty(mr.Data.currentLeagueTableId))
            {
                tableId = mr.Data.currentLeagueTableId;
            }

            if (string.IsNullOrEmpty(tableId))
            {
                Replace(slot, EmptyState.Build("≡", "No league table to rank"));
                return;
            }

            ViewBind.Load(
                Sdk.Tournaments.GetTopAndAroundAsync(cfg.key, tableId),
                slot,
                BuildStandings,
                isEmpty: d => d == null || d.top == null || d.top.Length == 0,
                emptyView: () => EmptyState.Build("▲", "No entries yet"));
        }

        private VisualElement BuildStandings(TournamentTopAndPlayersAroundDto d)
        {
            string myId = d.playersAround != null && d.playersAround.targetPlayer != null
                ? d.playersAround.targetPlayer.playerId
                : null;

            var cols = new[]
            {
                new DataColumn { Header = "#", FixedWidth = true, Px = 52, Align = "center", Cell = o => RankLabel(((TournamentEntryDto)o).position) },
                new DataColumn { Header = "PLAYER", Grow = 1f, Cell = o => PlayerCell((TournamentEntryDto)o) },
                new DataColumn { Header = "SCORE", FixedWidth = true, Px = 120, Align = "right", Cell = o => ScoreLabel(((TournamentEntryDto)o).value) },
            };
            return new DataTable(cols).Bind(d.top, row => myId != null && ((TournamentEntryDto)row).playerId == myId);
        }

        private static VisualElement RankLabel(int position)
        {
            var l = new Label("#" + position);
            l.AddToClassList("sc-rank");
            if (position == 1) l.style.color = new Color(0.91f, 0.78f, 0.32f);
            else if (position == 2) l.style.color = new Color(0.76f, 0.78f, 0.84f);
            else if (position == 3) l.style.color = new Color(0.82f, 0.54f, 0.32f);
            return l;
        }

        private VisualElement PlayerCell(TournamentEntryDto e)
        {
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

        private static VisualElement ScoreLabel(double value)
        {
            var l = new Label(value.ToString("#,0.##"));
            l.AddToClassList("sc-score");
            return l;
        }

        private VisualElement RewardRow(RewardDataDto[] rewards)
        {
            var row = new VisualElement();
            row.AddToClassList("sc-chip-row");
            if (rewards != null)
            {
                foreach (var rd in rewards)
                {
                    row.Add(new RewardChip(rd.rewardType == RewardType.Currency ? "¤" : "▣", "x" + rd.count, Meta.Accent));
                }
            }
            return row;
        }

        private static VisualElement AddInto(VisualElement parent)
        {
            var slot = new VisualElement();
            slot.style.marginBottom = 14;
            parent.Add(slot);
            return slot;
        }
    }
}
