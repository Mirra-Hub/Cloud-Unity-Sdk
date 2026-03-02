using System.Collections.Generic;
using MirraCloud.Core;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class AnalyticsTest : MonoBehaviour
    {
        [SerializeField] private string _eventId;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendEvent();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendEventWithParameters();
            }
        }

        public void SendEvent()
        {
            MirraCloudSDK.Analytics.SendEventAsync(_eventId);
        }

        public void SendEventWithParameters()
        {
            MirraCloudSDK.Analytics.SendEventAsync(_eventId, new Dictionary<string, string>
            {
                { "itemId", "sword_01" },
                { "price", "100" },
                { "currency", "gold" }
            });
        }
    }
}
