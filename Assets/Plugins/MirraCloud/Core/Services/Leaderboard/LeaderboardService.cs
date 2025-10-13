using System;
using MirraCloud.Core.Logger;

namespace MirraCloud.Core.Leaderboard
{
    public class LeaderboardService 
    {        
        private const string ControllerApi = "api/leaderboard/v1/";
        
        private readonly IJsonService _jsonService;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;

        public LeaderboardService(Configuration configuration, ILogger logger, IJsonService jsonService, RestApiClient restApi) 
        {
            _restApi = restApi;
            _logger = logger;
            _jsonService = jsonService;
        }

        /*public IRequestOperation SetScore(string leaderboardId, int score)
        {
            RequestOptions options = new RequestOptions();
            options.Uri = GetUrl("set-score-value");
            options.Headers.Add(_authContext.HeaderKey, _authContext.BearerToken);
            options.Data = new SetScoreValueRequest()
            {
                Score = score,
                Key = leaderboardId,
                ProjectId = Configuration.ProjectId,
            };

            IRequestOperation operation = _restApi.Post(options);

            return operation;
        }*/
    }
    
    [Serializable]
    public struct SetScoreValueRequest
    {
        public string Key;
        public string ProjectId;
        public int Score;
    }
}
