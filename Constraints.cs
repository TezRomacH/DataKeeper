using System;

using ConstraintCounter = System.Collections.Generic.Dictionary<string, int>;

namespace DataKeeper
{
    public sealed partial class Data
    {
        private ConstraintCounter counter = new ConstraintCounter();

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

    public class Constraint : ICloneable
    {
        public string Id { get; }
        public ConstraintProperties Properties { get; set; }

        private Predicate<dynamic> ConstraintPredicate { get; }
        private string idPrefix = null;

        public Constraint(Predicate<dynamic> constraint)
            : this(Data.Instance.GenerateConstraintId(), constraint) { }

        public Constraint(Predicate<dynamic> constraint, ConstraintProperties properties)
            : this(Data.Instance.GenerateConstraintId(), constraint, properties) { }

        public Constraint(string id, Predicate<dynamic> constraint)
            : this(id, constraint, null) { }

        public Constraint(string id, Predicate<dynamic> constraint,
            ConstraintProperties properties)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id), "Parameter \"id\" can't be null!");

            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint), "Parameter \"constraint\" can't be null!");

            idPrefix = id;
            Id = Data.Instance.ReturnId(idPrefix);

            ConstraintPredicate = constraint;
            if (properties == null)
                properties = ConstraintProperties.Default;

            Properties = properties;
        }

        public bool Validate(object x)
        {
            try
            {
                bool result = ConstraintPredicate((dynamic)x);
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
                Properties.Copy());

            return result;
        }

        public object Clone()
        {
            return this.Copy();
        }
    }

    public class ConstraintProperties : ICloneable
    {
        public ActivityStatus Status { get; }
        public string ErrorMessage { get; }

        public ConstraintProperties() : this(ActivityStatus.Active) { }

        public ConstraintProperties(ActivityStatus status) :
            this(status, null)
        { }

        public ConstraintProperties(ActivityStatus status, string errorMessage)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public static ConstraintProperties Default
        {
            get
            {
                return new ConstraintProperties();
            }
        }

        public ConstraintProperties Copy()
        {
            return new ConstraintProperties(Status, (string)ErrorMessage?.Clone());
        }

        public object Clone()
        {
            return this.Copy();
        }
    }
}
