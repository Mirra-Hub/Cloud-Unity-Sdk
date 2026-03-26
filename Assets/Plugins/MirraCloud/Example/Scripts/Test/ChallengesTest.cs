using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Example.Infrastructure.DI;
using Plugins.MirraCloud.Core.Services.Challenges.Entities;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class ChallengesTest : MonoBehaviour
    {
        [SerializeField] private string _challengeId;
        [SerializeField] private double _score = 100;
        [SerializeField] private string _playerName;
        [SerializeField] private int _entriesCount = 100;
        [SerializeField] private int _entriesRange = 10;
        [SerializeField] private List<ChallengeConfig> _configs;

        private IMirraCloudSdk _sdk;

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Initialize();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GetConfig();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SubmitScore();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Join();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Leave();
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                GetTop();
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                GetMyTop();
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                GetAroundPlayer();
            }

            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                GetPlayer();
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                ClaimReward();
            }
        }

        public async void Initialize()
        {
            await _sdk.Challenges.InitializeAsync().Task();

            _configs = new List<ChallengeConfig>(_sdk.Challenges.ChallengeConfigs);
            Debug.Log($"[Challenges] Initialized, loaded {_configs.Count} configs");
        }

        public void GetConfig()
        {
            var operation = _sdk.Challenges.GetConfigAsync(_challengeId);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] Config: {completed.Result.Data.id} - {completed.Result.Data.name}");
                }
                else
                {
                    Debug.LogError($"[Challenges] GetConfig failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void SubmitScore()
        {
            var name = string.IsNullOrEmpty(_playerName) ? null : _playerName;
            var operation = _sdk.Challenges.SubmitScoreAsync(_challengeId, _score, name);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] Score {_score} submitted to {_challengeId}");
                }
                else
                {
                    Debug.LogError($"[Challenges] SubmitScore failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void Join()
        {
            var operation = _sdk.Challenges.JoinAsync(_challengeId);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] Joined challenge {_challengeId}");
                }
                else
                {
                    Debug.LogError($"[Challenges] Join failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void Leave()
        {
            var operation = _sdk.Challenges.LeaveAsync(_challengeId);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] Left challenge {_challengeId}");
                }
                else
                {
                    Debug.LogError($"[Challenges] Leave failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void GetTop()
        {
            var operation = _sdk.Challenges.GetTopAsync(_challengeId, _entriesCount);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] Top entries for {_challengeId}: {completed.Result.Data.entries?.Length ?? 0}");
                }
                else
                {
                    Debug.LogError($"[Challenges] GetTop failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void GetMyTop()
        {
            var operation = _sdk.Challenges.GetMyTopAsync(_challengeId, _entriesCount);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] My top entries for {_challengeId}: {completed.Result.Data.entries?.Length ?? 0}");
                }
                else
                {
                    Debug.LogError($"[Challenges] GetMyTop failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void GetAroundPlayer()
        {
            var operation = _sdk.Challenges.GetAroundPlayerAsync(_challengeId, _entriesRange);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] Around player entries for {_challengeId}: {completed.Result.Data.entries?.Length ?? 0}");
                }
                else
                {
                    Debug.LogError($"[Challenges] GetAroundPlayer failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void GetPlayer()
        {
            var operation = _sdk.Challenges.GetPlayerAsync(_challengeId);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    var entry = completed.Result.Data;
                    Debug.Log($"[Challenges] Player entry: Position={entry.position}, Value={entry.value}, IsFinished={entry.isFinished}");
                }
                else
                {
                    Debug.LogError($"[Challenges] GetPlayer failed: {completed.Result.ErrorMessage}");
                }
            };
        }

        public void ClaimReward()
        {
            var operation = _sdk.Challenges.ClaimRewardAsync(_challengeId);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[Challenges] Reward claimed for {_challengeId}");
                }
                else
                {
                    Debug.LogError($"[Challenges] ClaimReward failed: {completed.Result.ErrorMessage}");
                }
            };
        }
    }
}
