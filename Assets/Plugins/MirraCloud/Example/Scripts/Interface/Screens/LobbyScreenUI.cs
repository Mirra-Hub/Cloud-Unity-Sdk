using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Example.Scripts;
using Plugins.MirraCloud.Example.Scripts.Interface.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example.Interface
{
    public class LobbyScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _inventoryButton;
        [SerializeField] private PlayerInfoWidget _playerInfoWidget;

        [InjectDep]
        public void Construct(PlayerProfile playerProfile)
        {
            _playerInfoWidget.Construct(playerProfile);
        }

        protected override void OnEnableScreen()
        {
            _leaderboardButton.onClick.AddListener(ShowLeaderboard);
            _inventoryButton.onClick.AddListener(ShowInventory);
        }

        protected override void OnDisableScreen()
        {
            _leaderboardButton.onClick.RemoveListener(ShowLeaderboard);
            _inventoryButton.onClick.RemoveListener(ShowInventory);
        }

        private void ShowLeaderboard()
        {
            UIController.ShowScreen<LeaderboardScreenUI>();
        }

        private void ShowInventory()
        {
            UIController.ShowScreen<InventoryScreenUI>();
        }
    }
}