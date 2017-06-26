using System;

namespace DataKeeper
{
    #region enums

    [Flags]
    public enum TriggerType
    {
        Before = 1,
        After = 2,
        Both = Before | After
    }

    [Flags]
    public enum BindType
    {
        OnUpdate = 1,
        OnRemove = 2,
        OnAll = OnUpdate | OnRemove
    }

    public enum ActivityStatus
    {
        Inactive,
        Active
    }

    #endregion

    internal static class Extentions
    {
        public static bool TriggerHasFlag(this TriggerType triggerType, TriggerType flag)
        {
            // Faster than native Enum.HasFlag
            return (triggerType & flag) == flag;
        }

        public static bool BindTypeHasFlag(this BindType bindType, BindType flag)
        {
            return (bindType & flag) == flag;
        }

        internal static dynamic __Add<T>(this object left, T right)
        {
            return (dynamic)left + (dynamic)right;
        }

        internal static dynamic __Substract<T>(this object left, T right)
        {
            return (dynamic)left - (dynamic)right;
        }
    }
}
