using System;
using System.Collections.Generic;
using System.Globalization;
using MirraCloud.Core;
using MirraCloud.Core.Purchases;
using MirraCloud.Core.Purchases.Dto;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Purchases detail: the store catalog (LoadCatalogAsync) as a product table (type/price/currency/
    /// rewards), and the player's order history (GetOrdersAsync) as a status list. Buy flow excluded.
    /// </summary>
    public sealed class PurchasesView : ServiceView
    {
        public PurchasesView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            Content.Add(new SectionHeader("Store catalog"));
            var catalogSlot = AddSlot();
            ViewBind.Load(
                Sdk.Purchases.LoadCatalogAsync(),
                catalogSlot,
                BuildCatalog,
                isEmpty: c => c == null || c.Count == 0,
                emptyView: () => EmptyState.Build("$$", "No products available"));

            Content.Add(new SectionHeader("Purchase history"));
            var ordersSlot = AddSlot();
            ViewBind.Load(
                Sdk.Purchases.GetOrdersAsync(),
                ordersSlot,
                BuildHistory,
                isEmpty: o => o == null || o.Count == 0,
                emptyView: () => EmptyState.Build("$$", "No purchases yet"));
        }

        private VisualElement BuildCatalog(List<CatalogItemDto> items)
        {
            var cols = new[]
            {
                new DataColumn { Header = "PRODUCT", Grow = 2f, Cell = o => Stacked(DisplayOf((CatalogItemDto)o), ((CatalogItemDto)o).Key) },
                new DataColumn { Header = "TYPE", FixedWidth = true, Px = 116, Cell = o => new Chip(((CatalogItemDto)o).Type.ToString(), ((CatalogItemDto)o).Type == PurchaseType.Subscription ? ChipTone.Info : ChipTone.Neutral) },
                new DataColumn { Header = "PRICE", FixedWidth = true, Px = 104, Align = "right", Cell = o => new Label(PriceText((CatalogItemDto)o)) },
                new DataColumn { Header = "CCY", FixedWidth = true, Px = 86, Align = "center", Cell = o => new Chip(CurrencyText((CatalogItemDto)o), ChipTone.Accent) },
                new DataColumn { Header = "REWARDS", Grow = 1.4f, Cell = o => RewardsCell((CatalogItemDto)o) },
            };
            return new DataTable(cols).Bind(items, row => ((CatalogItemDto)row).Type == PurchaseType.Subscription);
        }

        private VisualElement BuildHistory(List<PlayerOrderDto> orders)
        {
            var card = new Card();
            foreach (var o in orders)
            {
                var row = new ListRow();
                row.SetLead(new Chip(o.Amount.ToString(CultureInfo.InvariantCulture) + " " + o.Currency, ChipTone.Neutral));
                row.SetTitle(string.IsNullOrEmpty(o.PurchaseConfigId) ? o.OrderId : o.PurchaseConfigId);
                row.SetSubtitle(o.Provider + " · " + o.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
                row.SetTrailing(new Chip(o.Status.ToString(), StatusTone(o.Status)));
                card.Body.Add(row);
            }
            return card;
        }

        private static string DisplayOf(CatalogItemDto c)
        {
            return string.IsNullOrEmpty(c.DisplayName) ? c.Key : c.DisplayName;
        }

        private static CatalogPriceDto FirstPrice(CatalogItemDto c)
        {
            return c.Prices != null && c.Prices.Count > 0 ? c.Prices[0] : null;
        }

        private static string PriceText(CatalogItemDto c)
        {
            var p = FirstPrice(c);
            return p == null ? "—" : p.Amount.ToString(CultureInfo.InvariantCulture);
        }

        private static string CurrencyText(CatalogItemDto c)
        {
            var p = FirstPrice(c);
            return p == null ? "—" : p.Currency.ToString();
        }

        private VisualElement RewardsCell(CatalogItemDto c)
        {
            var box = new VisualElement();
            box.AddToClassList("sc-chip-row");
            if (c.Rewards != null)
            {
                foreach (var r in c.Rewards)
                {
                    box.Add(new RewardChip(RewardGlyph(r.EconomyResourceKind), "x" + r.Count, Meta.Accent));
                }
            }
            return box;
        }

        private static string RewardGlyph(PurchaseRewardKind kind)
        {
            switch (kind)
            {
                case PurchaseRewardKind.Currency: return "¤";
                case PurchaseRewardKind.Energy: return "⚡";
                default: return "▣";
            }
        }

        private static ChipTone StatusTone(OrderStatus s)
        {
            switch (s)
            {
                case OrderStatus.Paid:
                case OrderStatus.RewardsGranted: return ChipTone.Ok;
                case OrderStatus.Pending: return ChipTone.Warn;
                case OrderStatus.Cancelled:
                case OrderStatus.Refunded:
                case OrderStatus.Failed: return ChipTone.Bad;
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
