using System;
using MirraCloud.Core;
using MirraCloud.Core.ProfanityFilter.Responses;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Profanity Filter detail: a check tool (text [+ optional group] → CheckAsync) that renders the
    /// clean/profane verdict, the masked output, and a table of matched fragments.
    /// </summary>
    public sealed class ProfanityFilterView : ServiceView
    {
        private static readonly UnityEngine.Color OkColor = new UnityEngine.Color(0.18f, 0.81f, 0.63f);
        private static readonly UnityEngine.Color BadColor = new UnityEngine.Color(0.94f, 0.50f, 0.54f);

        private TextField _text;
        private TextField _group;
        private VisualElement _resultSlot;

        public ProfanityFilterView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");
            _text = new TextField { label = "Text" };
            _text.AddToClassList("sc-field");
            _text.AddToClassList("sc-chat-lookup__input");
            bar.Add(_text);
            _group = new TextField { label = "Group" };
            _group.AddToClassList("sc-field");
            bar.Add(_group);
            var check = new Button(Check) { text = "Check" };
            check.AddToClassList("sc-btn");
            check.AddToClassList("sc-btn--primary");
            bar.Add(check);
            Content.Add(bar);

            var hint = new Label("Empty input is treated as clean. Group is optional (blank = default). Max 2000 characters.");
            hint.AddToClassList("sc-chat-hint");
            Content.Add(hint);

            _resultSlot = AddSlot();
            Replace(_resultSlot, EmptyState.Build("*", "Enter text and press Check"));
        }

        private void Check()
        {
            var text = _text.value ?? string.Empty;
            var group = string.IsNullOrEmpty(_group.value) ? null : _group.value;
            ViewBind.Load(Sdk.ProfanityFilter.CheckAsync(text, group), _resultSlot, BuildResult);
        }

        private VisualElement BuildResult(ProfanityCheckResponse resp)
        {
            int matched = resp.matches != null ? resp.matches.Length : 0;

            var card = new Card(resp.isClean ? OkColor : BadColor);
            card.WithTitle(resp.isClean ? "Clean" : "Profanity found", resp.isClean ? OkColor : BadColor);

            var flags = new VisualElement();
            flags.AddToClassList("sc-chip-row");
            flags.Add(new Chip(resp.isClean ? "Clean" : "Profanity found", resp.isClean ? ChipTone.Ok : ChipTone.Bad));
            flags.Add(new Chip("isProfane: " + (!resp.isClean).ToString().ToLowerInvariant(), resp.isClean ? ChipTone.Neutral : ChipTone.Bad));
            flags.Add(new Chip("matched: " + matched, matched > 0 ? ChipTone.Warn : ChipTone.Neutral));
            card.Body.Add(flags);

            var masked = new ListRow();
            masked.SetTitle("Filtered text");
            masked.SetSubtitle(Fmt.Truncate(resp.maskedText ?? string.Empty, 240));
            card.Body.Add(masked);

            var sh = new SectionHeader("Matches", matched.ToString());
            sh.style.marginTop = 10;
            card.Body.Add(sh);
            if (matched == 0)
            {
                card.Body.Add(EmptyState.Build("OK", "No profanity matched"));
            }
            else
            {
                var cols = new[]
                {
                    new DataColumn { Header = "WORD", Grow = 2f, Cell = o => new Label(((ProfanityMatchDto)o).word) },
                    new DataColumn { Header = "START", FixedWidth = true, Px = 70, Align = "right", Cell = o => new Label(((ProfanityMatchDto)o).start.ToString()) },
                    new DataColumn { Header = "LEN", FixedWidth = true, Px = 64, Align = "right", Cell = o => new Label(((ProfanityMatchDto)o).length.ToString()) },
                };
                card.Body.Add(new DataTable(cols).Bind(resp.matches));
            }

            return card;
        }
    }
}
