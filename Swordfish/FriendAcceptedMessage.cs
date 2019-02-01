using System;
using ClientAPI.MessageHub;

namespace HeavyMetalMachines.Swordfish
{
	public class FriendAcceptedMessage : Message
	{
		public FriendAcceptedMessage() : base("FriendAcceptedMessage")
		{
		}

		protected override void ReadBody(string body)
		{
		}

		protected override string WriteBody()
		{
			return string.Empty;
		}
	}
}
