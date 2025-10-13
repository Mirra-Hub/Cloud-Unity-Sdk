using System.Collections;
using MirraCloud;
using MirraCloud.Example;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.MirraCloud.Example.Scripts.Interface.Popups
{
    public class ChangePlayerNamePopupUI : BasePopupUI
    {
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private Button _changeButton;
        
        private PlayerProfile _playerProfile;

        public void Construct(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        protected override void OnEnablePopup()
        {
            _nameInputField.text = _playerProfile.Name;
            
            _changeButton.onClick.AddListener(ChangeName);
        }

        protected override void OnDisablePopup()
        {
            _changeButton.onClick.RemoveListener(ChangeName);
        }

        private void ChangeName()
        {
            StartCoroutine(ChangeNameRoutine());
        }

        private IEnumerator ChangeNameRoutine()
        {
            IBaseRestApiOperation operation = _playerProfile.ChangeName(_nameInputField.text);

            yield return operation;

            if (operation.IsSuccess)
            {
                UIController.HideLastPopup();
            }
        }
    }
}
