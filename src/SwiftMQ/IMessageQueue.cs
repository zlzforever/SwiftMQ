using System.Threading;
using System.Threading.Tasks;

namespace SwiftMQ
{
	public interface IMessageQueue
	{
		Task PublishAsync<TMessage>(string queue, TMessage message);

		Task ConsumeAsync<TMessage>(AsyncMessageConsumer<TMessage> consumer, CancellationToken cancellationToken);

		void CloseQueue(string queue);
	}
}