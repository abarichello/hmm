using System;
using HeavyMetalMachines.DataTransferObjects.Player;

namespace HeavyMetalMachines.Players.Business
{
	public class Player : IPlayer, IIdentifiable
	{
		public Guid Id { get; set; }

		public long PlayerId { get; set; }

		public string UniversalId { get; set; }

		public string Nickname { get; set; }

		public long? PlayerTag { get; set; }

		public string Email { get; set; }

		public PlayerBag Bag { get; set; }
	}
}
