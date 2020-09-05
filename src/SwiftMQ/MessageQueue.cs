using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SwiftMQ
{
	public class MessageQueue : IMessageQueue
	{
		private readonly ConcurrentDictionary<string, Channel<dynamic>> _channelDict =
			new ConcurrentDictionary<string, Channel<dynamic>>();

		public async Task PublishAsync<TMessage>(string queue, TMessage message)
		{
			if (!DeclareQueue(queue))
			{
				throw new ApplicationException("Declare queue failed");
			}

			await _channelDict[queue].Writer.WriteAsync(message);
		}

		public async Task ConsumeAsync<TMessage>(AsyncMessageConsumer<TMessage> consumer,
			CancellationToken cancellationToken)
		{
			if (consumer.Registered)
			{
				throw new ApplicationException("This consumer is already registered");
			}

			if (!DeclareQueue(consumer.Queue))
			{
				throw new ApplicationException("Declare queue failed");
			}

			var channel = _channelDict[consumer.Queue];
			consumer.Register();
			consumer.OnClosing += x => { CompleteQueue(x.Queue); };

			await Task.Factory.StartNew(async () =>
			{
				while (await channel.Reader.WaitToReadAsync(cancellationToken))
				{
					if (await channel.Reader.ReadAsync(cancellationToken) is TMessage message)
					{
						Task.Factory.StartNew(async () => { await consumer.InvokeAsync(message); }, cancellationToken)
							.ConfigureAwait(false).GetAwaiter();
					}
				}
			}, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		private void CompleteQueue(string queue)
		{
			if (_channelDict.TryGetValue(queue, out var channel))
			{
				try
				{
					channel.Writer.Complete();
				}
				catch
				{
					// ignore
				}
			}
		}

		private bool DeclareQueue(string queue)
		{
			if (!_channelDict.ContainsKey(queue))
			{
				var channel = Channel.CreateUnbounded<dynamic>();
				return _channelDict.TryAdd(queue, channel);
			}

			return true;
		}
	}
}