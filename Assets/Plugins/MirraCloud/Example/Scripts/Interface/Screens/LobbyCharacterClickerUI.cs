using System;
using DG.Tweening;
using Plugins.MirraCloud.Example.Scripts.Test;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MirraCloud.Example
{
    public class LobbyCharacterClickerUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform _characterTransform;
        
        private LeaderboardTest _leaderboardTest;
        private Sequence _sequence;

        private void Awake()
        {
            _leaderboardTest = FindObjectOfType<LeaderboardTest>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // _leaderboardTest.SendEvent();

            // _sequence?.Kill();
            // _characterTransform.transform.localScale = Vector3.one;

            // _sequence = DOTween.Sequence();
            
            // _sequence
            //     .Append(_characterTransform.DOScale(1.1f, 0.2f))
            //     .Append(_characterTransform.DOScale(1f, 0.2f))
            //    .SetEase(Ease.InOutSine);
        }
    }
}