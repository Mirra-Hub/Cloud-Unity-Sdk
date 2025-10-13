using System;
using System.Collections.Generic;

namespace MirraCloud.Core.CloudSave
{
    [Serializable]
    public class ListJsonWrapper<T>
    {
        public List<T> Values;

        public ListJsonWrapper(List<T> values)
        {
            Values = values;
        }
    }
}