using System;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.Challenges.Dto;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Challenges detail: one Card per configured challenge (InitializeAsync), each with a live
    /// progress bar + rank/status from the player's entry (GetPlayerAsync), a CountdownChip to reset,
    /// and finisher / participation reward tiers. Read-only — claimed state is not exposed by reads.
    /// </summary>
    public sealed class ChallengesView : ServiceView
    {
        private static readonly Color FinishedColor = new Color(0.18f, 0.81f, 0.63f);

        public ChallengesView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            Content.Add(new SectionHeader("Challenges"));
            var slot = AddSlot();
            ViewBind.Load(
                Sdk.Challenges.InitializeAsync(),
                slot,
                BuildList,
                isEmpty: c => c == null || c.Length == 0,
                emptyView: () => EmptyState.Build("CH", "No challenges configured for this project"));
        }

        private VisualElement BuildList(ChallengeConfigDto[] configs)
        {
            var list = new VisualElement();
            foreach (var cfg in configs)
            {
                list.Add(BuildCard(cfg));
            }
            return list;
        }

        private VisualElement BuildCard(ChallengeConfigDto cfg)
        {
            var card = new Card(Meta.Accent);
            card.style.marginBottom = 14;
            card.WithTitle(string.IsNullOrEmpty(cfg.name) ? cfg.key : cfg.name, Meta.Accent);

            var head = new VisualElement();
            head.AddToClassList("sc-chip-row");
            head.Add(new Chip(cfg.rewardMode.ToString(), ChipTone.Info));
            if (cfg.isReset)
            {
                head.Add(new CountdownChip(cfg.nextResetDate.HasValue ? cfg.nextResetDate.Value.ToUniversalTime() : (DateTime?)null));
            }
            head.Add(new Chip("key: " + cfg.key, ChipTone.Neutral));
            card.Body.Add(head);

            var progressSlot = new VisualElement();
            progressSlot.style.marginTop = 12;
            card.Body.Add(progressSlot);
            ViewBind.Load(Sdk.Challenges.GetPlayerAsync(cfg.key), progressSlot, e => BuildProgress(cfg, e));

            if (cfg.rewardsForFinishers != null && cfg.rewardsForFinishers.Length > 0)
            {
                card.Body.Add(BuildRewards("Finisher rewards", cfg.rewardsForFinishers));
            }
            if (cfg.rewardsForNonFinishers != null && cfg.rewardsForNonFinishers.Length > 0)
            {
                card.Body.Add(BuildRewards("Participation rewards", cfg.rewardsForNonFinishers));
            }

            return card;
        }

        private VisualElement BuildProgress(ChallengeConfigDto cfg, ChallengeEntryDto e)
        {
            double current = e != null ? e.value : 0d;
            bool finished = e != null && e.isFinished;

            var box = new VisualElement();

            var status = new VisualElement();
            status.AddToClassList("sc-chip-row");
            status.Add(new Chip(
                finished ? "Completed" : (e == null ? "Active" : "In progress"),
                finished ? ChipTone.Ok : (e == null ? ChipTone.Accent : ChipTone.Info)));
            box.Add(status);

            var pb = new ProgressBar();
            pb.style.marginTop = 8;
            pb.Set((float)current, (float)cfg.targetValue);
            pb.SetLabel(current.ToString("0") + " / " + cfg.targetValue.ToString("0"));
            pb.SetAccent(finished ? FinishedColor : Meta.Accent);
            box.Add(pb);

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginTop = 10;
            stats.Add(new StatTile("Rank", "▲").Set(e != null ? e.position.ToString() : "—").Highlight(finished));
            stats.Add(new StatTile("Order", "≡").Set(cfg.orderType.ToString()));
            stats.Add(new StatTile("Scoring", "∑").Set(cfg.updateStrategy.ToString()));
            if (e != null && e.finishedAt.HasValue)
            {
                stats.Add(new StatTile("Finished", "✓").Set(e.finishedAt.Value.ToLocalTime().ToString("yyyy-MM-dd")));
            }
            box.Add(stats);

            return box;
        }

        private VisualElement BuildRewards(string title, RewardRangeDto[] ranges)
        {
            var box = new VisualElement();
            box.style.marginTop = 12;
            box.Add(new SectionHeader(title));

            foreach (var r in ranges)
            {
                var row = new ListRow();
                row.SetTitle(r.valueMin == r.valueMax
                    ? "#" + r.valueMin.ToString("0")
                    : "#" + r.valueMin.ToString("0") + "–" + r.valueMax.ToString("0"));
                row.SetTrailing(RewardRow(r.rewards));
                box.Add(row);
            }
            return box;
        }

        private VisualElement RewardRow(RewardDataDto[] rewards)
        {
            var row = new VisualElement();
            row.AddToClassList("sc-chip-row");
            if (rewards != null)
            {
                foreach (var rd in rewards)
                {
                    row.Add(new RewardChip("◆", "x" + rd.count, Meta.Accent));
                }
            }
            return row;
        }
    }
}
