using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;

namespace HeavyMetalMachines.Combat
{
	public class PlayerSpawnManager : BaseSpawnManager
	{
		protected override PlayerCarFactory GetFactory()
		{
			return this.Factory;
		}

		public override bool CarCreationFinished
		{
			get
			{
				return this.FirstSpawnFinished || (this.ObjectList.Count == 0 && SpectatorController.IsSpectating);
			}
		}

		protected override List<PlayerData> ObjectList
		{
			get
			{
				return GameHubBehaviour.Hub.Players.Players;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event Action<PlayerEvent> m_afnPlayerCreatedCallback;

		public event Action<PlayerEvent> CurrentPlayerCreatedCallback
		{
			add
			{
				this.m_afnPlayerCreatedCallback += value;
			}
			remove
			{
				this.m_afnPlayerCreatedCallback -= value;
			}
		}

		protected override void OnPlayerOwnerCreated(PlayerEvent player)
		{
			if (this.m_afnPlayerCreatedCallback != null)
			{
				this.m_afnPlayerCreatedCallback(player);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.m_afnPlayerCreatedCallback = null;
		}

		public override void OnCleanup(CleanupMessage msg)
		{
			base.OnCleanup(msg);
			this.CarCreationFinished = false;
		}

		public PlayerCarFactory Factory;
	}
}
