using MirraCloud.Core;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class LeaderboardTest : MonoBehaviour
    {
        [SerializeField] private double _score = 123;
        [SerializeField] private string _leaderboardid;
        public string LeaderboardId => _leaderboardid;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //SendEvent();
            }
        }

        public void SendEvent()
        {
            MirraCloudSDK.Leaderboard.SubmitScoreAsync(_score, _leaderboardid);
        }
    }
}
