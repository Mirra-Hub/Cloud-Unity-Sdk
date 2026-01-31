using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example.AssetManagement.Assets
{
    public interface IAssetService
    {
        Task<Image> LoadImage(string path);
        Task<Sprite> LoadSprite(string path);
        T LoadPrefab<T>(string path) where T : MonoBehaviour;
        Task<T> LoadPrefabAsync<T>(string path) where T : MonoBehaviour;
        T Load<T>(string path) where T : Object;
        T[] LoadAll<T>(string path) where T : Object;
    }
}