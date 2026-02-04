using System;

namespace Assets.Enemies.Intentions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IntentionTypeAttribute : Attribute
    {
        public IntentionType IntentionType { get; }

        public IntentionTypeAttribute(IntentionType intentionType)
        {
            IntentionType = intentionType;
        }
    }
}
