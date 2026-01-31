using System;
using System.Collections.Generic;
using MirraCloud.Example;
using MirraCloud.Example.Infrastructure.DI;
using MirraCloud.Example.Interface;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Transform _screensRoot;
        [SerializeField] private Transform _popupsRoot;
        
        private readonly Dictionary<Type, BaseScreenUI> _screensMap = new Dictionary<Type, BaseScreenUI>();
        private readonly Dictionary<Type, BasePopupUI> _popupsMap = new Dictionary<Type, BasePopupUI>();

        private BaseScreenUI _currentScreen;

        private readonly Stack<BasePopupUI> _currentPopups = new Stack<BasePopupUI>();
        private FactoryUI _factoryUI;

        [InjectDep]
        public void Construct(FactoryUI factoryUI)
        {
            _factoryUI = factoryUI;
        }

        public void ShowScreen<T>() where T : BaseScreenUI
        {
            if (_screensMap.TryGetValue(typeof(T), out var screen) == false)
            {
                screen = _factoryUI.CreateScreen<T>(_screensRoot);
                screen.Initialize(this);
                _screensMap.Add(typeof(T), screen);
            }
                  
            if (_currentScreen != null)
            {
                _currentScreen.Hide();
            }
                
            _currentScreen = screen;
            _currentScreen.Show();
        }

        public void ShowPopup<T>() where T : BasePopupUI
        {
            if (_popupsMap.TryGetValue(typeof(T), out var popup) == false)
            {
                popup = _factoryUI.CreatePopup<T>(_popupsRoot);
                popup.Initialize(this);
                _popupsMap.Add(typeof(T), popup);
            }
            
            popup.Show();
            _currentPopups.Push(popup);
        }

        public void HideLastPopup()
        {
            if (_currentPopups.TryPeek(out var popup))
            {
                popup.Hide();
            }
        }
    }
}
