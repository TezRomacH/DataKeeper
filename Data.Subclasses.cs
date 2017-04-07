using System;
using System.Collections.Generic;

namespace DataKeeper
{
    public sealed partial class Data
    {
        private readonly Dictionary<string, DataInfo> data;
        private readonly ISet<string> removedKeys;

        private static volatile Data instance;
        private static object syncRoot = new object();

        private Data()
        {
            data = new Dictionary<string, DataInfo>();
            removedKeys = new SortedSet<string>();
        }

        private bool IsKeyRemoved(string key) => removedKeys.Contains(key);

        private class DataInfo
        {
            public dynamic Value { get; set; }
            public ICollection<Action> TriggersBeforeChange { get; private set; }
            public ICollection<Action> TriggersAfterChange { get; private set; }

            public ICollection<Action> TriggersBeforeRemove { get; private set; }
            public ICollection<Action> TriggersAfterRemove { get; private set; }

            public void AddBindTriggers(Action action, BindType bindType, TriggerType type)
            {
                if (bindType.BindTypeHasFlag(BindType.OnChange))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                    {
                        if (TriggersBeforeChange == null)
                            TriggersBeforeChange = new List<Action>();

                        TriggersBeforeChange.Add(action);
                    }

                    if (type.TriggerHasFlag(TriggerType.After))
                    {
                        if (TriggersAfterChange == null)
                            TriggersAfterChange = new List<Action>();

                        TriggersAfterChange.Add(action);
                    }
                }

                if (bindType.BindTypeHasFlag(BindType.OnRemove))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                    {
                        if (TriggersBeforeRemove == null)
                            TriggersBeforeRemove = new List<Action>();

                        TriggersBeforeRemove.Add(action);
                    }

                    if (type.TriggerHasFlag(TriggerType.After))
                    {
                        if (TriggersAfterRemove == null)
                            TriggersAfterRemove = new List<Action>();

                        TriggersAfterRemove.Add(action);
                    }
                }
            }

            public void RemoveBindTriggers(BindType bindType, TriggerType type)
            {
                if (bindType.BindTypeHasFlag(BindType.OnChange))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        TriggersBeforeChange = null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        TriggersAfterChange = null;
                }

                if (bindType.BindTypeHasFlag(BindType.OnRemove))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        TriggersBeforeRemove = null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        TriggersAfterRemove = null;
                }
            }

            public bool HasAnyTrigger(BindType bindType = BindType.OnAll, TriggerType type = TriggerType.Both)
            {
                bool result = false;
                if (bindType.BindTypeHasFlag(BindType.OnChange))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        result |= TriggersBeforeChange != null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        result |= TriggersAfterChange != null;
                }

                if (bindType.BindTypeHasFlag(BindType.OnRemove))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        result |= TriggersBeforeRemove != null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        result |= TriggersAfterRemove != null;
                }

                return result;
            }
        }
    }
}
