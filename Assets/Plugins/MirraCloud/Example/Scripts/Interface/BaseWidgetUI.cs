using System;

namespace MirraCloud.Example
{
    public abstract class BaseWidgetUI : BaseObjectUI
    {
        private void OnEnable()
        {
            OnEnabledWidget();
        }

        private void OnDisable()
        {
            OnDisableWidget();
        }
        
        protected virtual void OnEnabledWidget() {}
        protected virtual void OnDisableWidget() {}
    }
}