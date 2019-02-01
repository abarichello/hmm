using System;

namespace HeavyMetalMachines.Combat
{
	public class PerkStickToGround : BasePerk
	{
		public override void PerkInitialized()
		{
			this.Effect.Data.Origin.y = 0f;
			base.transform.position = this.Effect.Data.Origin;
		}
	}
}
