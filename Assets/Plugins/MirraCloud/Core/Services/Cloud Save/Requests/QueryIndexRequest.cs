using System.Collections.Generic;

namespace MirraCloud.Core.CloudSave.Requests
{
    public class QueryIndexRequest
    {
        public string indexId;
        public List<QueryFilter> filters = new List<QueryFilter>();
        public string[] returnKeys;
        public int offset;
        public int limit = 20;
        public int? sampleSize;
    }

    public class QueryFilter
    {
        public string key;
        public CloudSaveIndexOp op = CloudSaveIndexOp.Eq;
        public object value;
        public bool? asc;
    }
}
