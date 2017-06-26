using System;

namespace DataKeeper
{
    /// <summary>
    /// Base exception class for all DataKeeper's exceptions
    /// </summary>
    public abstract class DataKeeperException : Exception
    {
        public string ErrorMessage { get; }

        protected DataKeeperException() : this(string.Empty) { }

        protected DataKeeperException(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Exception is thrown if trying to get variables OldValue or NewValue not on a trigger body
    /// </summary>
    public class NotOnTriggerException : DataKeeperException
    {
        public NotOnTriggerException() : base() { }

        public NotOnTriggerException(string message) : base(message) { }
    }

    public class DataKeeperTypeMismatch : DataKeeperException
    {
        public DataKeeperTypeMismatch() : base() { }

        public DataKeeperTypeMismatch(string message) : base(message) { }
    }

    /// <summary>
    /// Exception for constraints
    /// </summary>
    public class ConstraintException : DataKeeperException
    {
        public string ConstraintId { get; }

        public ConstraintException(string constraintId)
        {
            ConstraintId = constraintId;
        }

        public ConstraintException(string constraintId, string message) : base(message)
        {
            ConstraintId = constraintId;
        }
    }
}
