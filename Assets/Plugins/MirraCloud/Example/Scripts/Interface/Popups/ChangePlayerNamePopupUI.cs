using System.Collections;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Example;
using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Core.General.AsyncOperations;
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

        [InjectDep]
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
            AsyncOperation<RestApiResult> operation = _playerProfile.ChangeNameAsync(_nameInputField.text);

            yield return operation;

            if (operation.Result.IsSuccess)
            {
                UIController.HideLastPopup();
            }
        }
    }
}
