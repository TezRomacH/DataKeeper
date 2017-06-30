using System;
using System.Collections.Generic;

namespace DataKeeper
{
    public class Binding : DataKeeperElement, IComparable<Binding>, IComparable
    {
        public BindingProperty Property { get; set; }

        private Action BindedAction { get; }

        #region CONSTRUCTORS

        public Binding(Action action)
            : this(Data.Instance.GenerateConstraintId(), action)
        {
        }

        public Binding(Action action, BindingProperty property)
            : this(Data.Instance.GenerateConstraintId(), action, property)
        {
        }

        public Binding(string id, Action action)
            : this(id, action, null)
        {
        }

        public Binding(string id, Action action, BindingProperty property) : base(id)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action),
                    $"Parameter \"{nameof(action)}\" can't be null!");
            BindedAction = action;

            if (property == null)
                property = BindingProperty.DefaultAfterUpdate;

            Property = property;
        }

        public void Apply()
        {
            if (Property.Status == ActivityStatus.Active)
                BindedAction?.Invoke();
        }

        #endregion

        public Binding Copy()
        {
            Binding result = new Binding(idPrefix,
                (Action) BindedAction.Clone(),
                Property.Copy());

            return result;
        }

        public override object Clone()
        {
            return this.Copy();
        }

        #region COMPARERS (IDE GENERATED)

        public int CompareTo(Binding other)
        {
            return this.Property.Position - other.Property.Position;
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is Binding))
                throw new ArgumentException($"Object must be of type {nameof(Binding)}");
            return CompareTo((Binding) obj);
        }

        public static bool operator <(Binding left, Binding right)
        {
            return Comparer<Binding>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(Binding left, Binding right)
        {
            return Comparer<Binding>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(Binding left, Binding right)
        {
            return Comparer<Binding>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(Binding left, Binding right)
        {
            return Comparer<Binding>.Default.Compare(left, right) >= 0;
        }

        #endregion
    }

    public class BindingProperty : DataKeeperPropertyElement
    {
        public BindType BindType { get; private set; }
        public TriggerType TriggerType { get; private set; }

        #region CONSTRUCTORS
        
        public BindingProperty(BindType bindType, TriggerType triggerType)
            : base(ActivityStatus.Active)
        {
            this.BindType = bindType;
            this.TriggerType = triggerType;
        }

        public BindingProperty(
            BindType bindType,
            TriggerType triggerType,
            ActivityStatus status,
            int position
        ) : base(status, position)
        {
            this.BindType = bindType;
            this.TriggerType = triggerType;
        }

        #endregion

        public BindingProperty Copy()
        {
            return new BindingProperty(BindType, TriggerType, Status, Position);
        }

        public override object Clone()
        {
            return this.Copy();
        }

        public static BindingProperty DefaultAfterUpdate
        {
            get { return new BindingProperty(BindType.OnUpdate, TriggerType.After); }
        }

        public static BindingProperty DefaultBeforeRemove
        {
            get { return new BindingProperty(BindType.OnRemove, TriggerType.Before); }
        }
    }
}