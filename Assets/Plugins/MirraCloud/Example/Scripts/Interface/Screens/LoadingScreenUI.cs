using System.Collections;
using MirraCloud.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoadingScreenUI : BaseScreenUI
    {
        [SerializeField] private TextMeshProUGUI _progressLabel;
        [SerializeField] private Slider _progressSlider;

        protected override void OnEnableScreen()
        {
            StartCoroutine(LoadingRoutine());
        }

        private IEnumerator LoadingRoutine()
        {
            yield return MirraCloudSDK.Economy.LoadConfigAsync();
            yield return MirraCloudSDK.CloudSave.LoadAsync();
            
            Container.Instance.PlayerProfile.Initialize();
            
            UIController.ShowScreen<LobbyScreenUI>();
        }
    }
}
