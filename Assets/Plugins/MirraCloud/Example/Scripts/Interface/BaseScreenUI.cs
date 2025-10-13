namespace MirraCloud.Example
{
    public abstract class BaseScreenUI : BaseObjectUI
    {
        protected override void OnInitialized()
        {
            var widgets = GetComponentsInChildren<BaseWidgetUI>();

            foreach (var widget in widgets)
            {
                widget.Initialize(UIController);
            }
        }
        
        protected void OnEnable()
        {
            OnEnableScreen();
        }

        protected void OnDisable()
        {
            OnDisableScreen();
        }

        protected virtual void OnEnableScreen(){}
        protected virtual void OnDisableScreen(){}
    }
}