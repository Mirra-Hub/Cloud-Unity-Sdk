using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Example.Scripts;
using UnityEngine;

namespace MirraCloud.Example
{
    public abstract class BaseObjectUI : MonoBehaviour
    {
        protected UIController UIController { get; private set; }

        public void Initialize(UIController uiController)
        {
            UIController = uiController;

            OnInitialized();
        }

        protected virtual void OnInitialized(){}
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}