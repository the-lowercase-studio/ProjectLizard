using System;
using UnityEngine;

namespace Assets.CustomTypes.ValueRanges
{
    [Serializable]
    public struct FloatValueRange
    {
        [field: SerializeField] public float Min;
        [field: SerializeField] public float Max;

        public FloatValueRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float GetRandomValueFromRange()
        {
            return UnityEngine.Random.Range(Min, Max + 1f);
        }
    }
}
