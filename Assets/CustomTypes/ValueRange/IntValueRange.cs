using System;
using UnityEngine;

namespace Assets.CustomTypes.ValueRanges
{
    [Serializable]
    public struct IntValueRange
    {
        [field: SerializeField] public int Min;
        [field: SerializeField] public int Max;

        public IntValueRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int GetRandomValueFromRange()
        {
            return UnityEngine.Random.Range(Min, Max + 1);
        }
    }
}
