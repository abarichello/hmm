using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CreepInfo : GameHubScriptableObject, IContent
	{
		public int ContentId
		{
			get
			{
				return this.CreepInfoId;
			}
			set
			{
				this.CreepInfoId = value;
			}
		}

		public CombatInfo GetCombatInfo()
		{
			return (!(this.BotCharacterInfo == null)) ? this.BotCharacterInfo.Combat : this.Combat;
		}

		private void OnEnable()
		{
			CreepInfo.Creeps[this.CreepInfoId] = this;
		}

		private void OnDisable()
		{
			if (!Application.isEditor)
			{
				CreepInfo.Creeps.Remove(this.CreepInfoId);
			}
		}

		public static readonly Dictionary<int, CreepInfo> Creeps = new Dictionary<int, CreepInfo>();

		[ScriptId]
		public int CreepInfoId;

		public string Name;

		public string Description;

		public string Asset;

		public CreepInfo.AssetByRound[] AssetsByRound;

		public string AssetBlu;

		public string AssetRed;

		public string[] RandomAssets;

		public float AggroRange;

		public float PlayerAggroRange;

		public float CreepAggroRange;

		public float WardAggroRange;

		public float AvengerAggroRange;

		public float WaypointRangePow2 = 81f;

		public float AttackRangePow2 = 81f;

		public bool LockToAnyTarget;

		public CreepTemplateKind TemplateKind;

		public CreepAggroAIKind AIType;

		public CreepRespawnKind RespawnKind;

		public CombatInfo Combat;

		public GadgetInfo Attack;

		public GadgetInfo AttackBlu;

		public GadgetInfo AttackRed;

		public float TurretDamageMultiplier = 1f;

		public float MaxSpeed;

		public float TurnSpeed = 120f;

		public float CollisionRadius;

		public float NavigationRadius;

		public int AvoidancePriority = 50;

		public int RewardAmount;

		public string RewardPickup;

		public int DropIndex = -1;

		public HeavyMetalMachines.Character.CharacterInfo BotCharacterInfo;

		public int TrailerCount;

		public bool BotUseGadgets;

		[Serializable]
		public struct AssetByRound
		{
			public int[] Rounds;

			public string Asset;
		}
	}
}
