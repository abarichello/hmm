using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class UseBombGadgetTutorialBehavior : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._combats.Clear();
			EventScopeKind targetType = this._targetType;
			if (targetType != EventScopeKind.Player)
			{
				if (targetType != EventScopeKind.Bot)
				{
					throw new Exception("Invalid TargetType!!!");
				}
				this.ExtractCombatObjects(GameHubBehaviour.Hub.Players.Bots);
			}
			else
			{
				this.ExtractCombatObjects(GameHubBehaviour.Hub.Players.Players);
			}
			this.CompleteBehaviourAndSync();
		}

		protected override void UpdateOnServer()
		{
			for (int i = 0; i < this._combats.Count; i++)
			{
				CombatObject combatObject = this._combats[i];
				combatObject.BombGadget.Pressed = !GameHubBehaviour.Hub.BombManager.IsCarryingBomb(combatObject.Id.ObjId);
			}
		}

		private void ExtractCombatObjects(List<PlayerData> playerDatas)
		{
			for (int i = 0; i < playerDatas.Count; i++)
			{
				PlayerData playerData = playerDatas[i];
				if (this._teamKind == playerData.Team)
				{
					CombatObject component = playerData.CharacterInstance.GetComponent<CombatObject>();
					if (null != component)
					{
						this._combats.Add(component);
					}
				}
			}
		}

		[SerializeField]
		private EventScopeKind _targetType;

		[SerializeField]
		private TeamKind _teamKind;

		private readonly List<CombatObject> _combats = new List<CombatObject>();
	}
}
