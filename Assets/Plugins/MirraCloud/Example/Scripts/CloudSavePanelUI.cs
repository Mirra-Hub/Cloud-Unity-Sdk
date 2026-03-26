using System.Text;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Core.CloudSave;
using MirraCloud.Example.Infrastructure.DI;
using TMPro;
using UnityEngine;

namespace MirraCloud.Example
{
    public class CloudSavePanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _cloudSaveText;
        [SerializeField] private TMP_InputField _keyFiledInput;
        [SerializeField] private TMP_InputField _valueFiledInput;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public async void SaveData()
        {
            await SaveDataAsync(_keyFiledInput.text, _valueFiledInput.text);
        }

        private async Task SaveDataAsync(string key, string value)
        {
            UpdateDataContainer container = new UpdateDataContainer();
            container.AddString(key, value);

            await _sdk.CloudSave.SaveAsync(container).Task();

            await LoadCloudSaveAsync();
        }

        public async void LoadCloudSave()
        {
            await LoadCloudSaveAsync();
        }

        private async Task LoadCloudSaveAsync()
        {
            await _sdk.CloudSave.LoadAsync().Task();

            StringBuilder stringBuilder = new StringBuilder();

            if (_sdk.CloudSave.PlayerData != null)
            {
                foreach (var field in _sdk.CloudSave.PlayerData.Fields)
                {
                    Debug.Log(field);
                    stringBuilder.AppendLine($"{field.Key}: {field.CurrentValue}");
                }
            }

            _cloudSaveText.text = stringBuilder.ToString();
        }
    }
}
