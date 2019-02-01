using System;
using HeavyMetalMachines.AnimationHacks;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class HudGadgetObject : GameHubBehaviour
	{
		protected void Start()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.PlayerSpawnManagerOnCurrentPlayerCreated;
		}

		protected virtual void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.PlayerSpawnManagerOnCurrentPlayerCreated;
		}

		private void PlayerSpawnManagerOnCurrentPlayerCreated(PlayerEvent playerEvent)
		{
			CombatObject bitComponent = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
			if (!this.Setup(bitComponent))
			{
				this.AnimationPanelHack.Alpha = 0f;
			}
		}

		protected virtual bool Setup(CombatObject combatObject)
		{
			return true;
		}

		protected void Update()
		{
			this.RenderUpdate();
		}

		protected virtual void RenderUpdate()
		{
		}

		public AlphaAnimationPanelHack AnimationPanelHack;
	}
}
