using System.Text;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.CloudSave.Requests;
using TMPro;
using UnityEngine;

namespace MirraCloud.Example
{
    public class CloudSavePanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _cloudSaveText;
        [SerializeField] private TMP_InputField _keyFiledInput;
        [SerializeField] private TMP_InputField _valueFiledInput;

        public async void SaveData()
        {
            await SaveDataAsync(_keyFiledInput.text, _valueFiledInput.text);
        }

        private async Task SaveDataAsync(string key, string value)
        {
            var request = new CloudSaveDataRequest();
            request.AddString(key, value);

            await MirraCloudSDK.CloudSave.SaveAsync(request).Task();

            await LoadCloudSaveAsync();
        }

        public async void LoadCloudSave()
        {
            await LoadCloudSaveAsync();
        }

        private async Task LoadCloudSaveAsync()
        {
            await MirraCloudSDK.CloudSave.LoadAsync().Task();

            StringBuilder stringBuilder = new StringBuilder();

            if (MirraCloudSDK.CloudSave.PlayerData != null)
            {
                foreach (var field in MirraCloudSDK.CloudSave.PlayerData.Fields)
                {
                    Debug.Log(field);
                    stringBuilder.AppendLine($"{field.Key}: {field.CurrentValue}");
                }
            }

            _cloudSaveText.text = stringBuilder.ToString();
        }
    }
}