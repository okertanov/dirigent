using Dirigent.Common.Core.IoC;
using TinyMessenger;

namespace Dirigent.iOS.Core.IoC {
	internal class MessengerHub : IMessengerHub {
		private readonly ITinyMessengerHub messengerHub;

		public MessengerHub(ITinyMessengerHub messengerHub) {
			this.messengerHub = messengerHub;
		}

		public void Publish<TMessage>(TMessage message) where TMessage : class {
			messengerHub.Publish((ITinyMessage)message);
		}

		public void PublishAsync<TMessage>(TMessage message) where TMessage : class {
			messengerHub.PublishAsync((ITinyMessage)message);
		}
	}
}