using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.Enums;
using MirraCloud.Core.Localization.Dto;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Localization detail: lookup-driven (no list-collections API) — enter a collection ID to load
    /// all keys, then a language selector (All / per-locale) over a key → translation table.
    /// </summary>
    public sealed class LocalizationView : ServiceView
    {
        private TextField _input;
        private VisualElement _result;

        private List<LocalizationResponseDto> _rows;
        private readonly List<LanguageCode> _langs = new List<LanguageCode>();
        private LanguageCode? _selectedLang;
        private VisualElement _selectorSlot;
        private VisualElement _tableSlot;

        public LocalizationView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");
            _input = new TextField { label = "Collection ID" };
            _input.AddToClassList("sc-field");
            _input.AddToClassList("sc-chat-lookup__input");
            bar.Add(_input);
            var loadBtn = new Button(() => Load(_input.value)) { text = "Load" };
            loadBtn.AddToClassList("sc-btn");
            loadBtn.AddToClassList("sc-btn--primary");
            bar.Add(loadBtn);
            Content.Add(bar);

            var hint = new Label("Enter a localization collection ID to inspect its keys and translations.");
            hint.AddToClassList("sc-chat-hint");
            Content.Add(hint);

            _result = AddSlot();
            Replace(_result, EmptyState.Build("Aa", "No collection loaded"));
        }

        private void Load(string collectionId)
        {
            if (string.IsNullOrWhiteSpace(collectionId))
            {
                Replace(_result, EmptyState.Build("Aa", "Enter a collection ID"));
                return;
            }

            ViewBind.Load(
                Sdk.Localization.GetAllLocalizationsAsync(collectionId.Trim()),
                _result,
                BuildSelectorAndTable,
                isEmpty: d => d == null || d.Count == 0,
                emptyView: () => EmptyState.Build("Aa", "No strings in this collection"));
        }

        private VisualElement BuildSelectorAndTable(List<LocalizationResponseDto> rows)
        {
            _rows = rows;
            _selectedLang = null;

            _langs.Clear();
            var seen = new HashSet<LanguageCode>();
            foreach (var r in rows)
            {
                if (r.Values == null)
                {
                    continue;
                }
                foreach (var v in r.Values)
                {
                    if (seen.Add(v.LanguageCode))
                    {
                        _langs.Add(v.LanguageCode);
                    }
                }
            }

            var box = new VisualElement();
            box.Add(new SectionHeader("Strings", rows.Count.ToString()));

            _selectorSlot = new VisualElement();
            box.Add(_selectorSlot);

            _tableSlot = new VisualElement();
            _tableSlot.style.marginTop = 10;
            box.Add(_tableSlot);

            RenderSelector();
            RenderTable();
            return box;
        }

        private void RenderSelector()
        {
            _selectorSlot.Clear();
            var row = new VisualElement();
            row.AddToClassList("sc-chip-row");

            row.Add(LangChip("All", _selectedLang == null, () => { _selectedLang = null; RenderSelector(); RenderTable(); }));
            foreach (var lang in _langs)
            {
                var captured = lang;
                bool active = _selectedLang.HasValue && _selectedLang.Value == lang;
                row.Add(LangChip(lang.ToString(), active, () => { _selectedLang = captured; RenderSelector(); RenderTable(); }));
            }
            _selectorSlot.Add(row);
        }

        private static Chip LangChip(string text, bool active, Action onClick)
        {
            var chip = new Chip(text, active ? ChipTone.Accent : ChipTone.Neutral);
            chip.RegisterCallback<ClickEvent>(_ => onClick());
            return chip;
        }

        private void RenderTable()
        {
            _tableSlot.Clear();

            var cols = new List<DataColumn>();
            cols.Add(new DataColumn { Header = "KEY", Grow = 1.2f, Cell = o => new Label(((LocalizationResponseDto)o).KeyName) });

            if (_selectedLang == null)
            {
                foreach (var lang in _langs)
                {
                    var captured = lang;
                    cols.Add(new DataColumn
                    {
                        Header = captured.ToString(),
                        Grow = 1f,
                        Cell = o => new Label(Fmt.Truncate(ValueFor((LocalizationResponseDto)o, captured) ?? "—", 40)),
                    });
                }
            }
            else
            {
                var lang = _selectedLang.Value;
                cols.Add(new DataColumn
                {
                    Header = lang.ToString(),
                    Grow = 1.8f,
                    Cell = o => new Label(Fmt.Truncate(ValueFor((LocalizationResponseDto)o, lang) ?? "—", 80)),
                });
            }

            _tableSlot.Add(new DataTable(cols.ToArray()).Bind(_rows));
        }

        private static string ValueFor(LocalizationResponseDto row, LanguageCode lang)
        {
            if (row.Values == null)
            {
                return null;
            }
            foreach (var v in row.Values)
            {
                if (v.LanguageCode == lang)
                {
                    return v.Value;
                }
            }
            return null;
        }
    }
}
