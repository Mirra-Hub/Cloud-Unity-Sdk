using System.Text;
using System.Threading.Tasks;
using MirraCloud.Core;
using TMPro;
using UnityEngine;

namespace MirraCloud.Example
{
    public class RemoteConfigPanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _remoteConfigText;

        public async void LoadRemoteConfig()
        {
            await LoadRemoteConfigAsync();
        }
        
        private async Task LoadRemoteConfigAsync()
        {
            if (MirraCloudSDK.IsInitialized == false)
            {
                return;
            }
            
            await MirraCloudSDK.RemoteConfig.LoadConfigAsync().Task();

            StringBuilder stringBuilder = new StringBuilder();
            
            if (MirraCloudSDK.RemoteConfig.Config != null)
            {
                foreach (var field in MirraCloudSDK.RemoteConfig.Config.Fields)
                {
                    Debug.Log(field);
                    stringBuilder.AppendLine($"{field.Name}: {field.Value}");
                }
            }

            _remoteConfigText.text = stringBuilder.ToString();
        }
    }
}