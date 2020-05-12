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

		private long _publishCounter;
		private long _consumeCounter;

		public long PublishedCount => Interlocked.Read(ref _publishCounter);

		public long ConsumedCount => Interlocked.Read(ref _consumeCounter);

		public async Task PublishAsync<TMessage>(string queue, TMessage message) where TMessage : class
		{
			Interlocked.Increment(ref _publishCounter);
			if (!DeclareQueue(queue))
			{
				throw new ApplicationException("Declare queue failed");
			}

			await _channelDict[queue].Writer.WriteAsync(message);
		}

		public async Task ConsumeAsync<TMessage>(AsyncMessageConsumer<TMessage> consumer,
			CancellationToken cancellationToken) where TMessage : class
		{
			if (consumer.Registered)
			{
				throw new ApplicationException("This consumer is already registered");
			}

			Interlocked.Increment(ref _consumeCounter);
			if (!DeclareQueue(consumer.Queue))
			{
				throw new ApplicationException("Declare queue failed");
			}

			var channel = _channelDict[consumer.Queue];
			consumer.Register();

			await Task.Factory.StartNew(async () =>
			{
				while (await channel.Reader.WaitToReadAsync(cancellationToken))
				{
					if (await channel.Reader.ReadAsync(cancellationToken) is TMessage message)
					{
						await consumer.InvokeAsync(message);
					}
				}
			}, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		public void CloseQueue(string queue)
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