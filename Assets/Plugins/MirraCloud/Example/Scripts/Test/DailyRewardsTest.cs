using MirraCloud.Core;
using MirraCloud.Core.DailyRewards.Dto;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class DailyRewardsTest : MonoBehaviour
    {
        [SerializeField] private string _calendarId;
        [SerializeField] private int _dayNumber;

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
                GetCalendars();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GetAllStatuses();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GetCalendarStatus();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Claim();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ClaimWithDay();
            }
        }

        public void GetCalendars()
        {
            var operation = _sdk.DailyRewards.GetCalendarsAsync();
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    foreach (var calendar in completed.Result.Data)
                    {
                        Debug.Log($"[DailyRewards] Calendar: {calendar.id} - {calendar.name}");
                    }
                }
                else
                {
                    Debug.LogError($"[DailyRewards] GetCalendars failed: {completed.Result}");
                }
            };
        }

        public void GetAllStatuses()
        {
            var operation = _sdk.DailyRewards.GetStatusAsync();
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    foreach (var status in completed.Result.Data)
                    {
                        Debug.Log($"[DailyRewards] Status: CalendarId={status.calendarId}, CurrentDay={status.currentDayNumber}, CanClaim={status.canClaimToday}");
                    }
                }
                else
                {
                    Debug.LogError($"[DailyRewards] GetAllStatuses failed: {completed.Result}");
                }
            };
        }

        public void GetCalendarStatus()
        {
            var operation = _sdk.DailyRewards.GetStatusAsync(_calendarId);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    var status = completed.Result.Data;
                    Debug.Log($"[DailyRewards] Status for {_calendarId}: CurrentDay={status.currentDayNumber}, CanClaim={status.canClaimToday}");
                }
                else
                {
                    Debug.LogError($"[DailyRewards] GetCalendarStatus failed: {completed.Result}");
                }
            };
        }

        public void Claim()
        {
            var operation = _sdk.DailyRewards.ClaimAsync(_calendarId);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[DailyRewards] Claimed reward for calendar {_calendarId}");
                }
                else
                {
                    Debug.LogError($"[DailyRewards] Claim failed: {completed.Result}");
                }
            };
        }

        public void ClaimWithDay()
        {
            var operation = _sdk.DailyRewards.ClaimAsync(_calendarId, _dayNumber);
            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Debug.Log($"[DailyRewards] Claimed reward for calendar {_calendarId}, day {_dayNumber}");
                }
                else
                {
                    Debug.LogError($"[DailyRewards] ClaimWithDay failed: {completed.Result}");
                }
            };
        }
    }
}
