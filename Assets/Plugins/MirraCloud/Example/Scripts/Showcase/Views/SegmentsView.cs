using System;
using MirraCloud.Core;
using MirraCloud.Core.Auth;
using Plugins.MirraCloud.Core.Services.Segments.Dto;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Segments detail: the current player's segment membership (from the account) as a chip strip,
    /// then all configured segments (LoadConfigAsync) as a status table.
    /// </summary>
    public sealed class SegmentsView : ServiceView
    {
        public SegmentsView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            Content.Add(new SectionHeader("My segments"));
            var mineSlot = AddSlot();
            ViewBind.Load(
                Sdk.PlayerAccount.GetAccountAsync(),
                mineSlot,
                a => MyChips(a.SegmentKeys),
                isEmpty: a => a == null || a.SegmentKeys == null || a.SegmentKeys.Length == 0,
                emptyView: () => EmptyState.Build("≡", "You are not in any segment"));

            Content.Add(new SectionHeader("All segments"));
            var allSlot = AddSlot();
            ViewBind.Load(
                Sdk.Segments.LoadConfigAsync(),
                allSlot,
                BuildAll,
                isEmpty: d => d == null || d.Length == 0,
                emptyView: () => EmptyState.Build("≡", "No segments configured for this branch"));
        }

        private VisualElement MyChips(string[] keys)
        {
            var card = new Card();
            var row = new VisualElement();
            row.AddToClassList("sc-chip-row");
            foreach (var k in keys)
            {
                row.Add(new Chip(k, ChipTone.Accent));
            }
            card.Body.Add(row);
            return card;
        }

        private VisualElement BuildAll(SegmentDto[] segs)
        {
            var cols = new[]
            {
                new DataColumn { Header = "NAME", Grow = 1.6f, Cell = NameCell },
                new DataColumn { Header = "STATUS", FixedWidth = true, Px = 100, Align = "center", Cell = o => new Chip(((SegmentDto)o).isEnable ? "Enabled" : "Disabled", ((SegmentDto)o).isEnable ? ChipTone.Ok : ChipTone.Neutral) },
                new DataColumn { Header = "UPDATED", FixedWidth = true, Px = 112, Align = "right", Cell = o => new Label(((SegmentDto)o).updatedDate.ToLocalTime().ToString("yyyy-MM-dd")) },
            };
            return new DataTable(cols).Bind(segs, row => ((SegmentDto)row).isEnable);
        }

        private static VisualElement NameCell(object o)
        {
            var s = (SegmentDto)o;
            var box = new VisualElement();
            var name = new Label(string.IsNullOrEmpty(s.name) ? (string.IsNullOrEmpty(s.id) ? "—" : s.id) : s.name);
            name.style.color = new Color(0.90f, 0.90f, 0.92f);
            box.Add(name);
            if (!string.IsNullOrEmpty(s.description))
            {
                var d = new Label(Fmt.Truncate(s.description, 80));
                d.style.color = new Color(0.48f, 0.48f, 0.51f);
                d.style.fontSize = 11;
                d.style.whiteSpace = WhiteSpace.Normal;
                box.Add(d);
            }
            return box;
        }
    }
}
