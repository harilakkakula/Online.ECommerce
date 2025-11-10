namespace Common.Messaging.Kafka.Interfaces
{
    public interface IMessageDispatcher
    {
        void Dispatch(string topic, string message);
    }
}
