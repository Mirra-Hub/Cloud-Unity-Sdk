using System;

namespace MirraCloud.Core.ProfanityFilter.Requests
{
    /// <summary>Request body for <c>POST /profanity-filter/v1/projects/{projectId}/check</c>.</summary>
    [Serializable]
    public class ProfanityCheckRequest
    {
        public string text;
        public string groupName;
    }
}
