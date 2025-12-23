using System.Collections;
using MirraCloud.Core;
using MirraCloud.Core.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class ItemResourceUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _title;

        public void Initialize(ItemEconomyDefinition item)
        {
            _title.text = item.Name;

            if (item.Icon.HasIcon)
            {
                StartCoroutine(LoadIcon(item.Icon));
            }
        }

        private IEnumerator LoadIcon(IconDefinition iconDefinition)
        {
            var operation = MirraCloudSDK.AssetsStorage.LoadTextureFromId(iconDefinition.Value);

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
