using Plugins.MirraCloud.Example.Scripts;
using Plugins.MirraCloud.Example.Scripts.Interface.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LobbyScreenUI : BaseScreenUI
    {
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private PlayerInfoWidget _playerInfoWidget;

        public void Construct(PlayerProfile playerProfile)
        {
            _playerInfoWidget.Construct(playerProfile);
        }

        protected override void OnEnableScreen()
        {
            _leaderboardButton.onClick.AddListener(UIController.ShowScreen<LeaderboardScreenUI>);
        }

        protected override void OnDisableScreen()
        {
            _leaderboardButton.onClick.RemoveListener(UIController.ShowScreen<LeaderboardScreenUI>);
        }
    }
}