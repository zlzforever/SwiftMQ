using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SwiftMQ
{
    public class MessageQueue : IMessageQueue
    {
        private readonly ConcurrentDictionary<string, Channel<dynamic>> _channelDict =
            new ConcurrentDictionary<string, Channel<dynamic>>();

        private MessageQueue()
        {
        }

        private static readonly Lazy<MessageQueue> MyInstance = new Lazy<MessageQueue>(() => new MessageQueue());

        /// <summary>
        /// 单例对象
        /// </summary>
        public static IMessageQueue Instance => MyInstance.Value;

        public bool QueueDeclare(string queue)
        {
            if (!_channelDict.ContainsKey(queue))
            {
                var channel = Channel.CreateUnbounded<dynamic>();
                return _channelDict.TryAdd(queue, channel);
            }

            return true;
        }

        public async Task PublishAsync<TMessage>(string queue, TMessage message)
        {
            if (_channelDict.TryGetValue(queue, out var channel))
            {
                await channel.Writer.WriteAsync(message);
            }
            else
            {
                throw new ApplicationException("Use SwiftMQ.QueueDeclare to create a queue firstly");
            }
        }

        public Task ConsumeAsync<TMessage>(AsyncMessageConsumer<TMessage> consumer)
        {
            if (consumer.Registered)
            {
                throw new ApplicationException("This consumer is already registered");
            }

            if (_channelDict.TryGetValue(consumer.Queue, out var channel))
            {
                consumer.Register();
                return Task.Factory.StartNew(async () =>
                {
                    while (await channel.Reader.WaitToReadAsync())
                    {
                        if (await channel.Reader.ReadAsync() is TMessage message)
                        {
                            await consumer.InvokeAsync(message);
                        }
                    }
                });
            }
            else
            {
                throw new ApplicationException("Use SwiftMQ.QueueDeclare to create a queue firstly");
            }
        }

        public void CloseQueue(string queue)
        {
            if (_channelDict.TryGetValue(queue, out var channel))
            {
                channel.Writer.Complete();
            }
        }
    }
}