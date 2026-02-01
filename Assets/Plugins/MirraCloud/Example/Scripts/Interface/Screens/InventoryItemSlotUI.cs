using System.Collections;
using System.Globalization;
using MirraCloud.Example;
using Plugins.MirraCloud.Example.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.MirraCloud.Example.Scripts.Interface.Screens
{
    public class InventoryItemSlotUI : BaseObjectUI
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        
        public void Initialize(PlayerItem item, EconomyItem economyItem)
        {
            _iconImage.sprite = economyItem.Icon;
            _amountText.text = item.Amount.ToString(CultureInfo.InvariantCulture);

            economyItem.OnIconLoaded += () =>
            {
                _iconImage.sprite = economyItem.Icon;
            };
        }
    }
}