using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.AssetsStorage;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Asset Storage detail: one structure load (LoadConfigAsync) rendered as a summary stat row,
    /// a by-type chip breakdown, a folders list, and an assets table. No preview URLs exist on the
    /// DTOs, so each asset's lead is a type-colored initials badge rather than a thumbnail.
    /// </summary>
    public sealed class AssetsStorageView : ServiceView
    {
        public AssetsStorageView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var slot = AddSlot();
            ViewBind.Load(
                Sdk.AssetsStorage.LoadConfigAsync(),
                slot,
                Build,
                isEmpty: d => d == null
                    || ((d.assets == null || d.assets.Count == 0) && (d.folders == null || d.folders.Count == 0)),
                emptyView: () => EmptyState.Build("FS", "No assets or folders in this branch"));
        }

        private VisualElement Build(AssetStorageStructureDto data)
        {
            var root = new VisualElement();
            var assets = data.assets ?? new List<AssetDto>();
            var folders = data.folders ?? new List<FolderDto>();

            // summary
            long totalSize = 0;
            var types = new HashSet<AssetType>();
            foreach (var a in assets)
            {
                totalSize += a.size;
                types.Add(a.type);
            }

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginBottom = 16;
            stats.Add(new StatTile("Assets", "▦").Set(assets.Count.ToString()));
            stats.Add(new StatTile("Folders", "▱").Set(folders.Count.ToString()));
            stats.Add(new StatTile("Total size", "≡").Set(FormatBytes(totalSize)));
            stats.Add(new StatTile("Types", "#").Set(types.Count.ToString()));
            root.Add(stats);

            // by type
            if (assets.Count > 0)
            {
                var counts = new Dictionary<AssetType, int>();
                foreach (var a in assets)
                {
                    counts.TryGetValue(a.type, out var c);
                    counts[a.type] = c + 1;
                }
                root.Add(new SectionHeader("By type"));
                var chips = new VisualElement();
                chips.AddToClassList("sc-chip-row");
                chips.style.marginBottom = 16;
                foreach (var kv in counts)
                {
                    chips.Add(new Chip(kv.Key + ": " + kv.Value, ToneFor(kv.Key)));
                }
                root.Add(chips);
            }

            // folders
            root.Add(new SectionHeader("Folders", folders.Count.ToString()));
            if (folders.Count == 0)
            {
                var e = EmptyState.Build("▱", "No folders");
                e.style.marginBottom = 16;
                root.Add(e);
            }
            else
            {
                var list = new VisualElement();
                list.style.marginBottom = 16;
                foreach (var f in folders)
                {
                    int items = 0;
                    foreach (var a in assets)
                    {
                        if (a.folderId == f.itemId)
                        {
                            items++;
                        }
                    }
                    var row = new ListRow();
                    row.SetLead(new Avatar(36f).SetInitialsFor(f.name));
                    row.SetTitle(string.IsNullOrEmpty(f.name) ? "—" : f.name);
                    row.SetSubtitle(f.path);
                    row.SetTrailing(new Chip(items + " items", ChipTone.Neutral));
                    list.Add(row);
                }
                root.Add(list);
            }

            // assets table
            root.Add(new SectionHeader("Assets", assets.Count.ToString()));
            if (assets.Count == 0)
            {
                root.Add(EmptyState.Build("▦", "No assets"));
            }
            else
            {
                var cols = new[]
                {
                    new DataColumn { Header = "", FixedWidth = true, Px = 44, Align = "center", Cell = o => new Avatar(30f).SetInitialsFor(((AssetDto)o).type.ToString()) },
                    new DataColumn { Header = "NAME", Grow = 2f, Cell = o => Txt(((AssetDto)o).name) },
                    new DataColumn { Header = "TYPE", Grow = 1f, Cell = o => new Chip(((AssetDto)o).type.ToString(), ToneFor(((AssetDto)o).type)) },
                    new DataColumn { Header = "EXT", FixedWidth = true, Px = 64, Align = "center", Cell = o => Txt(((AssetDto)o).extension) },
                    new DataColumn { Header = "SIZE", FixedWidth = true, Px = 92, Align = "right", Cell = o => Txt(FormatBytes(((AssetDto)o).size)) },
                    new DataColumn { Header = "VER", FixedWidth = true, Px = 50, Align = "center", Cell = o => Txt("v" + ((AssetDto)o).version) },
                    new DataColumn { Header = "UPDATED", FixedWidth = true, Px = 112, Align = "right", Cell = o => Txt(((AssetDto)o).updatedAt.ToLocalTime().ToString("yyyy-MM-dd")) },
                };
                root.Add(new DataTable(cols).Bind(assets));
            }

            return root;
        }

        private static Label Txt(string s)
        {
            return new Label(string.IsNullOrEmpty(s) ? "—" : s);
        }

        private static ChipTone ToneFor(AssetType t)
        {
            switch (t)
            {
                case AssetType.Image: return ChipTone.Info;
                case AssetType.Audio: return ChipTone.Accent;
                case AssetType.Video: return ChipTone.Ok;
                case AssetType.Archive: return ChipTone.Warn;
                default: return ChipTone.Neutral;
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0)
            {
                return "0 B";
            }
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double v = bytes;
            int i = 0;
            while (v >= 1024d && i < units.Length - 1)
            {
                v /= 1024d;
                i++;
            }
            return (i == 0 ? v.ToString("0") : v.ToString("0.#")) + " " + units[i];
        }
    }
}
