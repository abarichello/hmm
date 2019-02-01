using System;
using ClientAPI.MessageHub;

namespace HeavyMetalMachines.Swordfish
{
	public class RemoveFromPartyMessage : Message
	{
		public RemoveFromPartyMessage() : base("RemoveFromPartyMessage")
		{
		}

		protected override void ReadBody(string body)
		{
			this.PlayerId = Convert.ToInt64(body);
		}

		protected override string WriteBody()
		{
			return this.PlayerId.ToString();
		}

		public long PlayerId;
	}
}
