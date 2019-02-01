using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkColliderIgnore : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this._colliders.Clear();
			this._ignoredColliders.Clear();
			CombatObject targetCombat = base.GetTargetCombat(this.Effect, BasePerk.PerkTarget.Target);
			CombatObject targetCombat2 = base.GetTargetCombat(this.Effect, BasePerk.PerkTarget.Owner);
			if (!targetCombat2)
			{
				return;
			}
			BasePerk.PerkTarget target = this.Target;
			Collider[] componentsInChildren;
			TeamKind team;
			if (target != BasePerk.PerkTarget.Target)
			{
				if (target != BasePerk.PerkTarget.Effect)
				{
					componentsInChildren = targetCombat2.GetComponentsInChildren<Collider>(true);
					team = targetCombat2.Team;
				}
				else
				{
					componentsInChildren = this.Effect.GetComponentsInChildren<Collider>(true);
					team = targetCombat2.Team;
				}
			}
			else
			{
				componentsInChildren = targetCombat.GetComponentsInChildren<Collider>(true);
				team = targetCombat.Team;
			}
			foreach (Collider collider in componentsInChildren)
			{
				if (collider && !collider.isTrigger)
				{
					this._colliders.Add(collider);
				}
			}
			this.GetTargets((!(targetCombat == null)) ? targetCombat.Id.ObjId : -1, targetCombat2.Id.ObjId, team);
			this.RunIgnores(true);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			this.RunIgnores(false);
			this._colliders.Clear();
			this._ignoredColliders.Clear();
		}

		private void GetTargets(int target, int owner, TeamKind team)
		{
			List<PlayerData> list = null;
			switch (this.Mode)
			{
			case PerkColliderIgnore.IgnoreMode.None:
				PerkColliderIgnore.Log.WarnFormat("Mode set to NONE! Effect={0}", new object[]
				{
					base.name
				});
				return;
			case PerkColliderIgnore.IgnoreMode.Enemies:
				if (team != TeamKind.Red)
				{
					if (team != TeamKind.Blue)
					{
						PerkColliderIgnore.Log.WarnFormat("Enemies ignore on a bomb? Effect={0} team={1}", new object[]
						{
							base.name,
							team
						});
						return;
					}
					list = GameHubBehaviour.Hub.Players.RedTeamPlayersAndBots;
				}
				else
				{
					list = GameHubBehaviour.Hub.Players.BlueTeamPlayersAndBots;
				}
				break;
			case PerkColliderIgnore.IgnoreMode.Allies:
				if (team != TeamKind.Red)
				{
					if (team != TeamKind.Blue)
					{
						PerkColliderIgnore.Log.WarnFormat("Allies ignore on a bomb? Effect={0} team={1}", new object[]
						{
							base.name,
							team
						});
						return;
					}
					list = GameHubBehaviour.Hub.Players.BlueTeamPlayersAndBots;
				}
				else
				{
					list = GameHubBehaviour.Hub.Players.RedTeamPlayersAndBots;
				}
				break;
			case PerkColliderIgnore.IgnoreMode.All:
				list = GameHubBehaviour.Hub.Players.PlayersAndBots;
				break;
			}
			if (list == null)
			{
				PerkColliderIgnore.Log.ErrorFormat("Targets went null Effect={2} Mode={0} team={1}", new object[]
				{
					this.Mode,
					team,
					base.name
				});
				return;
			}
			for (int i = 0; i < list.Count; i++)
			{
				PlayerData playerData = list[i];
				if (!this.ExceptOwner || playerData.PlayerCarId != owner)
				{
					if (!this.ExceptTarget || playerData.PlayerCarId != target)
					{
						Identifiable characterInstance = playerData.CharacterInstance;
						foreach (Collider collider in characterInstance.GetComponentsInChildren<Collider>(true))
						{
							if (collider && !collider.isTrigger)
							{
								this._ignoredColliders.Add(collider);
							}
						}
					}
				}
			}
		}

		private void RunIgnores(bool ignore)
		{
			for (int i = 0; i < this._colliders.Count; i++)
			{
				Collider collider = this._colliders[i];
				for (int j = 0; j < this._ignoredColliders.Count; j++)
				{
					Collider collider2 = this._ignoredColliders[j];
					Physics.IgnoreCollision(collider, collider2, ignore);
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkColliderIgnore));

		[Header("Get all colliders of the Target (and its children) and sets them to ignore other colliders as configured in Mode")]
		public PerkColliderIgnore.IgnoreMode Mode;

		public bool ExceptOwner;

		public bool ExceptTarget;

		public BasePerk.PerkTarget Target;

		private List<Collider> _colliders = new List<Collider>();

		private List<Collider> _ignoredColliders = new List<Collider>();

		public enum IgnoreMode
		{
			None,
			Enemies,
			Allies,
			All
		}
	}
}
