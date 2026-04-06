using System.Text;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Example.Infrastructure.DI;
using TMPro;
using UnityEngine;

namespace MirraCloud.Example
{
    public class RemoteConfigPanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _remoteConfigText;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public async void LoadRemoteConfig()
        {
            await LoadRemoteConfigAsync();
        }

        private async Task LoadRemoteConfigAsync()
        {
            if (_sdk.IsInitialized == false)
            {
                return;
            }

            await _sdk.RemoteConfig.LoadConfigAsync().Task();

            StringBuilder stringBuilder = new StringBuilder();

            if (_sdk.RemoteConfig.Config != null)
            {
                foreach (var field in _sdk.RemoteConfig.Config.Fields)
                {
                    Debug.Log(field);
                    stringBuilder.AppendLine($"{field.Name}: {field.Value}");
                }
            }

            _remoteConfigText.text = stringBuilder.ToString();
        }
    }
}
