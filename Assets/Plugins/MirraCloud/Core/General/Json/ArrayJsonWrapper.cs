using System;

namespace MirraCloud.Core.CloudSave
{
    [Serializable]
    public class ArrayJsonWrapper<T>
    {
        public T[] Values;

        public ArrayJsonWrapper(T[] values)
        {
            Values = values;
        }
    }
}