using System;
using MirraCloud.Core;
using MirraCloud.Core.CloudSave.Responses;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Cloud Save detail: the player's key/value records (GetPlayerDataAsync) as a table — key, value
    /// type, stringified JSON value, read/write access masks, last-updated, and version.
    /// </summary>
    public sealed class CloudSaveView : ServiceView
    {
        public CloudSaveView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            Content.Add(new SectionHeader("Player data"));
            var slot = AddSlot();
            ViewBind.Load(
                Sdk.CloudSave.GetPlayerDataAsync(),
                slot,
                BuildTable,
                isEmpty: d => d == null || d.Length == 0,
                emptyView: () => EmptyState.Build("DB", "No cloud-save records yet"));
        }

        private VisualElement BuildTable(DataItemResponse[] rows)
        {
            var cols = new[]
            {
                new DataColumn { Header = "KEY", Grow = 1.1f, Cell = o => new Label(((DataItemResponse)o).key) },
                new DataColumn { Header = "TYPE", FixedWidth = true, Px = 84, Align = "center", Cell = o => new Chip(((DataItemResponse)o).fieldType.ToString(), ChipTone.Info) },
                new DataColumn { Header = "VALUE", Grow = 2.2f, Cell = o => new Label(Fmt.Truncate(Fmt.Json(((DataItemResponse)o).value), 80)) },
                new DataColumn { Header = "ACCESS", FixedWidth = true, Px = 150, Align = "center", Cell = AccessCell },
                new DataColumn { Header = "UPDATED", FixedWidth = true, Px = 120, Align = "right", Cell = o => new Label(Fmt.Truncate(((DataItemResponse)o).updatedAtUtc, 16)) },
                new DataColumn { Header = "VER", FixedWidth = true, Px = 50, Align = "right", Cell = o => new Label(((DataItemResponse)o).version.ToString()) },
            };
            return new DataTable(cols).Bind(rows);
        }

        private static VisualElement AccessCell(object o)
        {
            var d = (DataItemResponse)o;
            var box = new VisualElement();
            box.AddToClassList("sc-chip-row");
            box.Add(new Chip("R:" + d.readMask, ChipTone.Neutral));
            box.Add(new Chip("W:" + d.writeMask, ChipTone.Warn));
            return box;
        }
    }
}
