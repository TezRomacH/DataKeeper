using System;
using System.Collections.Generic;

namespace DataKeeper
{
    public class Trigger : DataKeeperElement, IComparable<Trigger>, IComparable
    {
        public TriggerProperty Property { get; set; }

        private Action BindedAction { get; }

        #region CONSTRUCTORS

        public Trigger(Action action)
            : this(Ids.Container.GenerateTriggerId(), action) { }

        public Trigger(Action action, TriggerProperty property)
            : this(Ids.Container.GenerateTriggerId(), action, property) { }

        public Trigger(string id, Action action)
            : this(id, action, null) { }

        public Trigger(string id, Action action, TriggerProperty property) : base(id)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action),
                    $"Parameter \"{nameof(action)}\" can't be null!");
            BindedAction = action;

            if (property == null)
                property = TriggerProperty.DefaultAfterUpdate;

            Property = property;
        }

        #endregion

        public bool IsOnUpdate => Property.BindType.BindTypeHasFlag(BindType.OnUpdate);
        public bool IsOnRemove => Property.BindType.BindTypeHasFlag(BindType.OnRemove);

        public bool IsAfter => Property.TriggerType.TriggerHasFlag(TriggerType.After);
        public bool IsBefore => Property.TriggerType.TriggerHasFlag(TriggerType.Before);

        public void Apply()
        {
            if (Property.ActivityStatus == ActivityStatus.Active)
                BindedAction?.Invoke();
        }

        public Trigger Copy()
        {
            Trigger result = new Trigger(idPrefix,
                (Action) BindedAction.Clone(),
                Property.Copy());

            return result;
        }

        public override object Clone()
        {
            return this.Copy();
        }

        #region COMPARERS (IDE GENERATED)

        public int CompareTo(Trigger other)
        {
            var result = this.Property.Position - other.Property.Position;
            return result == 0 ? 1 : result;
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is Trigger))
                throw new ArgumentException($"Object must be of type {nameof(Trigger)}");
            return CompareTo((Trigger) obj);
        }

        public static bool operator <(Trigger left, Trigger right)
        {
            return Comparer<Trigger>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(Trigger left, Trigger right)
        {
            return Comparer<Trigger>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(Trigger left, Trigger right)
        {
            return Comparer<Trigger>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(Trigger left, Trigger right)
        {
            return Comparer<Trigger>.Default.Compare(left, right) >= 0;
        }

        #endregion
    }

    public class TriggerProperty : DataKeeperPropertyElement
    {
        public BindType BindType { get; private set; }
        public TriggerType TriggerType { get; private set; }

        #region CONSTRUCTORS
        
        public TriggerProperty(BindType bindType, TriggerType triggerType)
            : base(ActivityStatus.Active)
        {
            this.BindType = bindType;
            this.TriggerType = triggerType;
        }

        public TriggerProperty(
            BindType bindType,
            TriggerType triggerType,
            ActivityStatus activityStatus,
            int position
        ) : base(activityStatus, position)
        {
            this.BindType = bindType;
            this.TriggerType = triggerType;
        }

        #endregion

        public TriggerProperty Copy()
        {
            return new TriggerProperty(BindType, TriggerType, ActivityStatus, Position);
        }

        public override object Clone()
        {
            return this.Copy();
        }

        public static TriggerProperty DefaultAfterUpdate
        {
            get { return new TriggerProperty(BindType.OnUpdate, TriggerType.After); }
        }

        public static TriggerProperty DefaultBeforeRemove
        {
            get { return new TriggerProperty(BindType.OnRemove, TriggerType.Before); }
        }
    }
}
