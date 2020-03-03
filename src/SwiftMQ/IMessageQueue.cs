using System.Threading.Tasks;

namespace SwiftMQ
{
    public interface IMessageQueue
    {
        bool QueueDeclare(string queue);
        Task PublishAsync<TMessage>(string queue, TMessage message);
        Task ConsumeAsync<TMessage>(AsyncMessageConsumer<TMessage> consumer);
        void CloseQueue(string queue);
    }
}