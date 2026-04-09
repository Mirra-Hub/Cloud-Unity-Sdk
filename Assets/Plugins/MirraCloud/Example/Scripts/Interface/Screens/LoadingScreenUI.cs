using System.Collections;
using MirraCloud.Core;
using MirraCloud.Example.Infrastructure.DI;
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

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }
    }
}
