using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example.AssetManagement.Assets
{
    public class AssetService : IAssetService
    {
        public async Task<Image> LoadImage(string path)
        {
            ResourceRequest operation = Resources.LoadAsync<Image>(path);
            while (operation.isDone == false)
            {
                await Task.Yield();
            }

            return (Image)operation.asset;
        }

        public async Task<Sprite> LoadSprite(string path)
        {
            ResourceRequest operation = Resources.LoadAsync<Sprite>(path);
            while (operation.isDone == false)
            {
                await Task.Yield();
            }

            return (Sprite)operation.asset;
        }

        public T LoadPrefab<T>(string path) where T : MonoBehaviour
        {
            var prefab = Resources.Load<GameObject>(path);
            
            return prefab.GetComponent<T>();
        }
        
        public async Task<T> LoadPrefabAsync<T>(string path) where T : MonoBehaviour
        {
            ResourceRequest operation = Resources.LoadAsync<GameObject>(path);
            while (operation.isDone == false)
            {
                await Task.Yield();
            }

            GameObject prefab = (GameObject)operation.asset;
            
            return prefab.GetComponent<T>();
        }

        public T Load<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }
        
        public T[] LoadAll<T>(string path) where T : Object
        {
            return Resources.LoadAll<T>(path);
        }
    }
}
