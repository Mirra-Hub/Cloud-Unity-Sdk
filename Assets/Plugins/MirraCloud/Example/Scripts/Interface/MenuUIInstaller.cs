using MirraCloud.Example;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using Plugins.MirraCloud.Example.Scripts.Interface.Screens;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts
{
    public class MenuUIInstaller : MonoBehaviour
    {
        [SerializeField] private LobbyScreenUI _lobbyScreenUI;
        [SerializeField] private LeaderboardScreenUI _leaderboardScreenUI;
        [SerializeField] private ChangePlayerNamePopupUI _changePlayerNamePopupUI;

        private void Start()
        {
            _lobbyScreenUI.Construct(Container.Instance.PlayerProfile);
            _changePlayerNamePopupUI.Construct(Container.Instance.PlayerProfile);
        }
    }
}