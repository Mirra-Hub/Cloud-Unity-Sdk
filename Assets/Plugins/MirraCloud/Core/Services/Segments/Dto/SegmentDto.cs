using System;

namespace Plugins.MirraCloud.Core.Services.Segments.Dto
{
    [Serializable]
    public class SegmentDto
    {
        public string id;
        public string name;
        public string projectId;
        public string description;
        public bool isEnable;
        public string ruleTreeId;
        public string createdDate;
        public string updatedDate;
    }
}