using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Example;
using MirraCloud.Example.Interface;
using Plugins.MirraCloud.Example.Scripts.Test;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.MirraCloud.Example.Scripts.Interface.Screens
{
    public class InventoryScreenUI : BaseScreenUI
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private Button _closeButton;
        [SerializeField] private LeaderboardItemUI _playerLeaderboardItemUI;
        [SerializeField] private LeaderboardItemUI _leaderboardItemUIPrefab;
        [SerializeField] private string _leaderboardId;

        private readonly List<LeaderboardItemUI> _leaderboardItemsUI = new List<LeaderboardItemUI>();

        private void Awake()
        {
            var  leaderboardTest = FindObjectOfType<LeaderboardTest>();
            _leaderboardId = leaderboardTest.LeaderboardId;
        }

        protected override void OnEnableScreen()
        {
            _closeButton.onClick.AddListener(UIController.ShowScreen<LobbyScreenUI>);
          //  Refresh();
        }

        protected override void OnDisableScreen()
        {
            _closeButton.onClick.RemoveListener(UIController.ShowScreen<LobbyScreenUI>);
        }
        
        private async void Refresh()
        {
            var operation = MirraCloudSDK.Leaderboard.GetLeaderboardPlayer(_leaderboardId);
            await operation.Task();

            if (operation.Result.IsSuccess && operation.Result.Data != null)
            {
                _playerLeaderboardItemUI.Initialize(operation.Result.Data);
            }

            foreach (var leaderboardItem in _leaderboardItemsUI)
            {
                leaderboardItem.Hide();
            }
            
            var operationTable = MirraCloudSDK.Leaderboard.GetLeaderboardTopEntries(_leaderboardId);
            await operationTable.Task();

            if (operationTable.Result.IsSuccess && operationTable.Result.Data != null)
            {
                Debug.Log(operationTable.Result.ResponseBody);
                
                for (int index = 0; index < operationTable.Result.Data.entries.Length; index++)
                {
                    var leaderboardEntry = operationTable.Result.Data.entries[index];

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