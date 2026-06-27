using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.Economy.Dto;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Economy detail: a single inventory load fans out into three visual sections — currency wallet
    /// tiles, energy meters (fill bar + recharge/cooldown chips), and an item grid with quantities.
    /// </summary>
    public sealed class EconomyView : ServiceView
    {
        public EconomyView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            Content.Add(new SectionHeader("Wallet"));
            var walletSlot = AddSlot();
            Content.Add(new SectionHeader("Energies"));
            var energySlot = AddSlot();
            Content.Add(new SectionHeader("Inventory"));
            var itemsSlot = AddSlot();

            Load(walletSlot, energySlot, itemsSlot);
        }

        private async void Load(VisualElement walletSlot, VisualElement energySlot, VisualElement itemsSlot)
        {
            Skeleton.Into(walletSlot);
            Skeleton.Into(energySlot);
            Skeleton.Into(itemsSlot);

            var op = Sdk.Economy.LoadInventoryAsync();
            await op.Task();
            var r = op.Result;

            if (r == null || !r.IsSuccess)
            {
                Replace(walletSlot, ErrorState.Build(r?.Error));
                Replace(energySlot, ErrorState.Build(r?.Error));
                Replace(itemsSlot, ErrorState.Build(r?.Error));
                return;
            }

            var inv = r.Data;
            Replace(walletSlot, BuildWallet(inv != null ? inv.Wallet : null));
            Replace(energySlot, BuildEnergies(inv != null ? inv.Energies : null));
            Replace(itemsSlot, BuildItems(inv != null ? inv.Items : null));
        }

        private VisualElement BuildWallet(List<WalletEntryDto> wallet)
        {
            if (wallet == null || wallet.Count == 0)
            {
                return EmptyState.Build("¤", "No currencies in this wallet");
            }

            var grid = new VisualElement();
            grid.AddToClassList("sc-stat-grid");
            foreach (var w in wallet)
            {
                grid.Add(new StatTile(w.CurrencyId, "¤").Set(w.Balance.ToString("#,0.##")));
            }
            return grid;
        }

        private VisualElement BuildEnergies(List<EnergyBalanceDto> energies)
        {
            if (energies == null || energies.Count == 0)
            {
                return EmptyState.Build("⚡", "No energy meters configured");
            }

            var list = new VisualElement();
            foreach (var e in energies)
            {
                var card = new Card();
                card.style.marginBottom = 10;

                var head = new VisualElement();
                head.AddToClassList("sc-energy__head");
                var nm = new Label(e.EnergyId);
                nm.AddToClassList("sc-energy__name");
                var val = new Label(e.CurrentValue + " / " + e.MaxValue);
                val.AddToClassList("sc-energy__val");
                head.Add(nm);
                head.Add(val);
                card.Body.Add(head);

                var bar = new ProgressBar();
                bar.Set(e.CurrentValue, e.MaxValue);
                bar.SetAccent(Meta.Accent);
                card.Body.Add(bar);

                var chips = new VisualElement();
                chips.AddToClassList("sc-chip-row");
                if (e.IsUnlimited)
                {
                    chips.Add(new Chip("Unlimited", ChipTone.Ok));
                }
                if (e.IsOnCooldown && e.CooldownRemainingSeconds.HasValue)
                {
                    chips.Add(new Chip("Cooldown " + Dur(e.CooldownRemainingSeconds.Value), ChipTone.Warn));
                }
                if (!e.IsUnlimited && e.SecondsUntilNextRecharge.HasValue)
                {
                    chips.Add(new Chip("+1 in " + Dur(e.SecondsUntilNextRecharge.Value), ChipTone.Info));
                }
                if (!e.IsUnlimited && e.SecondsUntilFullRecharge.HasValue)
                {
                    chips.Add(new Chip("Full in " + Dur(e.SecondsUntilFullRecharge.Value), ChipTone.Neutral));
                }
                if (chips.childCount > 0)
                {
                    chips.style.marginTop = 10;
                    card.Body.Add(chips);
                }

                list.Add(card);
            }
            return list;
        }

        private VisualElement BuildItems(List<ItemSlotDto> items)
        {
            if (items == null || items.Count == 0)
            {
                return EmptyState.Build("▣", "Inventory is empty");
            }

            var grid = new VisualElement();
            grid.AddToClassList("sc-item-grid");
            foreach (var it in items)
            {
                var cell = new VisualElement();
                cell.AddToClassList("sc-item");

                var icon = new VisualElement();
                icon.AddToClassList("sc-item__icon");
                var glyph = new Label("▣");
                glyph.AddToClassList("sc-item__glyph");
                glyph.style.color = Meta.Accent;
                icon.Add(glyph);
                var qty = new Label("×" + it.Quantity);
                qty.AddToClassList("sc-item__qty");
                icon.Add(qty);
                cell.Add(icon);

                var name = new Label(it.ItemId);
                name.AddToClassList("sc-item__name");
                cell.Add(name);

                if (!string.IsNullOrEmpty(it.InventoryKey))
                {
                    var sub = new Label(it.InventoryKey);
                    sub.AddToClassList("sc-item__sub");
                    cell.Add(sub);
                }

                grid.Add(cell);
            }
            return grid;
        }

        private static string Dur(int seconds)
        {
            var s = TimeSpan.FromSeconds(seconds);
            if (s.TotalDays >= 1)
            {
                return (int)s.TotalDays + "d " + s.Hours + "h";
            }
            if (s.TotalHours >= 1)
            {
                return s.Hours + "h " + s.Minutes + "m";
            }
            if (s.TotalMinutes >= 1)
            {
                return s.Minutes + "m " + s.Seconds + "s";
            }
            return s.Seconds + "s";
        }
    }
}
