using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.CloudCode.Responses;
using MirraCloud.Json;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Cloud Code detail: a run tool (function key → ExecuteAsync) that renders the dynamic JsonValue
    /// response — its type, a stringified preview, and (for objects) a key/value field table.
    /// </summary>
    public sealed class CloudCodeView : ServiceView
    {
        private TextField _key;
        private VisualElement _resultSlot;

        public CloudCodeView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");
            _key = new TextField { label = "Function key" };
            _key.AddToClassList("sc-field");
            _key.AddToClassList("sc-chat-lookup__input");
            bar.Add(_key);
            var run = new Button(Run) { text = "Run" };
            run.AddToClassList("sc-btn");
            run.AddToClassList("sc-btn--primary");
            bar.Add(run);
            Content.Add(bar);

            var hint = new Label("Invokes a Cloud Code server function by key (no parameters) and shows its result.");
            hint.AddToClassList("sc-chat-hint");
            Content.Add(hint);

            _resultSlot = AddSlot();
            Replace(_resultSlot, EmptyState.Build("</>", "Enter a function key and press Run"));
        }

        private void Run()
        {
            var key = (_key.value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key))
            {
                Replace(_resultSlot, EmptyState.Build("</>", "Enter a function key"));
                return;
            }
            ViewBind.Load(Sdk.CloudCode.ExecuteAsync(key), _resultSlot, RenderResult);
        }

        private VisualElement RenderResult(ExecuteCloudCodeResponseDto dto)
        {
            var card = new Card(Meta.Accent);
            card.WithTitle("Response", Meta.Accent);

            var result = dto != null ? dto.Result : null;
            if (result == null || result.Type == JsonValueType.Null)
            {
                card.Body.Add(EmptyState.Build("{ }", "Function returned no value"));
                return card;
            }

            var head = new VisualElement();
            head.AddToClassList("sc-chip-row");
            head.Add(new Chip(result.Type.ToString(), ChipTone.Info));
            card.Body.Add(head);

            var preview = new ListRow();
            preview.SetTitle("result");
            preview.SetSubtitle(Fmt.Json(result));
            card.Body.Add(preview);

            if (result.Type == JsonValueType.Object && result.Count > 0)
            {
                var sh = new SectionHeader("Fields", result.Count.ToString());
                sh.style.marginTop = 10;
                card.Body.Add(sh);

                var rows = new List<KeyValuePair<string, string>>();
                foreach (var k in result.Keys)
                {
                    rows.Add(new KeyValuePair<string, string>(k, Fmt.Truncate(Fmt.Json(result[k]), 60)));
                }
                var cols = new[]
                {
                    new DataColumn { Header = "KEY", Grow = 1f, Cell = o => new Label(((KeyValuePair<string, string>)o).Key) },
                    new DataColumn { Header = "VALUE", Grow = 2f, Cell = o => new Label(((KeyValuePair<string, string>)o).Value) },
                };
                card.Body.Add(new DataTable(cols).Bind(rows));
            }

            return card;
        }
    }
}
