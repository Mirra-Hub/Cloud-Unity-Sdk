using MirraCloud.Core;
using Plugins.MirraCloud.Example.Scripts;
using UnityEngine;

namespace MirraCloud.Example
{
    public class Container
    {
        public static Container Instance { get; private set; }

        public Container()
        {
            Instance = this;
        }
        
        public PlayerProfile PlayerProfile;
    }
    
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private UIController uiController;
        
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            Container container = new Container();
            container.PlayerProfile = new PlayerProfile();
            
            MirraCloudSDK.Initialize();
        }
    }
}