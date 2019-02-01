using System;
using ClientAPI.MessageHub;

namespace HeavyMetalMachines.Swordfish
{
	public class GenericPartyMessage : Message
	{
		public GenericPartyMessage() : base("GenericPartyMessage")
		{
		}

		protected override void ReadBody(string body)
		{
			string[] array = body.Split(new char[]
			{
				':'
			});
			this.MessageKind = (PartyMessageEnum)Convert.ToInt32(array[0]);
		}

		protected override string WriteBody()
		{
			int messageKind = (int)this.MessageKind;
			return messageKind.ToString();
		}

		public PartyMessageEnum MessageKind;
	}
}
