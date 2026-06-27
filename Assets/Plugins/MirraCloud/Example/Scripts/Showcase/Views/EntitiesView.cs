using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.Entities.Dto;
using MirraCloud.Json;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Entities detail: the entity-config snapshot (GetConfigsAsync) as one card per config, each with
    /// a dynamic key/value field table (JsonValue) and a components list.
    /// </summary>
    public sealed class EntitiesView : ServiceView
    {
        public EntitiesView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var slot = AddSlot();
            ViewBind.Load(
                Sdk.Entities.GetConfigsAsync(),
                slot,
                Build,
                isEmpty: snap => snap == null || snap.Configs == null || snap.Configs.Count == 0,
                emptyView: () => EmptyState.Build("[ ]", "No entity configs on this branch"));
        }

        private VisualElement Build(EntitiesConfigsSnapshotDto snap)
        {
            var root = new VisualElement();
            root.Add(new SectionHeader("Entities", snap.Configs.Count.ToString()));

            foreach (var kv in snap.Configs)
            {
                var key = kv.Key;
                var cfg = kv.Value;

                var card = new Card(Meta.Accent);
                card.style.marginBottom = 14;
                card.WithTitle(string.IsNullOrEmpty(cfg.Name) ? key : cfg.Name, Meta.Accent);

                var chips = new VisualElement();
                chips.AddToClassList("sc-chip-row");
                chips.Add(new Chip(key, ChipTone.Accent));
                if (!string.IsNullOrEmpty(cfg.StableId))
                {
                    chips.Add(new Chip("stable: " + cfg.StableId, ChipTone.Neutral));
                }
                chips.Add(new Chip((cfg.Components != null ? cfg.Components.Length : 0) + " components", ChipTone.Info));
                card.Body.Add(chips);

                card.Body.Add(FieldsBlock(cfg.Fields));

                if (cfg.Components != null && cfg.Components.Length > 0)
                {
                    var sh = new SectionHeader("Components", cfg.Components.Length.ToString());
                    sh.style.marginTop = 12;
                    card.Body.Add(sh);
                    foreach (var comp in cfg.Components)
                    {
                        var row = new ListRow();
                        row.SetTitle(string.IsNullOrEmpty(comp.Key) ? "(component)" : comp.Key);
                        row.SetSubtitle(comp.TypeStableId);
                        row.SetTrailing(new Chip(comp.Data != null ? comp.Data.Type.ToString() : "null", ChipTone.Neutral));
                        card.Body.Add(row);
                    }
                }

                root.Add(card);
            }

            return root;
        }

        private VisualElement FieldsBlock(JsonValue fields)
        {
            var wrap = new VisualElement();
            wrap.style.marginTop = 10;

            if (fields == null || fields.Type != JsonValueType.Object || fields.Count == 0)
            {
                var row = new ListRow();
                row.SetTitle("fields");
                row.SetSubtitle(Fmt.Json(fields));
                wrap.Add(row);
                return wrap;
            }

            var rows = new List<KeyValuePair<string, string>>();
            foreach (var k in fields.Keys)
            {
                rows.Add(new KeyValuePair<string, string>(k, Fmt.Truncate(Fmt.Json(fields[k]), 60)));
            }

            var cols = new[]
            {
                new DataColumn { Header = "FIELD", Grow = 1f, Cell = o => new Label(((KeyValuePair<string, string>)o).Key) },
                new DataColumn { Header = "VALUE", Grow = 2f, Cell = o => new Label(((KeyValuePair<string, string>)o).Value) },
            };
            wrap.Add(new DataTable(cols).Bind(rows));
            return wrap;
        }
    }
}
