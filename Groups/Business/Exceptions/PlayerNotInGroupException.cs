using System;

namespace HeavyMetalMachines.Groups.Business.Exceptions
{
	public class PlayerNotInGroupException : Exception
	{
		public PlayerNotInGroupException() : base("The player is currently not in a group.")
		{
		}
	}
}
