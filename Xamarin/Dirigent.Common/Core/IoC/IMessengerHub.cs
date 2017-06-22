using System;

namespace Dirigent.Common.Core.IoC {
	public interface IMessengerHub {
		void Publish<TMessage>(TMessage message) where TMessage : class;
		void PublishAsync<TMessage>(TMessage message) where TMessage : class;
	}
}