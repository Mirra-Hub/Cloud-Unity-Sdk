using System;
using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts;
using UnityEngine;

namespace MirraCloud.Example
{
    public class LobbyScreenUI : BaseScreenUI
    {
        [SerializeField] private PlayerInfoWidget _playerInfoWidget;

        public void Construct(PlayerProfile playerProfile)
        {
            _playerInfoWidget.Construct(playerProfile);
        }
    }
}