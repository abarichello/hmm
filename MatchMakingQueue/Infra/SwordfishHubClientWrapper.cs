using System;
using ClientAPI;
using ClientAPI.MessageHub;
using Pocketverse;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class SwordfishHubClientWrapper : ISwordfishHubClientWrapper
	{
		public SwordfishHubClientWrapper(AbstractHubClient abstractHubClient)
		{
			this._abstractHubClient = abstractHubClient;
			this._abstractHubClient.ConnectionInstability += new EventHandlerEx<ConnectionInstabilityMessage>(this.AbstractHubClientOnConnectionInstability);
		}

		public event Action<ConnectionInstabilityMessage> OnConnectionInstability
		{
			add
			{
				this._onConnectionInstability = (Action<ConnectionInstabilityMessage>)Delegate.Combine(this._onConnectionInstability, value);
			}
			remove
			{
				this._onConnectionInstability = (Action<ConnectionInstabilityMessage>)Delegate.Remove(this._onConnectionInstability, value);
			}
		}

		~SwordfishHubClientWrapper()
		{
			if (this._abstractHubClient != null)
			{
				this._abstractHubClient.ConnectionInstability -= new EventHandlerEx<ConnectionInstabilityMessage>(this.AbstractHubClientOnConnectionInstability);
			}
		}

		private void AbstractHubClientOnConnectionInstability(object sender, ConnectionInstabilityMessage eventArgs)
		{
			SwordfishHubClientWrapper._log.InfoFormat("Connection Instability sender={0}, eventArgs={1}", new object[]
			{
				sender,
				eventArgs
			});
			if (this._onConnectionInstability != null)
			{
				this._onConnectionInstability(eventArgs);
			}
		}

		private static readonly BitLogger _log = new BitLogger(typeof(SwordfishHubClientWrapper));

		private readonly AbstractHubClient _abstractHubClient;

		private Action<ConnectionInstabilityMessage> _onConnectionInstability;
	}
}
