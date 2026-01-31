using System;
using System.Collections.Generic;
using System.IO;
using MirraCloud.Example.AssetManagement.Assets;
using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using Plugins.MirraCloud.Example.Scripts.Interface.Screens;
using UnityEngine;

namespace MirraCloud.Example.Interface
{
    public class FactoryUI
    {
        private readonly IResolverDI _containerDi;

        private readonly Dictionary<Type, string> _screenPaths = new Dictionary<Type, string>();
        private readonly IAssetService _assetService;

        public const string SCRRENS_FOLDER = "Screens";
        public const string POPUPS_FOLDER = "popups";
        
        public FactoryUI(IResolverDI containerDi, IAssetService assetService)
        {
            _assetService = assetService;
            _containerDi = containerDi;

            RegistryScreen<LobbyScreenUI>("lobby_screen");
            RegistryScreen<InventoryScreenUI>("inventory_screen");
            RegistryScreen<LeaderboardScreenUI>("leaderboard_screen");
            RegistryScreen<LoginScreenUI>("login_screen");
            RegistryScreen<LoadingScreenUI>("loading_screen");
            RegistryPopup<ChangePlayerNamePopupUI>("change_player_name_popup");
            RegistryPopup<NetworkErrorPopupUI>("network_error_popup");
        }

        private void RegistryScreen<T>(string screenPrefabPath) where T : BaseScreenUI
        {
            _screenPaths[typeof(T)] = screenPrefabPath;
        }
        
        private void RegistryPopup<T>(string screenPrefabPath) where T : BasePopupUI
        {
            _screenPaths[typeof(T)] = screenPrefabPath;
        }

        public T CreateScreen<T>(Transform screensRoot) where T : BaseScreenUI
        {
            if (_screenPaths.TryGetValue(typeof(T), out string prefabPath) == false)
            {
                throw new KeyNotFoundException($"Screen prefab path not registered for type: {typeof(T).Name}");
            }

            var prefab = _assetService.LoadPrefab<T>(Path.Combine(SCRRENS_FOLDER, prefabPath));

            return _containerDi.Instantiate(prefab, screensRoot);
        }
        
        public T CreatePopup<T>(Transform popupsRoot) where T : BasePopupUI
        {
            if (_screenPaths.TryGetValue(typeof(T), out string prefabPath) == false)
            {
                throw new KeyNotFoundException($"Screen prefab path not registered for type: {typeof(T).Name}");
            }

            var prefab = _assetService.LoadPrefab<T>(Path.Combine(POPUPS_FOLDER, prefabPath));

            return _containerDi.Instantiate(prefab, popupsRoot);
        }
    }
}
