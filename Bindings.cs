using System;
using System.Collections.Generic;

namespace DataKeeper
{
    public class Binding : DataKeeperElement, IComparable<Binding>, IComparable
    {
        public BindingProperty Property { get; set; }
        
        private Action BindedAction { get; }

        #region CONSTRUCTORS

        public Binding(string id, Action action, BindingProperty property) : base(id)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action),
                    $"Parameter \"{nameof(action)}\" can't be null!");
            BindedAction = action;
            
            if (property == null)
                property = BindingProperty.Default;

            Property = property;
        }

        public void Apply()
        {
            if(Property.Status == ActivityStatus.Active)
                BindedAction?.Invoke();
        }

        #endregion

        public Binding Copy()
        {
            throw new NotImplementedException();
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

        

        #endregion
        
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public static BindingProperty Default
        {
            get
            {
                return new BindingProperty();
            }
        }
    }
}