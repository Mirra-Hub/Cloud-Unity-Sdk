using System.Collections;
using MirraCloud.Core;
using MirraCloud.Core.Economy;
using MirraCloud.Example.Infrastructure.DI;
using MirraCloud.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class ItemResourceUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _title;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public void Initialize(string key, EconomyResourceConfig item)
        {
            _title.text = TryGetFieldString(item, "name", out var name) ? name : key;

            if (TryGetFieldString(item, "icon", out var iconKey) && string.IsNullOrWhiteSpace(iconKey) == false)
            {
                StartCoroutine(LoadIcon(iconKey));
            }
        }

        private static bool TryGetFieldString(EconomyResourceConfig resource, string fieldKey, out string value)
        {
            value = null;
            if (resource?.Fields == null || resource.Fields.Type != JsonValueType.Object)
            {
                return false;
            }

            if (!resource.Fields.TryGetValue(fieldKey, out var v) || v == null || v.Type != JsonValueType.String)
            {
                return false;
            }

            value = (string)v;
            return string.IsNullOrWhiteSpace(value) == false;
        }

        private IEnumerator LoadIcon(string iconKey)
        {
            var operation = _sdk.AssetsStorage.LoadTextureFromId(iconKey);

            yield return operation;

            if (operation.Result.IsSuccess && operation.Result.Data != null)
            {
                var texture = operation.Result.Data;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                _icon.sprite = sprite;
            }
        }
    }
}
