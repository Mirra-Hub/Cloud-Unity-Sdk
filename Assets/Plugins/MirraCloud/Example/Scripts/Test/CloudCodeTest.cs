using System.Collections.Generic;
using MirraCloud.Core;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class CloudCodeTest : MonoBehaviour
    {
        [SerializeField] private string _scriptId;
        [SerializeField] private int _numberValue = 10;
        [SerializeField] private int _factorValue = 2;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Execute();
            }
        }

        public void Execute()
        {
            var input = new Dictionary<string, object>
            {
                ["numberValue"] = _numberValue,
                ["factorValue"] = _factorValue
            };

            var op = MirraCloudSDK.CloudCode.ExecuteAsync<double>(_scriptId, input);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    Debug.LogError(completed.Result.Error?.Message ?? "Cloud Code execute failed.");
                    return;
                }

                Debug.Log($"Cloud Code result: {completed.Result.Data}");
            };
        }
    }
}

