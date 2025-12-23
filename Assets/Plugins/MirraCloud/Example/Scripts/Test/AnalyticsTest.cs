using MirraCloud.Core;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class AnalyticsTest : MonoBehaviour
    {
        [SerializeField] private string _eventid;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendEvent();
            }
        }

        public void SendEvent()
        {
            MirraCloudSDK.Analytics.SendEventAsync(_eventid);
        }
    }
}
