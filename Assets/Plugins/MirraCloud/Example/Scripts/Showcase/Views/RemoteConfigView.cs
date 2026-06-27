using System;
using MirraCloud.Core;
using MirraCloud.Core.RemoteConfig;
using MirraCloud.Core.RemoteConfig.Responses;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Remote Config detail: the resolved config groups (LoadConfigAsync) rendered as a summary
    /// stat row plus, per group, a typed key/value table (name + type chip + value).
    /// </summary>
    public sealed class RemoteConfigView : ServiceView
    {
        public RemoteConfigView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var slot = AddSlot();
            ViewBind.Load(
                Sdk.RemoteConfig.LoadConfigAsync(),
                slot,
                Build,
                isEmpty: d => d == null || d.configs == null || d.configs.Length == 0,
                emptyView: () => EmptyState.Build("{ }", "No remote config for this branch"));
        }

        private VisualElement Build(FetchRemoteConfigResponse data)
        {
            var root = new VisualElement();

            int keyCount = 0;
            foreach (var g in data.configs)
            {
                keyCount += g.fields != null ? g.fields.Length : 0;
            }

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.style.marginBottom = 16;
            stats.Add(new StatTile("Groups", "#").Set(data.configs.Length.ToString()));
            stats.Add(new StatTile("Keys", "{ }").Set(keyCount.ToString()));
            root.Add(stats);

            foreach (var group in data.configs)
            {
                var card = new Card();
                card.style.marginBottom = 14;
                card.WithTitle(string.IsNullOrEmpty(group.key) ? "(default)" : group.key);

                if (group.fields == null || group.fields.Length == 0)
                {
                    card.Body.Add(EmptyState.Build("{ }", "No fields"));
                }
                else
                {
                    var cols = new[]
                    {
                        new DataColumn { Header = "NAME", Grow = 1.4f, Cell = o => Stacked(((FetchRemoteConfigResponse.Field)o).name, ((FetchRemoteConfigResponse.Field)o).key) },
                        new DataColumn { Header = "TYPE", FixedWidth = true, Px = 88, Align = "center", Cell = o => new Chip(((FetchRemoteConfigResponse.Field)o).fieldType.ToString(), ToneFor(((FetchRemoteConfigResponse.Field)o).fieldType)) },
                        new DataColumn { Header = "VALUE", Grow = 1.8f, Cell = ValueCell },
                    };
                    card.Body.Add(new DataTable(cols).Bind(group.fields));
                }
                root.Add(card);
            }

            return root;
        }

        private static VisualElement ValueCell(object o)
        {
            var f = (FetchRemoteConfigResponse.Field)o;
            var s = f.value ?? string.Empty;
            var l = new Label(Fmt.Truncate(s, 64));
            l.tooltip = s;
            return l;
        }

        private static ChipTone ToneFor(RemoteConfigFieldType t)
        {
            switch (t)
            {
                case RemoteConfigFieldType.Boolean: return ChipTone.Info;
                case RemoteConfigFieldType.Int:
                case RemoteConfigFieldType.Float: return ChipTone.Accent;
                default: return ChipTone.Neutral;
            }
        }

        private static VisualElement Stacked(string title, string sub)
        {
            var box = new VisualElement();
            var t = new Label(string.IsNullOrEmpty(title) ? "—" : title);
            t.style.color = new UnityEngine.Color(0.90f, 0.90f, 0.92f);
            box.Add(t);
            if (!string.IsNullOrEmpty(sub))
            {
                var s = new Label(sub);
                s.style.color = new UnityEngine.Color(0.48f, 0.48f, 0.51f);
                s.style.fontSize = 11;
                box.Add(s);
            }
            return box;
        }
    }
}
