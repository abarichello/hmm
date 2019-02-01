using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.GadgetScript;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "GadgetScript/CombatGadget")]
	public class CombatGadget : BaseGadget
	{
		public GadgetSlot Slot
		{
			get
			{
				return this._slot;
			}
		}

		public GadgetNatureKind Nature
		{
			get
			{
				return this._nature;
			}
		}

		public override IHMMGadgetContext CreateGadgetContext(int id, Identifiable owner, GadgetEventParser eventParser, IHMMContext context)
		{
			CombatGadget combatGadget = (CombatGadget)base.CreateGadgetContext(id, owner, eventParser, context);
			combatGadget._combat = (CombatObject)combatGadget.Owner;
			combatGadget._events = this._events;
			if (this._ownerParameter != null)
			{
				this._ownerParameter.SetValue(combatGadget, combatGadget._combat);
			}
			if (context.IsServer)
			{
				combatGadget.SubscribeEvents(this._events);
			}
			combatGadget.InitializeModifiableParameters();
			combatGadget.InitializeCooldownParameters();
			return combatGadget;
		}

		private void InitializeCooldownParameters()
		{
			this._cooldownEndTime = this.GetUIParameter<int>("CooldownEndTime");
			this._cooldownTotalTime = this.GetUIParameter<float>("CooldownDuration");
		}

		public bool HasCooldownParameters()
		{
			return this._cooldownTotalTime != null && this._cooldownEndTime != null;
		}

		public int GetCooldownEndTime()
		{
			return (this._cooldownEndTime != null) ? this._cooldownEndTime.GetValue(this) : 0;
		}

		public float GetCooldownTotalTime()
		{
			return (this._cooldownTotalTime != null) ? this._cooldownTotalTime.GetValue(this) : 0f;
		}

		public INumericParameter GetModifiableParameter(string parameterName)
		{
			return this._modifiableParametersDict[parameterName];
		}

		public bool Pressed
		{
			get
			{
				return this._pressed;
			}
			set
			{
				if (value && !this.CheckGadgetCanBePressed())
				{
					return;
				}
				this._pressed = value;
				if (this._pressed && this._events.OnPressedBlock != null)
				{
					base.TriggerEvent(this._events.OnPressedBlock.Id);
				}
				else if (!this._pressed && this._events.OnReleasedBlock != null)
				{
					base.TriggerEvent(this._events.OnReleasedBlock.Id);
				}
			}
		}

		public void RouteParametersGadgets()
		{
			for (int i = 0; i < this._parametersFromOtherGadgets.Count; i++)
			{
				IHMMGadgetContext gadgetContext = this._combat.GetGadgetContext((int)this._parametersFromOtherGadgets[i].Slot);
				this._parametersFromOtherGadgets[i].Parameter.RouteContext(this, gadgetContext);
			}
		}

		private void SubscribeEvents(CombatGadget.GadgetEventConfig events)
		{
			GameHubScriptableObject.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			if (events.OnCombatDeathBlock != null)
			{
				GameHubScriptableObject.Hub.Events.Bots.ListenToObjectDeath += this.OnCombatDeath;
				GameHubScriptableObject.Hub.Events.Players.ListenToObjectDeath += this.OnCombatDeath;
			}
			if (events.OnCombatSpawnBlock != null)
			{
				GameHubScriptableObject.Hub.Events.Bots.ListenToObjectSpawn += this.OnCombatSpawn;
				GameHubScriptableObject.Hub.Events.Players.ListenToObjectSpawn += this.OnCombatSpawn;
			}
			if (events.OnModifierAppliedBlock != null)
			{
				if (this._onModifierAppliedEventParameters.Modifier != null)
				{
					this._onModifierAppliedEventParameters.HasModifierParameter = true;
				}
				if (this._onModifierAppliedEventParameters.Target != null)
				{
					this._onModifierAppliedEventParameters.HasTargetParameter = true;
				}
				if (this._onModifierAppliedEventParameters.Amount != null)
				{
					this._onModifierAppliedEventParameters.HasAmountParameter = true;
				}
				if (this._onModifierAppliedEventParameters.Causer != null)
				{
					this._onModifierAppliedEventParameters.HasCauserParameter = true;
				}
				CombatController.OnInstantModifierApplied += this.OnModifierApplied;
			}
		}

		private void UnsubscribeEvents(CombatGadget.GadgetEventConfig events)
		{
			GameHubScriptableObject.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			if (events.OnCombatDeathBlock != null)
			{
				GameHubScriptableObject.Hub.Events.Bots.ListenToObjectDeath -= this.OnCombatDeath;
				GameHubScriptableObject.Hub.Events.Players.ListenToObjectDeath -= this.OnCombatDeath;
			}
			if (events.OnCombatSpawnBlock != null)
			{
				GameHubScriptableObject.Hub.Events.Bots.ListenToObjectSpawn -= this.OnCombatSpawn;
				GameHubScriptableObject.Hub.Events.Players.ListenToObjectSpawn -= this.OnCombatSpawn;
			}
			if (events.OnModifierAppliedBlock != null)
			{
				CombatController.OnInstantModifierApplied -= this.OnModifierApplied;
			}
		}

		private void OnModifierApplied(ModifierInstance mod, CombatObject causer, CombatObject target, float amount, int eventId)
		{
			if (this._modiferFilter.FilterModifierApplication(mod, causer, target, this._combat))
			{
				return;
			}
			CombatGadget._eventsParameters.Clear();
			if (this._onModifierAppliedEventParameters.HasModifierParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierAppliedEventParameters.Modifier);
				this._onModifierAppliedEventParameters.Modifier.SetValue(this, mod.Data);
			}
			if (this._onModifierAppliedEventParameters.HasTargetParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierAppliedEventParameters.Target);
				this._onModifierAppliedEventParameters.Target.SetValue(this, target);
			}
			if (this._onModifierAppliedEventParameters.HasAmountParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierAppliedEventParameters.Amount);
				this._onModifierAppliedEventParameters.Amount.SetValue(this, amount);
			}
			if (this._onModifierAppliedEventParameters.HasCauserParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierAppliedEventParameters.Causer);
				this._onModifierAppliedEventParameters.Causer.SetValue(this, causer);
			}
			base.TriggerEvent(new GadgetEvent(this._events.OnModifierAppliedBlock.Id, this, CombatGadget._eventsParameters));
		}

		private void OnCombatDeath(PlayerEvent data)
		{
			CombatGadget._eventsParameters.Clear();
			CombatGadget._eventsParameters.Add(this._onPlayerStatusParameters.Combat);
			this._onPlayerStatusParameters.Combat.SetValue(this, (CombatObject)base.GetCombatObject(data.TargetId));
			base.TriggerEvent(new GadgetEvent(this._events.OnCombatDeathBlock.Id, this, CombatGadget._eventsParameters));
		}

		private void OnCombatSpawn(PlayerEvent data)
		{
			CombatGadget._eventsParameters.Clear();
			CombatGadget._eventsParameters.Add(this._onPlayerStatusParameters.Combat);
			this._onPlayerStatusParameters.Combat.SetValue(this, (CombatObject)base.GetCombatObject(data.TargetId));
			base.TriggerEvent(new GadgetEvent(this._events.OnCombatSpawnBlock.Id, this, CombatGadget._eventsParameters));
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			if (state == BombScoreBoard.State.BombDelivery && this._events.OnRoundStartBlock != null)
			{
				base.TriggerEvent(this._events.OnRoundStartBlock.Id);
			}
			if (state == BombScoreBoard.State.Replay && this._events.OnRoundEndBlock != null)
			{
				base.TriggerEvent(this._events.OnRoundEndBlock.Id);
			}
			if (state == BombScoreBoard.State.EndGame)
			{
				this.UnsubscribeEvents(this._events);
			}
		}

		private void InitializeModifiableParameters()
		{
			this._modifiableParametersDict = new Dictionary<string, INumericParameter>();
			for (int i = 0; i < this._modifiableParameters.Count; i++)
			{
				if (this._modifiableParameters[i] != null && !(this._modifiableParameters[i].Parameter == null) && !string.IsNullOrEmpty(this._modifiableParameters[i].Name))
				{
					INumericParameter numericParameter = this._modifiableParameters[i].Parameter as INumericParameter;
					if (numericParameter == null)
					{
						Debug.LogErrorFormat("Modifiable parameter {0} is not Numeric!", new object[]
						{
							this._modifiableParameters[i].Name
						});
					}
					else
					{
						this._modifiableParametersDict.Add(this._modifiableParameters[i].Name, numericParameter);
					}
				}
			}
		}

		public override void SetLifebarVisibility(int combatObjectId, bool visible)
		{
			base.SetLifebarVisibility(combatObjectId, visible);
			GameGui stateGuiController = GameHubScriptableObject.Hub.State.Current.GetStateGuiController<GameGui>();
			stateGuiController.HudLifebarController.SetCurrentPlayerVisibility(combatObjectId, visible);
			if (visible)
			{
				this.TrySetAlliesAttachedLifebarGroundVisibility(combatObjectId);
			}
		}

		private void TrySetAlliesAttachedLifebarGroundVisibility(int combatObjectId)
		{
			TeamKind teamKind = TeamKind.Neutral;
			for (int i = 0; i < GameHubScriptableObject.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubScriptableObject.Hub.Players.Players[i];
				if (playerData.PlayerCarId == combatObjectId)
				{
					teamKind = playerData.Team;
					break;
				}
			}
			if (teamKind == TeamKind.Neutral)
			{
				return;
			}
			List<PlayerData> list = (teamKind != TeamKind.Blue) ? GameHubScriptableObject.Hub.Players.RedTeamPlayersAndBots : GameHubScriptableObject.Hub.Players.BlueTeamPlayersAndBots;
			for (int j = 0; j < list.Count; j++)
			{
				this.SetAttachedLifebarGroupVisibility(list[j].PlayerCarId, combatObjectId, false);
			}
		}

		public override void SetAttachedLifebarGroupVisibility(int lifebarOwnerId, int attachedId, bool visible)
		{
			base.SetAttachedLifebarGroupVisibility(lifebarOwnerId, attachedId, visible);
			GameGui stateGuiController = GameHubScriptableObject.Hub.State.Current.GetStateGuiController<GameGui>();
			stateGuiController.HudLifebarController.SetAttachedLifebarVisibility(lifebarOwnerId, attachedId, visible);
		}

		public override List<BaseParameter> GetAllUIParameters()
		{
			List<BaseParameter> list = new List<BaseParameter>();
			for (int i = 0; i < this._UIParameters.Count; i++)
			{
				list.Add(this._UIParameters[i].Parameter);
			}
			return list;
		}

		public override IParameter<T> GetUIParameter<T>(string param)
		{
			for (int i = 0; i < this._UIParameters.Count; i++)
			{
				CombatGadget.NamedParameter namedParameter = this._UIParameters[i];
				if (string.Compare(param, namedParameter.Name, StringComparison.InvariantCulture) == 0)
				{
					return namedParameter.Parameter as IParameter<T>;
				}
			}
			return null;
		}

		public bool CheckGadgetCanBePressed()
		{
			if (this.GetCooldownEndTime() > GameHubScriptableObject.Hub.Clock.GetPlaybackTime())
			{
				return false;
			}
			if (!this._canBeUsedWhileDead && !this._combat.Data.IsAlive())
			{
				return false;
			}
			BombScoreBoard.State roundState = (BombScoreBoard.State)this._hmmContext.ScoreBoard.RoundState;
			bool flag = roundState == BombScoreBoard.State.BombDelivery;
			return this._canBeUsedBeforeRoundStart || flag;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CombatGadget));

		[SerializeField]
		private GadgetSlot _slot;

		[FlagDrawer(typeof(GadgetNatureKind))]
		[SerializeField]
		private GadgetNatureKind _nature;

		[SerializeField]
		private CombatObjectParameter _ownerParameter;

		[SerializeField]
		private bool _canBeUsedBeforeRoundStart;

		[SerializeField]
		private bool _canBeUsedWhileDead;

		[SerializeField]
		private CombatGadget.GadgetEventConfig _events;

		[SerializeField]
		private CombatGadget.OnPlayerStatusParameters _onPlayerStatusParameters;

		[SerializeField]
		private CombatGadget.OnModifierAppliedParameters _onModifierAppliedEventParameters;

		[SerializeField]
		private CombatGadget.ModifierAppliedFilter _modiferFilter;

		[SerializeField]
		private List<CombatGadget.NamedParameter> _modifiableParameters;

		[SerializeField]
		private List<CombatGadget.ParameterGadgetContextRoute> _parametersFromOtherGadgets;

		[SerializeField]
		private List<CombatGadget.NamedParameter> _UIParameters;

		private IParameter<int> _cooldownEndTime;

		private IParameter<float> _cooldownTotalTime;

		private CombatObject _combat;

		private bool _pressed;

		private Dictionary<string, INumericParameter> _modifiableParametersDict;

		private static List<BaseParameter> _eventsParameters = new List<BaseParameter>(8);

		[Serializable]
		private class ParameterGadgetContextRoute
		{
			public GadgetSlot Slot;

			public BaseParameter Parameter;
		}

		[Serializable]
		public class GadgetEventConfig
		{
			public BaseBlock OnPressedBlock;

			public BaseBlock OnReleasedBlock;

			public BaseBlock OnCombatDeathBlock;

			public BaseBlock OnCombatSpawnBlock;

			public BaseBlock OnRoundStartBlock;

			public BaseBlock OnRoundEndBlock;

			public BaseBlock OnModifierAppliedBlock;
		}

		[Serializable]
		private class OnPlayerStatusParameters
		{
			public CombatObjectParameter Combat;
		}

		[Serializable]
		private class OnModifierAppliedParameters
		{
			public ModifierDataParameter Modifier;

			public CombatObjectParameter Causer;

			public CombatObjectParameter Target;

			public FloatParameter Amount;

			[HideInInspector]
			public bool HasModifierParameter;

			[HideInInspector]
			public bool HasCauserParameter;

			[HideInInspector]
			public bool HasTargetParameter;

			[HideInInspector]
			public bool HasAmountParameter;
		}

		[Serializable]
		private class NamedParameter
		{
			public string Name;

			public BaseParameter Parameter;
		}

		[Serializable]
		private class ModifierAppliedFilter
		{
			public override string ToString()
			{
				return string.Format("T=[{0}] C=[{1}] Eff=[{2}]", this.Target, this.Causer, (!this.AnyEffect) ? Arrays.ToStringWithComma(this.ValidEffects) : "Any");
			}

			public bool FilterModifierApplication(ModifierInstance mod, CombatObject causer, CombatObject target, CombatObject self)
			{
				return this.FilterCombat(causer, self, this.Causer) || this.FilterCombat(target, self, this.Target) || this.FilterModifier(mod);
			}

			private bool FilterCombat(CombatObject combat, CombatObject self, CombatGadget.ModifierAppliedFilter.Filter filter)
			{
				if (combat == null)
				{
					return !filter.Scenery;
				}
				if (combat == self)
				{
					return !filter.Self;
				}
				TeamKind team = self.Team;
				TeamKind team2 = combat.Team;
				if (team2 == team)
				{
					return !filter.Allies;
				}
				return team2 != team && !filter.Enemies;
			}

			private bool FilterModifier(ModifierInstance mod)
			{
				if (this.AnyEffect)
				{
					return false;
				}
				EffectKind effect = mod.Info.Effect;
				for (int i = 0; i < this.ValidEffects.Length; i++)
				{
					EffectKind effectKind = this.ValidEffects[i];
					if (effect == effectKind)
					{
						return false;
					}
				}
				return true;
			}

			public CombatGadget.ModifierAppliedFilter.Filter Target;

			public CombatGadget.ModifierAppliedFilter.Filter Causer;

			public bool AnyEffect;

			public EffectKind[] ValidEffects;

			[Serializable]
			public class Filter
			{
				public override string ToString()
				{
					return string.Format("S={0} A={1} E={2} H={3}", new object[]
					{
						this.Self,
						this.Allies,
						this.Enemies,
						this.Scenery
					});
				}

				public bool Self;

				public bool Allies;

				public bool Enemies;

				public bool Scenery;
			}
		}
	}
}
