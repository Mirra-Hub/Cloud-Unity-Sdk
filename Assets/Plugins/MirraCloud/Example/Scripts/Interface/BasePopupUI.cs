using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public abstract class BasePopupUI : BaseObjectUI
    {
        [SerializeField] private Button[] _closeElements;
        
        protected void OnEnable()
        {
            OnEnablePopup();

            foreach (var closeElement in _closeElements)
            {
                closeElement.onClick.AddListener(Hide);
            }
        }

        protected void OnDisable()
        {
            OnDisablePopup();
            
            foreach (var closeElement in _closeElements)
            {
                closeElement.onClick.RemoveListener(Hide);
            }
        }

        protected virtual void OnEnablePopup(){}
        protected virtual void OnDisablePopup(){}
    }
}