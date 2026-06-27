using System;
using MirraCloud.Core;
using MirraCloud.Core.PromoCodes.Dto;
using MirraCloud.Core.PromoCodes.Enums;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Promo Codes detail: a redeem tool (code → RedeemAsync) with a double success gate
    /// (transport + RedemptionStatus), then the player's redemption history.
    /// </summary>
    public sealed class PromoCodesView : ServiceView
    {
        private TextField _code;
        private VisualElement _resultSlot;
        private VisualElement _historySlot;

        public PromoCodesView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");
            _code = new TextField { label = "Code" };
            _code.AddToClassList("sc-field");
            _code.AddToClassList("sc-chat-lookup__input");
            bar.Add(_code);
            var redeem = new Button(Redeem) { text = "Redeem" };
            redeem.AddToClassList("sc-btn");
            redeem.AddToClassList("sc-btn--primary");
            bar.Add(redeem);
            Content.Add(bar);

            var hint = new Label("Enter a campaign promo code to grant its rewards.");
            hint.AddToClassList("sc-chat-hint");
            Content.Add(hint);

            _resultSlot = AddSlot();

            Content.Add(new SectionHeader("My redemptions"));
            _historySlot = AddSlot();
            LoadHistory();
        }

        private void Redeem()
        {
            var code = (_code.value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(code))
            {
                Replace(_resultSlot, EmptyState.Build("%", "Enter a code first"));
                return;
            }
            ViewBind.Load(Sdk.PromoCodes.RedeemAsync(code), _resultSlot, r => BuildRedeemResult(code, r));
        }

        private VisualElement BuildRedeemResult(string code, RedeemPromoCodeResponseDto r)
        {
            if (r == null || r.status != RedemptionStatus.Success)
            {
                var bad = new Card();
                bad.WithTitle("Not redeemed");
                var row = new ListRow();
                row.SetLead(new Chip(StatusLabel(r != null ? r.status : 0), ChipTone.Bad));
                row.SetTitle(StatusLabel(r != null ? r.status : 0));
                row.SetSubtitle("Code: " + Fmt.Truncate(code, 24));
                bad.Body.Add(row);
                return bad;
            }

            // success: refresh history (state changed)
            LoadHistory();

            var card = new Card(new UnityEngine.Color(0.18f, 0.81f, 0.63f));
            card.WithTitle(string.IsNullOrEmpty(r.campaignDisplayName) ? r.campaignKey : r.campaignDisplayName);

            var head = new VisualElement();
            head.AddToClassList("sc-chip-row");
            head.Add(new Chip("Redeemed", ChipTone.Ok));
            card.Body.Add(head);

            var rewardCount = r.rewards != null ? r.rewards.Count : 0;
            var sh = new SectionHeader("Rewards", rewardCount.ToString());
            sh.style.marginTop = 10;
            card.Body.Add(sh);
            if (rewardCount == 0)
            {
                card.Body.Add(EmptyState.Build("—", "No rewards granted"));
            }
            else
            {
                foreach (var gr in r.rewards)
                {
                    var row = new ListRow();
                    row.SetLead(new RewardChip("◆", "x" + gr.count, Meta.Accent));
                    row.SetTitle(gr.rewardId);
                    row.SetSubtitle("kind " + gr.economyResourceKind);
                    card.Body.Add(row);
                }
            }

            var effectCount = r.effects != null ? r.effects.Count : 0;
            if (effectCount > 0)
            {
                var esh = new SectionHeader("Effects", effectCount.ToString());
                esh.style.marginTop = 10;
                card.Body.Add(esh);
                foreach (var ge in r.effects)
                {
                    var row = new ListRow();
                    row.SetTitle(ge.key);
                    row.SetSubtitle(string.IsNullOrEmpty(ge.expiresAt) ? "Permanent" : "Expires " + ge.expiresAt);
                    if (ge.metadata != null && ge.metadata.Count > 0)
                    {
                        row.SetTrailing(new Chip(ge.metadata.Count + " meta", ChipTone.Neutral));
                    }
                    card.Body.Add(row);
                }
            }

            return card;
        }

        private void LoadHistory()
        {
            ViewBind.Load(
                Sdk.PromoCodes.GetHistoryAsync(50),
                _historySlot,
                BuildHistory,
                isEmpty: a => a == null || a.Length == 0,
                emptyView: () => EmptyState.Build("%", "No redemptions yet"));
        }

        private VisualElement BuildHistory(PromoHistoryItemDto[] rows)
        {
            var cols = new[]
            {
                new DataColumn { Header = "CAMPAIGN", Grow = 2f, Cell = o => new Label(Name((PromoHistoryItemDto)o)) },
                new DataColumn { Header = "REDEEMED", Grow = 1.4f, Align = "right", Cell = o => new Label(Fmt.Truncate(((PromoHistoryItemDto)o).redeemedAt, 16)) },
                new DataColumn { Header = "EFFECT ENDS", Grow = 1.4f, Align = "right", Cell = o => new Label(((PromoHistoryItemDto)o).effectExpiresAt == null ? "—" : Fmt.Truncate(((PromoHistoryItemDto)o).effectExpiresAt, 16)) },
            };
            return new DataTable(cols).Bind(rows, row => ((PromoHistoryItemDto)row).effectExpiresAt != null);
        }

        private static string Name(PromoHistoryItemDto h)
        {
            return string.IsNullOrEmpty(h.campaignDisplayName) ? h.campaignKey : h.campaignDisplayName;
        }

        private static string StatusLabel(int status)
        {
            switch (status)
            {
                case RedemptionStatus.Success: return "Success";
                case RedemptionStatus.InvalidCode: return "Invalid code";
                case RedemptionStatus.Expired: return "Expired";
                case RedemptionStatus.NotYetActive: return "Not yet active";
                case RedemptionStatus.Disabled: return "Disabled";
                case RedemptionStatus.LimitExceeded: return "Limit exceeded";
                case RedemptionStatus.RuleFailed: return "Rule failed";
                case RedemptionStatus.AlreadyRedeemed: return "Already redeemed";
                case RedemptionStatus.CodeBlocked: return "Code blocked";
                default: return "Unknown";
            }
        }
    }
}
