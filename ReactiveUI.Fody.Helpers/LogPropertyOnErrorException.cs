using System;

namespace ReactiveUI.Fody.Helpers
{
    public class LogPropertyOnErrorException : Exception
    {
        public new object Source { get; }
        public string Property { get; }

        public LogPropertyOnErrorException(object source, string property, Exception innerException) :
            base($"An exception occurred when a property change notification was fired on {source.GetType().FullName}.{property} on an instance of {source.GetType().FullName}: {source}", innerException)
        {
            Source = source;
            Property = property;
        }
    }
}
