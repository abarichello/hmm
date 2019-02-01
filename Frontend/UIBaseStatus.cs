using System;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public abstract class UIBaseStatus : GameHubBehaviour
	{
		public void SetupOnCurrentPlayerBuildComplete(CombatData combatData)
		{
			this._combatData = combatData;
			this.SetupListeners();
		}

		protected void OnDestroy()
		{
			this.RemoveListeners();
		}

		protected abstract void SetupListeners();

		protected abstract void RemoveListeners();

		protected abstract void UpdateInterface();

		public UI2DSprite statusBar;

		protected CombatData _combatData;
	}
}
