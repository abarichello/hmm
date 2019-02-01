using System;
using System.Collections.Generic;
using ClientAPI.MessageHub;

namespace HeavyMetalMachines.Swordfish
{
	public class AddPartyPlayerMessage : Message
	{
		public AddPartyPlayerMessage() : base("AddPartyPlayerMessage")
		{
		}

		protected override void ReadBody(string body)
		{
			string[] array = body.Split(new char[]
			{
				':'
			});
			int num = 0;
			while (num + 1 < array.Length)
			{
				this.PlayersList.Add(new AddPartyPlayerMessage.PlayerPartyStatus
				{
					PlayerId = Convert.ToInt64(array[num]),
					PlayerStatus = (PartyStatusEnum)Convert.ToInt32(array[num + 1])
				});
				num += 2;
			}
		}

		protected override string WriteBody()
		{
			string text = string.Empty;
			for (int i = 0; i < this.PlayersList.Count; i++)
			{
				string text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					this.PlayersList[i].PlayerId,
					":",
					(int)this.PlayersList[i].PlayerStatus,
					":"
				});
			}
			return text;
		}

		public List<AddPartyPlayerMessage.PlayerPartyStatus> PlayersList = new List<AddPartyPlayerMessage.PlayerPartyStatus>();

		public class PlayerPartyStatus
		{
			public long PlayerId;

			public PartyStatusEnum PlayerStatus;
		}
	}
}
