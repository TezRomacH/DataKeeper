using System;

namespace DataKeeper
{
    /// <summary>
    /// Base exception class for all DataKeeper's exceptions
    /// </summary>
    public abstract class DataKeeperException : Exception
    {
        public string ErrorMessage { get; protected set; } = "";
    }

    /// <summary>
    /// Throws if trying to get variables OldValue or NewValue not on a trigger body
    /// </summary>
    public class NotOnTriggerException : DataKeeperException
    {
        public NotOnTriggerException() { }
        
        public NotOnTriggerException(string message)
        {
            ErrorMessage = message;
        }
    }
}