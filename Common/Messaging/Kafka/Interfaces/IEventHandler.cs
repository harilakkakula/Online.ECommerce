using System.Threading.Tasks;

namespace Common.Messaging.Kafka.Interfaces
{
    public interface IEventHandler
    {
        string Topic { get; }
        void Handle(string message);
    }
}
