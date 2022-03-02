using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.VFX;
using Hoplon.DependencyInjection;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "GadgetScript/CombatGadget")]
	public class CombatGadget : BaseGadget, IHMMCombatGadgetContext, CombatLayer.ILayerChanger, IHMMGadgetContext, IGadgetContext, IGadgetInput
	{
		public IGadgetHudElement GadgetHudElement { get; private set; }

		public IHudEmotePresenter HudEmoteView
		{
			get
			{
				return this._hmmContext.GetHudEmote(this._combat);
			}
		}

		public IGadgetHudElement GetGadgetHudElement(GadgetSlot slot)
		{
			return this._hmmContext.GadgetHud.GetElement(slot);
		}

		public IHudIconBar GetHudIconBar(ICombatObject combat)
		{
			return this._hmmContext.GetHudIconBar(combat);
		}

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

		public CombatGadget.GadgetEventConfig Events
		{
			get
			{
				return this._events;
			}
		}

		private void OnEnable()
		{
			if (null == this._originalScriptable)
			{
				this.PopulateSubgadgetEvents();
			}
		}

		public override void PrecacheAssets(IHMMContext context)
		{
			base.PrecacheAssets(context);
			for (int i = 0; i < this._precacheObjects.Count; i++)
			{
				MasterVFX masterVFX = this._precacheObjects[i];
				ResourceLoader.Instance.PreCache(masterVFX.name, null, 1);
			}
		}

		public override IHMMGadgetContext CreateGadgetContext(int id, IGadgetOwner owner, IGadgetEventDispatcher eventParser, IHMMContext context, IServerPlaybackDispatcher dispatcher, IInjectionResolver injectionResolver)
		{
			CombatGadget combatGadget = (CombatGadget)base.CreateGadgetContext(id, owner, eventParser, context, dispatcher, injectionResolver);
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
			else if (combatGadget._combat.IsLocalPlayer)
			{
				combatGadget.GadgetHudElement = context.GadgetHud.GetElement(this._slot);
			}
			combatGadget._subGadgetEvents = this._subGadgetEvents;
			combatGadget.InitializeCooldownParameters();
			return combatGadget;
		}

		private void InitializeCooldownParameters()
		{
			this._cooldownEndTime = this.GetUIParameter<float>("CooldownEndTime");
			this._cooldownTotalTime = this.GetUIParameter<float>("CooldownDuration");
		}

		public bool HasCooldownParameters()
		{
			return this._cooldownTotalTime != null && this._cooldownEndTime != null;
		}

		public int GetCooldownEndTime()
		{
			return (this._cooldownEndTime != null) ? ((int)((BaseParameter)this._cooldownEndTime).GetValue<float>(this)) : 0;
		}

		public float GetCooldownTotalTime()
		{
			return (this._cooldownTotalTime != null) ? this._cooldownTotalTime.GetValue(this) : 0f;
		}

		public bool Pressed
		{
			get
			{
				return this._pressed;
			}
			set
			{
				bool pressed = this._pressed;
				this._pressed = value;
				if (value && !pressed)
				{
					this.TriggerPressed();
				}
				else if (!value && pressed)
				{
					this.TriggerReleased();
				}
			}
		}

		private void TriggerPressed()
		{
			if (this._events.OnPressedBlock != null)
			{
				base.TriggerEvent(this._events.OnPressedBlock.Id);
			}
		}

		private void TriggerReleased()
		{
			if (this._events.OnReleasedBlock != null)
			{
				base.TriggerEvent(this._events.OnReleasedBlock.Id);
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
			this._onPlayerStatusParameters.Initialize();
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
			this._onModifierEventParameters.Initialize();
			if (events.OnModifierAppliedBlock != null)
			{
				CombatController.OnInstantModifierApplied += new CombatController.OnInstantModifierAppliedDelegate(this.OnModifierApplied);
				CombatController.OnStatusModifierApplied += this.OnStatusApplied;
			}
			if (events.OnModifierRemovedBlock != null)
			{
				CombatController.OnModifierRemoved += this.OnModifierRemoved;
			}
		}

		private void UnsubscribeEvents(CombatGadget.GadgetEventConfig events)
		{
			GameHubScriptableObject.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			GameHubScriptableObject.Hub.Events.Bots.ListenToObjectDeath -= this.OnCombatDeath;
			GameHubScriptableObject.Hub.Events.Players.ListenToObjectDeath -= this.OnCombatDeath;
			GameHubScriptableObject.Hub.Events.Bots.ListenToObjectSpawn -= this.OnCombatSpawn;
			GameHubScriptableObject.Hub.Events.Players.ListenToObjectSpawn -= this.OnCombatSpawn;
			CombatController.OnInstantModifierApplied -= new CombatController.OnInstantModifierAppliedDelegate(this.OnModifierApplied);
			CombatController.OnStatusModifierApplied -= this.OnStatusApplied;
			CombatController.OnModifierRemoved -= this.OnModifierRemoved;
		}

		private void OnModifierRemoved(ModifierInstance modInstance, ICombatObject causer, ICombatObject target, int eventId)
		{
			CombatGadget._eventsParameters.Clear();
			if (this._onModifierEventParameters.HasModifierParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierEventParameters.Modifier);
				this._onModifierEventParameters.Modifier.SetValue(this, modInstance.Data);
			}
			if (this._onModifierEventParameters.HasTargetParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierEventParameters.Target);
				this._onModifierEventParameters.Target.SetValue(this, target);
			}
			if (this._onModifierEventParameters.HasCauserParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierEventParameters.Causer);
				this._onModifierEventParameters.Causer.SetValue(this, causer);
			}
			base.TriggerEvent(GadgetEvent.GetInstance(this._events.OnModifierRemovedBlock.Id, this, CombatGadget._eventsParameters));
		}

		private void OnStatusApplied(ModifierInstance mod, ICombatObject causer, ICombatObject target, int eventId)
		{
			this.OnModifierApplied(mod, causer, target, 0f, eventId);
		}

		private void OnModifierApplied(ModifierInstance mod, ICombatObject causer, ICombatObject target, float amount, int eventId)
		{
			CombatGadget._eventsParameters.Clear();
			if (this._onModifierEventParameters.HasModifierParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierEventParameters.Modifier);
				this._onModifierEventParameters.Modifier.SetValue(this, mod.Data);
			}
			if (this._onModifierEventParameters.HasTargetParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierEventParameters.Target);
				this._onModifierEventParameters.Target.SetValue(this, target);
			}
			if (this._onModifierEventParameters.HasAmountParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierEventParameters.Amount);
				IParameterTomate<float> parameterTomate = this._onModifierEventParameters.Amount.ParameterTomate as IParameterTomate<float>;
				parameterTomate.SetValue(this, amount);
			}
			if (this._onModifierEventParameters.HasCauserParameter)
			{
				CombatGadget._eventsParameters.Add(this._onModifierEventParameters.Causer);
				this._onModifierEventParameters.Causer.SetValue(this, causer);
			}
			base.TriggerEvent(GadgetEvent.GetInstance(this._events.OnModifierAppliedBlock.Id, this, CombatGadget._eventsParameters));
		}

		private void OnCombatDeath(PlayerEvent data)
		{
			if (data.Reason != SpawnReason.Death)
			{
				return;
			}
			CombatGadget._eventsParameters.Clear();
			if (this._onPlayerStatusParameters.HasCombatParameter)
			{
				CombatGadget._eventsParameters.Add(this._onPlayerStatusParameters.Combat);
				this._onPlayerStatusParameters.Combat.SetValue(this, (CombatObject)base.GetCombatObject(data.TargetId));
			}
			if (this._onPlayerStatusParameters.HasCauserParameter)
			{
				CombatGadget._eventsParameters.Add(this._onPlayerStatusParameters.Causer);
				this._onPlayerStatusParameters.Causer.SetValue(this, (CombatObject)base.GetCombatObject(data.CauserId));
			}
			base.TriggerEvent(GadgetEvent.GetInstance(this._events.OnCombatDeathBlock.Id, this, CombatGadget._eventsParameters));
		}

		private void OnCombatSpawn(PlayerEvent data)
		{
			CombatGadget._eventsParameters.Clear();
			if (this._onPlayerStatusParameters.HasCombatParameter)
			{
				CombatGadget._eventsParameters.Add(this._onPlayerStatusParameters.Combat);
				this._onPlayerStatusParameters.Combat.SetValue(this, (CombatObject)base.GetCombatObject(data.TargetId));
			}
			base.TriggerEvent(GadgetEvent.GetInstance(this._events.OnCombatSpawnBlock.Id, this, CombatGadget._eventsParameters));
		}

		private void OnPhaseChange(BombScoreboardState state)
		{
			if (state == BombScoreboardState.PreBomb && this._events.OnRoundPreStartBlock != null)
			{
				base.TriggerEvent(this._events.OnRoundPreStartBlock.Id);
			}
			if (state == BombScoreboardState.BombDelivery && this._events.OnRoundStartBlock != null)
			{
				base.TriggerEvent(this._events.OnRoundStartBlock.Id);
			}
			if (state == BombScoreboardState.Replay && this._events.OnRoundEndBlock != null)
			{
				base.TriggerEvent(this._events.OnRoundEndBlock.Id);
			}
			if (state == BombScoreboardState.EndGame)
			{
				this.UnsubscribeEvents(this._events);
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

		public override void ForcePressed()
		{
			this.TriggerPressed();
		}

		public override void ForceReleased()
		{
			this.TriggerReleased();
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

		public void PopulateSubgadgetEvents()
		{
			this._subGadgetEvents = new Dictionary<int, IList<IBlock>>();
			if (this._subgadgets == null || this._subgadgets.Length == 0)
			{
				return;
			}
			List<KeyValuePair<IBlock, IBlock>> list = new List<KeyValuePair<IBlock, IBlock>>();
			for (int i = 0; i < this._subgadgets.Length; i++)
			{
				list.AddRange(this._subgadgets[i].Events);
			}
			for (int j = 0; j < list.Count; j++)
			{
				IList<IBlock> list2;
				if (!this._subGadgetEvents.TryGetValue(list[j].Key.Id, out list2))
				{
					list2 = new List<IBlock>();
					this._subGadgetEvents.Add(list[j].Key.Id, list2);
				}
				list2.Add(list[j].Value);
			}
		}

		protected override Queue<BaseBlock> _blocksToInitialize
		{
			get
			{
				Queue<BaseBlock> queue = new Queue<BaseBlock>();
				queue.Enqueue(this._events.OnPressedBlock);
				queue.Enqueue(this._events.OnCombatDeathBlock);
				queue.Enqueue(this._events.OnReleasedBlock);
				queue.Enqueue(this._events.OnCombatSpawnBlock);
				queue.Enqueue(this._events.OnRoundEndBlock);
				queue.Enqueue(this._events.OnRoundStartBlock);
				queue.Enqueue(this._events.OnModifierAppliedBlock);
				queue.Enqueue(this._events.OnModifierRemovedBlock);
				return queue;
			}
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
		private CombatGadget.GadgetEventConfig _events;

		[SerializeField]
		private CombatGadget.OnPlayerStatusParameters _onPlayerStatusParameters;

		[FormerlySerializedAs("_onModifierAppliedEventParameters")]
		[SerializeField]
		private CombatGadget.OnModifierEventParameters _onModifierEventParameters;

		[SerializeField]
		private SubGadget[] _subgadgets;

		[SerializeField]
		private List<CombatGadget.ParameterGadgetContextRoute> _parametersFromOtherGadgets;

		[SerializeField]
		private List<CombatGadget.NamedParameter> _UIParameters;

		[SerializeField]
		private List<MasterVFX> _precacheObjects;

		private IParameter<float> _cooldownEndTime;

		private IParameter<float> _cooldownTotalTime;

		private CombatObject _combat;

		private bool _pressed;

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

			public BaseBlock OnRoundPreStartBlock;

			public BaseBlock OnRoundStartBlock;

			public BaseBlock OnRoundEndBlock;

			public BaseBlock OnModifierAppliedBlock;

			public BaseBlock OnModifierRemovedBlock;
		}

		[Serializable]
		private class OnPlayerStatusParameters
		{
			public bool HasCombatParameter { get; private set; }

			public bool HasCauserParameter { get; private set; }

			public void Initialize()
			{
				this.HasCombatParameter = (this.Combat != null);
				this.HasCauserParameter = (this.Causer != null);
			}

			public CombatObjectParameter Combat;

			public CombatObjectParameter Causer;
		}

		[Serializable]
		private class OnModifierEventParameters
		{
			public bool HasModifierParameter { get; private set; }

			public bool HasCauserParameter { get; private set; }

			public bool HasTargetParameter { get; private set; }

			public bool HasAmountParameter { get; private set; }

			public void Initialize()
			{
				this.HasModifierParameter = (this.Modifier != null);
				this.HasCauserParameter = (this.Causer != null);
				this.HasTargetParameter = (this.Target != null);
				this.HasAmountParameter = (this.Amount != null);
			}

			public ModifierDataParameter Modifier;

			public CombatObjectParameter Causer;

			public CombatObjectParameter Target;

			public BaseParameter Amount;
		}

		[Serializable]
		private class NamedParameter
		{
			public string Name;

			public BaseParameter Parameter;
		}
	}
}
