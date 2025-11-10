namespace Common.Messaging.Kafka.Interfaces
{
    public interface IDeadLetterProducer
    {
        void Send(string message, string error);
    }
}
