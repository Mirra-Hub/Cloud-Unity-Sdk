using MirraCloud.Core;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class LeaderboardTest : MonoBehaviour
    {
        [SerializeField] private double _score = 123;
        [SerializeField] private string _leaderboardid;
        public string LeaderboardId => _leaderboardid;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //SendEvent();
            }
        }

        public void SendEvent()
        {
            _sdk.Leaderboard.SubmitScoreAsync(_score, _leaderboardid);
        }
    }
}
