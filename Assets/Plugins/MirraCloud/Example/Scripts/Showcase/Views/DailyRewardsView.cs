using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.DailyRewards.Dto;
using MirraCloud.Core.DailyRewards.Enums;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Daily Rewards detail: per-player live status (GetStatusAsync) rendered as a streak/progress
    /// header, a day-by-day reward track (current day highlighted, claim status color-coded), and
    /// streak-bonus + milestone sections. Read-only — never claims.
    /// </summary>
    public sealed class DailyRewardsView : ServiceView
    {
        private static readonly Color BonusColor = new Color(0.91f, 0.61f, 0.24f);

        private readonly List<Button> _tabs = new List<Button>();
        private VisualElement _body;

        public DailyRewardsView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var pickerSlot = AddSlot(12f);
            _body = AddSlot();

            ViewBind.Load(
                Sdk.DailyRewards.GetStatusAsync(),
                pickerSlot,
                BuildPicker,
                isEmpty: a => a == null || a.Length == 0,
                emptyView: () => EmptyState.Build("DR", "No daily reward tracks for this player"));
        }

        private VisualElement BuildPicker(DailyRewardStatusDto[] statuses)
        {
            _tabs.Clear();
            var tabs = new VisualElement();
            tabs.AddToClassList("sc-tabs");
            if (statuses.Length > 1)
            {
                foreach (var s in statuses)
                {
                    var captured = s;
                    var btn = new Button { text = string.IsNullOrEmpty(s.calendarName) ? s.calendarKey : s.calendarName };
                    btn.AddToClassList("sc-tab");
                    btn.clicked += () => Select(captured, btn);
                    _tabs.Add(btn);
                    tabs.Add(btn);
                }
            }
            Select(statuses[0], _tabs.Count > 0 ? _tabs[0] : null);
            return tabs;
        }

        private void Select(DailyRewardStatusDto s, Button active)
        {
            foreach (var b in _tabs)
            {
                b.EnableInClassList("sc-tab--active", b == active);
            }
            _body.Clear();
            _body.Add(RenderStatus(s));
        }

        private VisualElement RenderStatus(DailyRewardStatusDto s)
        {
            var card = new Card(Meta.Accent);
            card.WithTitle(string.IsNullOrEmpty(s.calendarName) ? s.calendarKey : s.calendarName, Meta.Accent);

            var chips = new VisualElement();
            chips.AddToClassList("sc-chip-row");
            chips.Add(new Chip("key: " + s.calendarKey, ChipTone.Neutral));
            chips.Add(new Chip("Cycle " + s.currentCycle, ChipTone.Info));
            chips.Add(new Chip(
                s.isCompleted ? "Completed" : (s.canClaimToday ? "Claim ready" : "Claimed today"),
                s.isCompleted ? ChipTone.Ok : (s.canClaimToday ? ChipTone.Accent : ChipTone.Neutral)));
            if (s.canClaimToday)
            {
                chips.Add(new Chip("Available now", ChipTone.Ok));
            }
            else
            {
                chips.Add(new CountdownChip(s.nextResetTime.ToUniversalTime()));
            }
            card.Body.Add(chips);

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginTop = 12;
            stats.Add(new StatTile("Streak", "▲").Set(s.totalClaimDays.ToString()).Highlight(true));
            stats.Add(new StatTile("Current day", "#").Set("Day " + s.currentDayNumber).Highlight(s.canClaimToday));
            stats.Add(new StatTile("Cycle length", "≡").Set(s.cycleLengthDays.ToString()));
            card.Body.Add(stats);

            var pb = new ProgressBar();
            pb.style.marginTop = 10;
            pb.Set(s.currentDayNumber, s.cycleLengthDays);
            pb.SetLabel("Day " + s.currentDayNumber + " / " + s.cycleLengthDays);
            pb.SetAccent(Meta.Accent);
            card.Body.Add(pb);

            // day track
            if (s.days != null && s.days.Length > 0)
            {
                card.Body.Add(SubHeader("Reward track", s.days.Length));
                var track = new VisualElement();
                track.AddToClassList("sc-day-track");
                foreach (var d in s.days)
                {
                    track.Add(DayTile(d, d.dayNumber == s.currentDayNumber));
                }
                card.Body.Add(track);
            }

            // streak bonuses
            if (s.streakBonuses != null && s.streakBonuses.Length > 0)
            {
                card.Body.Add(SubHeader("Streak bonuses", s.streakBonuses.Length));
                foreach (var b in s.streakBonuses)
                {
                    var row = new ListRow();
                    row.SetTitle(b.streakDays + "-day streak");
                    row.SetSubtitle(b.bonusType.ToString() + (b.bonusType == StreakBonusType.Multiplier ? " x" + b.multiplier : ""));
                    row.SetTrailing(RewardRow(b.rewards, Meta.Accent));
                    card.Body.Add(row);
                }
            }

            // milestones
            if (s.milestoneProgress != null && s.milestoneProgress.Length > 0)
            {
                card.Body.Add(SubHeader("Milestones", s.milestoneProgress.Length));
                foreach (var m in s.milestoneProgress)
                {
                    var row = new ListRow();
                    row.SetLead(new Chip(m.isReached ? "Reached" : "Locked", m.isReached ? ChipTone.Ok : ChipTone.Neutral));
                    row.SetTitle(m.totalDaysRequired + " days");
                    row.SetTrailing(RewardRow(m.rewards, Meta.Accent));
                    card.Body.Add(row);
                }
            }

            return card;
        }

        private VisualElement DayTile(DayStatusDto d, bool current)
        {
            var tile = new VisualElement();
            tile.AddToClassList("sc-day-tile");
            if (current)
            {
                tile.AddToClassList("sc-day-tile--current");
            }

            tile.Add(new Chip("Day " + d.dayNumber + (d.isSpecialDay ? " ★" : ""), DayTone(d.status)));

            var rewards = new VisualElement();
            rewards.AddToClassList("sc-chip-row");
            rewards.style.marginTop = 6;
            if (d.rewards != null)
            {
                foreach (var rd in d.rewards)
                {
                    rewards.Add(new RewardChip("◆", "x" + rd.count, Meta.Accent));
                }
            }
            if (d.bonusRewards != null)
            {
                foreach (var rd in d.bonusRewards)
                {
                    rewards.Add(new RewardChip("◆", "x" + rd.count, BonusColor));
                }
            }
            tile.Add(rewards);

            return tile;
        }

        private static VisualElement RewardRow(RewardDataDto[] rewards, Color accent)
        {
            var row = new VisualElement();
            row.AddToClassList("sc-chip-row");
            if (rewards != null)
            {
                foreach (var rd in rewards)
                {
                    row.Add(new RewardChip("◆", "x" + rd.count, accent));
                }
            }
            return row;
        }

        private static VisualElement SubHeader(string title, int count)
        {
            var h = new SectionHeader(title, count.ToString());
            h.style.marginTop = 14;
            return h;
        }

        private static ChipTone DayTone(DailyRewardClaimStatus s)
        {
            switch (s)
            {
                case DailyRewardClaimStatus.Claimed: return ChipTone.Ok;
                case DailyRewardClaimStatus.Available:
                case DailyRewardClaimStatus.CatchUpAvailable: return ChipTone.Accent;
                case DailyRewardClaimStatus.Missed: return ChipTone.Bad;
                default: return ChipTone.Neutral;
            }
        }
    }
}
