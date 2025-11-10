using System;

namespace Common.Messaging.Kafka.Interfaces
{
    public interface IRetryPolicy
    {
        void Execute(Action action, Action<Exception> onFailure);
    }
}
