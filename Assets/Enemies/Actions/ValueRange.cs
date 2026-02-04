using System;
using UnityEngine;

namespace Assets.Enemies.Actions
{
    [Serializable]
    public struct ValueRange
    {
        [field: SerializeField] private int Min;
        [field: SerializeField] private int Max;

        public ValueRange(int min, int max)
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
