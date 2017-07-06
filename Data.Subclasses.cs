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
            private string Key { get; }

            public dynamic Value { get; set; }
            public SortedSet<Trigger> BindedTriggers { get; }
            public SortedSet<Constraint> BindedConstraints { get; }

            public DataInfo(string key)
            {
                Key = key;

                BindedConstraints = new SortedSet<Constraint>();
                BindedTriggers = new SortedSet<Trigger>();
            }

            public void AddConstraint(Constraint constraint)
            {
                if(BindedConstraints.FirstOrDefault(c => c.Id == constraint.Id) != null)
                    throw new SameIdExistsException(constraint.Id,
                        $"Constraint with the same id \"{constraint.Id}\" already exists  constraints by \"{Key}\" key!");
                BindedConstraints.Add(constraint);
            }

            public void AddTrigger(Trigger trigger)
            {
                if(BindedTriggers.FirstOrDefault(t => t.Id == trigger.Id) != null)
                    throw new SameIdExistsException(trigger.Id,
                        $"Trigger with the same id \"{trigger.Id}\" already exists in triggers by \"{Key}\" key!");
                BindedTriggers.Add(trigger);
            }

            public bool HasAnyTrigger()
            {
                return BindedTriggers.Count > 0;
            }
        }

        private Constraint FindConstarintById(string constraintId)
        {
            foreach (var info in data.Values)
            {
                foreach (var constraint in info.BindedConstraints)
                {
                    if (constraint.Id == constraintId)
                        return constraint;
                }
            }

            return null;
        }

        private Trigger FindTriggerById(string triggerId)
        {
            foreach (var info in data.Values)
            {
                foreach (var trigger in info.BindedTriggers)
                {
                    if (trigger.Id == triggerId)
                        return trigger;
                }
            }

            return null;
        }

        public DataKeeperElement FindDataKeeperElementById(string id)
        {
            return (DataKeeperElement) FindTriggerById(id) ?? FindConstarintById(id);
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
                throw new ArgumentNullException(nameof(id), $"Parameter \"{nameof(id)}\" can't be null!");
            
            this.idPrefix = id;
            Id = Ids.Container.ReturnId(id);
        }

    }

    public abstract class DataKeeperPropertyElement : ICloneable
    {
        public ActivityStatus ActivityStatus { get; private set; }

        private int _position = DefaultPosition;
        public int Position
        {
            get { return _position; }
            private set { _position = NormilizePosition(value); }
        }

        #region CONSTRUCTORS

        public DataKeeperPropertyElement(ActivityStatus activityStatus)
        {
            ActivityStatus = activityStatus;
            Position = DefaultPosition;
        }

        public DataKeeperPropertyElement(ActivityStatus activityStatus, int position)
        {
            ActivityStatus = activityStatus;
            Position = position;
        }

        #endregion

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
            ActivityStatus = newStatus;
        }
    }
}
