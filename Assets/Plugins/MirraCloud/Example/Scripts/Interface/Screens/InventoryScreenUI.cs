using System.Collections.Generic;
using MirraCloud.Example;
using MirraCloud.Example.Infrastructure.DI;
using MirraCloud.Example.Interface;
using Plugins.MirraCloud.Example.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.MirraCloud.Example.Scripts.Interface.Screens
{
    public class InventoryScreenUI : BaseScreenUI
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private Button _closeButton;
        [SerializeField] private InventoryItemSlotUI _itemSlotPrefab;

        private readonly List<InventoryItemSlotUI> _itemSlotUis = new List<InventoryItemSlotUI>();
        private PlayerEconomy _playerEconomy;
        private EconomyService _economyService;

        [InjectDep]
        public void Construct(PlayerEconomy playerEconomy, EconomyService economyService)
        {
            _economyService = economyService;
            _playerEconomy = playerEconomy;
        }

        protected override void OnEnableScreen()
        {
            _closeButton.onClick.AddListener(Close);
            Refresh();
        }

    

        protected override void OnDisableScreen()
        {
            _closeButton.onClick.RemoveListener(Close);
        }
        
        private async void Refresh()
        {
            foreach (var itemSlotUI in _itemSlotUis)
            {
                itemSlotUI.Hide();
            }
            
            for (int index = 0; index < _playerEconomy.PlayerItems.Count; index++)
            {
                var playerItem = _playerEconomy.PlayerItems[index];
                var economyItem = _economyService.GetItem(playerItem.ItemKey);

                if (index >= _itemSlotUis.Count)
                {
                    var itemUI = Instantiate(_itemSlotPrefab, _container);
                    itemUI.Initialize(playerItem, economyItem);
                    _itemSlotUis.Add(itemUI);
                }
                else
                {
                    var itemUI = _itemSlotUis[index];
                    itemUI.Initialize(playerItem, economyItem);
                    itemUI.Show();
                }
            }
        }
        
        private void Close()
        {
            UIController.ShowScreen<LobbyScreenUI>();
        }
    }
}