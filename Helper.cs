using System;
using System.Collections.Generic;

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
        OnChange = 1,
        OnRemove = 2,
        OnAll = OnChange | OnRemove
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

        public static void InvokeAll(this IEnumerable<Action> actions)
        {
            if (actions == null)
                return;

            foreach (var action in actions)
            {
                action?.Invoke();
            }
        }
    }
}
