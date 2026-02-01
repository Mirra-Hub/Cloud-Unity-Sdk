using System.Collections;
using MirraCloud.Core;
using MirraCloud.Example.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class LoadingScreenUI : BaseScreenUI
    {
        [SerializeField] private TextMeshProUGUI _progressLabel;
        [SerializeField] private Slider _progressSlider;

        private IEnumerator LoadingRoutine()
        {
            yield return MirraCloudSDK.Economy.LoadConfigsAsync();
            yield return MirraCloudSDK.CloudSave.LoadAsync();
            
            Container.Instance.PlayerProfile.Initialize();
            
            UIController.ShowScreen<LobbyScreenUI>();
        }
    }
}
