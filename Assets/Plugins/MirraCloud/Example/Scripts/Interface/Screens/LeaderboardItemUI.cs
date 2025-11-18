using MirraCloud.Core.Leaderboard.Dto;
using MirraCloud.Example;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.MirraCloud.Example.Scripts.Interface.Screens
{
    public class LeaderboardItemUI : BaseObjectUI
    {
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TextMeshProUGUI _rangText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _nicknameText;

        public void Initialize(LeaderboardEntryDto leaderboardEntry)
        {
            _rangText.SetText(leaderboardEntry.position.ToString());
            _scoreText.SetText(leaderboardEntry.value.ToString());
            _nicknameText.SetText(leaderboardEntry.playerName);
        }
    }
}