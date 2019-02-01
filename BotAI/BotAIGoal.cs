﻿using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	public class BotAIGoal : GameHubScriptableObject
	{
		public bool IsOnBotOnlyTeam
		{
			get
			{
				return this._isOnBotOnlyTeam;
			}
			set
			{
				this._isOnBotOnlyTeam = value;
			}
		}

		public BotAIGoal Parent
		{
			get
			{
				if (!this._isOnBotOnlyTeam)
				{
					return null;
				}
				if (GameHubScriptableObject.Hub.BotAIMatchRules.BotOnlyTeamCap.shouldRespectMatch)
				{
					return GameHubScriptableObject.Hub.BotAIMatchRules.GetMatchAIGoal();
				}
				return GameHubScriptableObject.Hub.BotAIMatchRules.BotOnlyTeamCap;
			}
		}

		public float Focus
		{
			get
			{
				return (!this._isOnBotOnlyTeam) ? this.focus : (this.Parent.Focus * this.focus);
			}
		}

		public float DecisionInterval
		{
			get
			{
				return (!this._isOnBotOnlyTeam) ? this.decisionInterval : (this.Parent.DecisionInterval * this.decisionInterval);
			}
		}

		public float Healing
		{
			get
			{
				return (!this._isOnBotOnlyTeam) ? this.healing : (this.Parent.Healing * this.healing);
			}
		}

		public float Aggressiveness
		{
			get
			{
				return (!this._isOnBotOnlyTeam) ? this.aggressiveness : (this.Parent.Aggressiveness * this.aggressiveness);
			}
		}

		public float DropBomb
		{
			get
			{
				return (!this._isOnBotOnlyTeam) ? this.dropBomb : (this.Parent.DropBomb * this.dropBomb);
			}
		}

		public float Ambush
		{
			get
			{
				return (!this._isOnBotOnlyTeam) ? this.ambush : (this.Parent.Ambush * this.ambush);
			}
		}

		public float Boost
		{
			get
			{
				return (!this._isOnBotOnlyTeam) ? this.boost : (this.Parent.Boost * this.boost);
			}
		}

		private void Awake()
		{
			this.BombGadget.AttackKind = BotAIGoal.GadgetUseInfo.GadgetAttackKind.Bomb;
		}

		private bool _isOnBotOnlyTeam;

		public bool shouldRespectMatch;

		public BotAIGoal.GadgetUseInfo Gadget0;

		public BotAIGoal.GadgetUseInfo Gadget1;

		public BotAIGoal.GadgetUseInfo Gadget2;

		public BotAIGoal.GadgetUseInfo BoostGadget;

		public BotAIGoal.GadgetUseInfo BombGadget;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Chance of loose focus on carrier and focus on closer player por ex. also change the pursuit range!")]
		private float focus = 1f;

		[Range(0.2f, 2f)]
		[SerializeField]
		[Tooltip("Interval that the bot will think about what to do [vary between (0.5s..4s and 1s..8s)] (CheckSubState and ProcessBehaviour)")]
		private float decisionInterval = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Chance of focus in healing")]
		private float healing = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Chance of focus in attacking")]
		private float aggressiveness = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Life percent to decide to be considered low health (drop bomb if transporter or stop supporting if supporter) TODO Rename this to mean lowHealth")]
		private float dropBomb = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Change of try to ambush player")]
		private float ambush = 1f;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Change of use boost")]
		private float boost = 1f;

		public enum BotDifficulty
		{
			Invalid,
			Easy,
			Medium,
			Hard
		}

		[Serializable]
		public class GadgetUseInfo
		{
			public float GetDotLimit
			{
				get
				{
					float? attackAngleCos = this._attackAngleCos;
					return ((attackAngleCos == null) ? (this._attackAngleCos = new float?(Mathf.Cos(this.AttackAngle * 0.0174532924f))) : attackAngleCos).Value;
				}
			}

			[BitMask(typeof(BotAIGoal.GadgetUseInfo.GadgetAttackPattern))]
			public BotAIGoal.GadgetUseInfo.GadgetAttackPattern AttackPattern;

			public BotAIGoal.GadgetUseInfo.GadgetAttackKind AttackKind;

			public bool CrossesWalls;

			public bool CheckRange = true;

			public bool CheckRadius;

			[Range(2.5f, 85f)]
			public float AttackAngle = 85f;

			[NonSerialized]
			private float? _attackAngleCos;

			[Tooltip("Works only with forward, attackspeed must be greater than 0")]
			public bool PredictMovement;

			[Tooltip("For bombGadget this is the mininum time holding before shooting the bomb")]
			public float AttackSpeed;

			[Tooltip("Delay between checking if a target is on sight and actually using this gadget on him")]
			public float ReactionTimeMin;

			[Tooltip("Delay between checking if a target is on sight and actually using this gadget on him")]
			public float ReactionTimeMax;

			[Flags]
			public enum GadgetAttackPattern
			{
				Area = 2,
				Forward = 4,
				Back = 8,
				Droppings = 16
			}

			[Flags]
			public enum GadgetAttackKind
			{
				Bomb = 1,
				Damage = 2,
				Heal = 4,
				Boost = 8,
				DamageAndHeal = 6
			}

			public enum GadgetPenalties
			{
				Impulse,
				Hook
			}
		}
	}
}
