using System;

namespace ReactiveUI.Fody.Helpers
{
    internal class LogPropertyOnErrorObservable<T> : IObservable<T>
    {
        private readonly IObservable<T> @this;
        private readonly object source;
        private readonly string property;

        public LogPropertyOnErrorObservable(IObservable<T> @this, object source, string property)
        {
            this.@this = @this;
            this.source = source;
            this.property = property;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return @this.Subscribe(new LogPropertyOnErrorObserver(observer, source, property));
        }

        private class LogPropertyOnErrorObserver : IObserver<T>
        {
            private readonly IObserver<T> @this;
            private readonly object source;
            private readonly string property;

            public LogPropertyOnErrorObserver(IObserver<T> @this, object source, string property)
            {
                this.@this = @this;
                this.source = source;
                this.property = property;
            }

            public void OnCompleted()
            {
                @this.OnCompleted();
            }

            public void OnError(Exception error)
            {
                @this.OnError(new LogPropertyOnErrorException(source, property, error));
            }

            public void OnNext(T value)
            {
                @this.OnNext(value);
            }
        }
    }
}
