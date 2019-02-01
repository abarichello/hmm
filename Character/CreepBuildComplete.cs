using System;
using Pocketverse;

namespace HeavyMetalMachines.Character
{
	public class CreepBuildComplete : Mural.IMuralMessage
	{
		public CreepBuildComplete(Identifiable ident)
		{
			this.Ident = ident;
		}

		public string Message
		{
			get
			{
				return "OnCreepBuildComplete";
			}
		}

		public Identifiable Ident;

		public const string Msg = "OnCreepBuildComplete";

		public interface ICreepBuildCompleteListener
		{
			void OnCreepBuildComplete(CreepBuildComplete evt);
		}
	}
}
