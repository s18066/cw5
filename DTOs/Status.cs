using System;

namespace Wyklad5.DTOs
{
    public class Status
    {
        protected Status(bool isSucceeded, string reason)
        {
            IsSucceeded = isSucceeded;
            Reason = reason;
        }

        public bool IsSucceeded { get; }
        
        public string Reason { get; }
        
        public static Status Failed(string reason) => new Status(false, reason);
        
        public static Status Succeeded() => new Status(true, string.Empty);
    }
    
    public class Status<T> : Status where T : class
    {
        private Status(bool isSucceeded, T value, string reason) : base(isSucceeded, reason)
        {
            Value = value;
        }
        
        public T Value { get; }
        
        public static Status<T> Failed(string reason) => new Status<T>(false, null, reason);
        
        public static Status<T> Succeeded(T value) => new Status<T>(true, value, string.Empty);
    }
}