using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.AssetsStorage;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Asset Storage detail: one structure load (LoadConfigAsync) rendered as a summary stat row,
    /// a by-type chip breakdown, a folders list, and an asset card grid. Image-type assets show their
    /// actual downloaded picture (LoadTextureFromId); other types show a type glyph.
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

            // assets — card grid with image previews
            root.Add(new SectionHeader("Assets", assets.Count.ToString()));
            if (assets.Count == 0)
            {
                root.Add(EmptyState.Build("▦", "No assets"));
            }
            else
            {
                var grid = new VisualElement();
                grid.AddToClassList("sc-asset-grid");
                foreach (var a in assets)
                {
                    grid.Add(BuildAssetCard(a));
                }
                root.Add(grid);
            }

            return root;
        }

        private VisualElement BuildAssetCard(AssetDto a)
        {
            var card = new VisualElement();
            card.AddToClassList("sc-asset-card");

            var preview = new VisualElement();
            preview.AddToClassList("sc-asset-card__preview");

            var glyph = new Label(GlyphFor(a.type));
            glyph.AddToClassList("sc-asset-card__glyph");
            glyph.style.color = AccentFor(a.type);
            preview.Add(glyph);

            var img = new Image { scaleMode = ScaleMode.ScaleToFit };
            img.AddToClassList("sc-asset-card__img");
            img.style.display = DisplayStyle.None;
            preview.Add(img);

            if (a.type == AssetType.Image && !string.IsNullOrEmpty(a.itemId))
            {
                LoadImage(a.itemId, img, glyph);
            }
            card.Add(preview);

            var body = new VisualElement();
            body.AddToClassList("sc-asset-card__body");
            var name = new Label(string.IsNullOrEmpty(a.name) ? "—" : a.name);
            name.AddToClassList("sc-asset-card__name");
            body.Add(name);

            var chips = new VisualElement();
            chips.AddToClassList("sc-chip-row");
            chips.style.marginTop = 6;
            chips.Add(new Chip(a.type.ToString(), ToneFor(a.type)));
            chips.Add(new Chip(FormatBytes(a.size), ChipTone.Neutral));
            body.Add(chips);

            card.Add(body);
            return card;
        }

        private async void LoadImage(string id, Image img, Label glyph)
        {
            var op = Sdk.AssetsStorage.LoadTextureFromId(id);
            if (op == null)
            {
                return;
            }
            await op.Task();
            var r = op.Result;
            if (r == null || !r.IsSuccess || r.Data == null)
            {
                return; // leave the glyph placeholder
            }
            img.image = r.Data;
            img.style.display = DisplayStyle.Flex;
            glyph.style.display = DisplayStyle.None;
        }

        private static string GlyphFor(AssetType t)
        {
            switch (t)
            {
                case AssetType.Image: return "IMG";
                case AssetType.Audio: return "♪";
                case AssetType.Video: return "▶";
                case AssetType.Document: return "DOC";
                case AssetType.Archive: return "ZIP";
                default: return "?";
            }
        }

        private static Color AccentFor(AssetType t)
        {
            switch (t)
            {
                case AssetType.Image: return new Color(0.35f, 0.71f, 0.94f);
                case AssetType.Audio: return new Color(0.65f, 0.55f, 0.98f);
                case AssetType.Video: return new Color(0.18f, 0.81f, 0.63f);
                case AssetType.Archive: return new Color(0.91f, 0.61f, 0.24f);
                default: return new Color(0.55f, 0.55f, 0.60f);
            }
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
