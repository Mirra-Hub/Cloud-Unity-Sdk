using Plugins.MirraCloud.Example.Scripts;
using Plugins.MirraCloud.Example.Scripts.Interface.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class PlayerInfoWidget : BaseWidgetUI
    {
        [SerializeField] private TextMeshProUGUI _playerNameLabel;
        [SerializeField] private TextMeshProUGUI _playerIdLabel;
        [SerializeField] private Button _changeNameButton;
       
        private PlayerProfile _playerProfile;

        public void Construct(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        protected override void OnEnabledWidget()
        {
            _playerProfile.OnPlayerInfoChanged += RefreshInfo;
            _changeNameButton.onClick.AddListener(ChangeNameClicked);

            RefreshInfo();
        }

        protected override void OnDisableWidget()
        {
            _playerProfile.OnPlayerInfoChanged -= RefreshInfo;

            _changeNameButton.onClick.RemoveListener(ChangeNameClicked);
        }

        private void ChangeNameClicked()
        {
            UIController.ShowPopup<ChangePlayerNamePopupUI>();
        }

        private void RefreshInfo()
        {
            _playerIdLabel.text = _playerProfile.PlayerId;
            _playerNameLabel.text = _playerProfile.Name;
        }
    }
}