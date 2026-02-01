using System;
using System.Collections.Generic;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.Tournaments;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class TournamentsTest : MonoBehaviour
    {
        [SerializeField] private double _score = 123;
        [SerializeField] private string _leaderboardid;
        [SerializeField] private List<TournamentConfig> _configs;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendEvent();
            }
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                Init();
            }
        }

        private async void Init()
        {
            await MirraCloudSDK.Tournaments.InitializeAsync().Task();

            _configs = new List<TournamentConfig>(MirraCloudSDK.Tournaments.TournamentConfigs);
        }

        public void SendEvent()
        {
            
        }
    }
}
