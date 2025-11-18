using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Example;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.MirraCloud.Example.Scripts.Interface.Screens
{
    public class LeaderboardScreenUI : BaseScreenUI
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private Button _closeButton;
        [SerializeField] private LeaderboardItemUI _playerLeaderboardItemUI;
        [SerializeField] private LeaderboardItemUI _leaderboardItemUIPrefab;
        [SerializeField] private string _leaderboardId;

        private readonly List<LeaderboardItemUI> _leaderboardItemsUI = new List<LeaderboardItemUI>();
        
        protected override void OnEnableScreen()
        {
            _closeButton.onClick.AddListener(UIController.ShowScreen<LobbyScreenUI>);
            Refresh();
        }

        protected override void OnDisableScreen()
        {
            _closeButton.onClick.RemoveListener(UIController.ShowScreen<LobbyScreenUI>);
        }
        
        private async void Refresh()
        {
            var operation = MirraCloudSDK.Leaderboard.GetLeaderboardPlayer(_leaderboardId);
            await operation.Task;

            if (operation.IsSuccess)
            {
                _playerLeaderboardItemUI.Initialize(operation.Value);
            }

            foreach (var leaderboardItem in _leaderboardItemsUI)
            {
                leaderboardItem.Hide();
            }
            
            var operationTable = MirraCloudSDK.Leaderboard.GetLeaderboardTopEntries(_leaderboardId);
            await operationTable.Task;

            if (operationTable.IsSuccess)
            {
                Debug.Log(operationTable.DownloadHandler.text);
                
                for (int index = 0; index < operationTable.Value.entries.Length; index++)
                {
                    var leaderboardEntry = operationTable.Value.entries[index];

                    if (index >= _leaderboardItemsUI.Count)
                    {
                        var itemUI = Instantiate(_leaderboardItemUIPrefab, _container);
                        itemUI.Initialize(leaderboardEntry);
                        _leaderboardItemsUI.Add(itemUI);
                    }
                    else
                    {
                        var itemUI = _leaderboardItemsUI[index];
                        itemUI.Initialize(leaderboardEntry);
                        itemUI.Show();
                    }
                }
            }
        }
    }
}
