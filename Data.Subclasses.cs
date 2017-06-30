using System;
using System.Collections.Generic;
using System.Linq;

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
            public ICollection<Action> TriggersBeforeUpdate { get; private set; }
            public ICollection<Action> TriggersAfterUpdate { get; private set; }

            public ICollection<Action> TriggersBeforeRemove { get; private set; }
            public ICollection<Action> TriggersAfterRemove { get; private set; }

            public ICollection<Constraint> Constraints { get; }

            public DataInfo()
            {
                Constraints = new LinkedList<Constraint>();
            }

            public void AddConstraint(Constraint constraint)
            {
                if(Constraints.FirstOrDefault(c => c.Id == constraint.Id) != null)
                    throw new ConstraintException(constraint.Id,
                        $"Constraint with the same id \"{constraint.Id}\" exists!");
                Constraints.Add(constraint);
            }

            public void AddBindTriggers(Action action, BindType bindType, TriggerType type)
            {
                if (bindType.BindTypeHasFlag(BindType.OnUpdate))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                    {
                        if (TriggersBeforeUpdate == null)
                            TriggersBeforeUpdate = new List<Action>();

                        TriggersBeforeUpdate.Add(action);
                    }

                    if (type.TriggerHasFlag(TriggerType.After))
                    {
                        if (TriggersAfterUpdate == null)
                            TriggersAfterUpdate = new List<Action>();

                        TriggersAfterUpdate.Add(action);
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
                if (bindType.BindTypeHasFlag(BindType.OnUpdate))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        TriggersBeforeUpdate = null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        TriggersAfterUpdate = null;
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
                if (bindType.BindTypeHasFlag(BindType.OnUpdate))
                {
                    if (type.TriggerHasFlag(TriggerType.Before))
                        result |= TriggersBeforeUpdate != null;

                    if (type.TriggerHasFlag(TriggerType.After))
                        result |= TriggersAfterUpdate != null;
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

        private Constraint FindById(string constraintId)
        {
            foreach (var info in data.Values)
            {
                foreach (var constraint in info.Constraints)
                {
                    if (constraint.Id == constraintId)
                        return constraint;
                }
            }

            return null;
        }
    }

    public abstract class DataKeeperElement : ICloneable
    {
        public string Id { get; }
        public abstract object Clone();
        
        protected string idPrefix { get; }

        public DataKeeperElement(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id), "Parameter \"id\" can't be null!");
            
            this.idPrefix = id;
            Id = Data.Instance.ReturnId(id);
        }

    }
    
    public abstract class DataKeeperPropertyElement : ICloneable
    {
        public DataKeeperPropertyElement(ActivityStatus status)
        {
            Status = status;
            Position = DefaultPosition;
        }

        public DataKeeperPropertyElement(ActivityStatus status, int position)
        {
            Status = status;
            Position = position;
        }

        public ActivityStatus Status { get; private set; }

        private int _position = DefaultPosition;
        public int Position
        {
            get { return _position; }
            private set { _position = NormilizePosition(value); }
        }

        public abstract object Clone();

        private static int NormilizePosition(int value)
        {
            if (value < LowestPosition)
                return LowestPosition;

            if (value > LargestPosition)
                return LargestPosition;

            return value;
        }

        private const int LowestPosition = 0;
        private const int LargestPosition = 20;
        private const int DefaultPosition = 10;

        public void SetPosition(int newPosition)
        {
            Position = newPosition;
        }

        public void SetActivityStatus(ActivityStatus newStatus)
        {
            Status = newStatus;
        }
    }
}
