using System;

namespace HeavyMetalMachines.Combat
{
	public class PerkSetEffectRadius : BasePerk
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this.Effect.SetRadius((!this.UserCustomVar) ? this.Effect.Gadget.Radius : ((float)this.Effect.CustomVar));
		}

		public bool UserCustomVar = true;
	}
}
