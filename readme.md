# SwiftMQ

A simple memory message queue via Channel

## Sample

```c#
            MessageQueue.Instance.QueueDeclare("test");
            var consumer1 = new AsyncMessageConsumer("test");
            consumer1.Received += message =>
            {
                Console.WriteLine($"handler 1: {message}");
                return Task.CompletedTask;
            };
            await MessageQueue.Instance.ConsumeAsync(consumer1);

            var consumer2 = new AsyncMessageConsumer("test");
            consumer2.Received += message =>
            {
                Console.WriteLine($"handler 2: {message}");
                return Task.CompletedTask;
            };
            await MessageQueue.Instance.ConsumeAsync(consumer2);

            for (int i = 0; i < 1000; ++i)
            {
                await MessageQueue.Instance.PublishAsync("test", i);
            }
```