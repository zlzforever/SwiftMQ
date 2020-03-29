using System;
using Microsoft.Extensions.DependencyInjection;

namespace SwiftMQ.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddSwiftMQ(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IMessageQueue, MessageQueue>();
			return serviceCollection;
		}
	}
}