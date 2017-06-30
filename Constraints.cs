using System;
using System.Collections.Generic;
using IdCounter = System.Collections.Generic.Dictionary<string, int>;

namespace DataKeeper
{
    public sealed partial class Data
    {
        private IdCounter counter = new IdCounter();

        internal string GenerateConstraintId()
        {
            return GenerateId("constraint");
        }

        // используется для генерации id: prefix_number
        internal string GenerateId(string prefix)
        {
            int count = 0;

            counter.TryGetValue(prefix, out count);
            counter[prefix] = count + 1;
            return prefix + "_" + count;
        }

        internal string ReturnId(string prefix)
        {
            int count = 0;
            bool idExists = counter.TryGetValue(prefix, out count);

            string result = prefix;
            if (idExists)
            {
                result = prefix + "_" + count;
            }
            counter[prefix] = count + 1;

            return result;
        }

        public static class Constraints
        {
            public static Constraint NotNull
            {
                get
                {
                    return new Constraint(
                        Instance.GenerateId("notnull_constraint"),
                        x => x != null
                    );
                }
            }

            public static Constraint ChangeOnly
            {
                get
                {
                    return new Constraint(
                        Instance.GenerateId("changeonly_constraint"),
                        x => x != Instance.OldValue
                    );
                }
            }

            public static Constraint Never
            {
                get
                {
                    return new Constraint(
                        Instance.GenerateId("never_constraint"),
                        x => false
                    );
                }
            }

            public static Constraint TypeOf<T>()
            {
                return new Constraint(
                    Instance.GenerateId($"typeof_{typeof(T)}_constraint"),
                    x => x is T
                );
            }

            #region NUMERIC CONSTRAINTS

            public static Constraint GreaterThan(object obj)
            {
                return new Constraint(
                    Instance.GenerateId("gt_constraint"),
                    x => x > (dynamic)obj
                );
            }

            public static Constraint GreaterThanOrEqual(object obj)
            {
                return new Constraint(
                    Instance.GenerateId("gte_constraint"),
                    x => x >= (dynamic)obj
                );
            }

            public static Constraint LessThan(object obj)
            {
                return new Constraint(
                    Instance.GenerateId("lt_constraint"),
                    x => x < (dynamic)obj
                );
            }

            public static Constraint LessThanOrEqual(object obj)
            {
                return new Constraint(
                    Instance.GenerateId("lte_constraint"),
                    x => x <= (dynamic)obj
                );
            }

            public static Constraint NotEqual(object obj)
            {
                return new Constraint(
                    Instance.GenerateId("ne_constraint"),
                    x => x != (dynamic)obj
                );
            }

            public static Constraint Ascending
            {
                get
                {
                    return new Constraint(
                        Instance.GenerateId("asc_constraint"),
                        x => (Instance.OldValue == null && x != null) || x >= Instance.OldValue
                    );
                }
            }

            public static Constraint Descending
            {
                get
                {
                    return new Constraint(
                        Instance.GenerateId("desc_constraint"),
                        x => (Instance.OldValue == null && x != null) || x <= Instance.OldValue
                    );
                }
            }

            #endregion

        }
    }

    public class Constraint : DataKeeperElement, IComparable<Constraint>, IComparable
    {
        public ConstraintProperty Property { get; set; }

        private Predicate<dynamic> ConstraintPredicate { get; }

        #region CONSTRUCTORS

        public Constraint(Predicate<dynamic> constraint)
            : this(Data.Instance.GenerateConstraintId(), constraint) { }

        public Constraint(Predicate<dynamic> constraint, ConstraintProperty property)
            : this(Data.Instance.GenerateConstraintId(), constraint, property) { }

        public Constraint(string id, Predicate<dynamic> constraint)
            : this(id, constraint, null) { }

        public Constraint(string id, Predicate<dynamic> constraint,
            ConstraintProperty property): base(id)
        {
            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint),
                    $"Parameter \"{nameof(constraint)}\" can't be null!");

            ConstraintPredicate = constraint;
            if (property == null)
                property = ConstraintProperty.Default;

            Property = property;
        }

        #endregion

        public bool Validate(object x)
        {
            if (Property.Status != ActivityStatus.Active)
                return true;
            
            try
            {
                bool result = ConstraintPredicate((dynamic) x);
                return result;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
                throw new DataKeeperTypeMismatch($"Type mismatch on constraint \"{Id}\"");
            }
        }

        public Constraint Copy()
        {
            Constraint result = new Constraint(idPrefix,
                (Predicate<dynamic>)ConstraintPredicate.Clone(),
                Property.Copy());

            return result;
        }

        public override object Clone()
        {
            return this.Copy();
        }

        public static string ErrorString(string id) => $"Error on constarint {id}";
        
        #region COMPARERS (IDE GENERATED)

        public int CompareTo(Constraint other)
        {
            return this.Property.Position - other.Property.Position;
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is Constraint))
                throw new ArgumentException($"Object must be of type {nameof(Constraint)}");
            return CompareTo((Constraint) obj);
        }

        public static bool operator <(Constraint left, Constraint right)
        {
            return Comparer<Constraint>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(Constraint left, Constraint right)
        {
            return Comparer<Constraint>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(Constraint left, Constraint right)
        {
            return Comparer<Constraint>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(Constraint left, Constraint right)
        {
            return Comparer<Constraint>.Default.Compare(left, right) >= 0;
        }
        
        #endregion
    }

    public class ConstraintProperty : DataKeeperPropertyElement
    {
        public string ErrorMessage { get; }

        #region CONSTRUCTORS

        public ConstraintProperty(string errorMessage):
            this(ActivityStatus.Active, errorMessage) {}

        public ConstraintProperty(): this(ActivityStatus.Active) { }

        public ConstraintProperty(ActivityStatus status):
            this(status, null) { }

        public ConstraintProperty(ActivityStatus status, string errorMessage)
            : base(status)
        {
            ErrorMessage = errorMessage;
        }

        #endregion

        public static ConstraintProperty Default
        {
            get
            {
                return new ConstraintProperty();
            }
        }

        public ConstraintProperty Copy()
        {
            return new ConstraintProperty(Status, (string)ErrorMessage?.Clone());
        }

        public override object Clone()
        {
            return this.Copy();
        }
    }
}
