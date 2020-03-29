using System.Threading.Tasks;

namespace SwiftMQ
{
	public delegate Task AsyncMessageHandler<in TMessage>(TMessage message);
}