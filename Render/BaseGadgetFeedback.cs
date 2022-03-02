using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class BaseGadgetFeedback : GameHubBehaviour
	{
		public bool SetPrevizIsAlly
		{
			set
			{
				this.previzIsAlly = value;
			}
		}

		protected void Awake()
		{
			if (!GameHubBehaviour.Hub)
			{
				this.previzMode = true;
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				Object.Destroy(this);
			}
		}

		protected virtual void Start()
		{
			this.InitializeGadgetFeedback();
			if (this.combatObject == null)
			{
				base.enabled = false;
			}
		}

		protected virtual void InitializeGadgetFeedback()
		{
			if (this.combatObject == null)
			{
				this.combatObject = base.GetComponentInParent<CombatObject>();
				if (this.combatObject == null || this.combatObject.Combat == null)
				{
					base.enabled = false;
					return;
				}
				if (this.combatObject.Data == null || this.combatObject.Combat == null || this.combatObject.Combat.GadgetStates == null || this.combatObject.CustomGadget0 == null || this.combatObject.CustomGadget1 == null || this.combatObject.CustomGadget2 == null)
				{
					this.combatObject = null;
					return;
				}
				this.gadgetState = this.combatObject.Combat.GadgetStates.GetGadgetState(this.slot);
				if (this.gadgetState == null)
				{
					base.enabled = false;
					BaseGadgetFeedback.Log.ErrorFormat("Failed to get gadget state. Slot: {0} GameObject: {1}", new object[]
					{
						this.slot,
						base.gameObject.name
					});
					return;
				}
				this.InitializeValueCondition();
				this.InitializeNewGadgetParameterCondition();
				if (this.checkForMana && !this.HasEnoughMana())
				{
					this.OnDeactivate();
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (this.CheckValueFromGadget == GadgetSlot.None || GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			if (this._valueGadgetStateObject != null)
			{
				this._valueGadgetStateObject.ListenToValueChange -= this.GadgetStateListenToValueChange;
			}
			this._valueGadgetStateObject = null;
		}

		private void InitializeNewGadgetParameterCondition()
		{
			if (this._gadgetParameters == null)
			{
				this._gadgetParameters = new PublicParameterComparisonFloat[0];
			}
			for (int i = 0; i < this._gadgetParameters.Length; i++)
			{
				PublicParameterComparisonFloat publicParameterComparisonFloat = this._gadgetParameters[i];
				CombatGadget combatGadget = (CombatGadget)this.combatObject.GetGadgetContext((int)publicParameterComparisonFloat.ParameterGadget);
				if (null != combatGadget)
				{
					publicParameterComparisonFloat.Initialize(combatGadget);
				}
			}
		}

		private void InitializeValueCondition()
		{
			this._valueConditionMet = true;
			if (this.CheckValueFromGadget == GadgetSlot.None)
			{
				return;
			}
			this._valueGadgetStateObject = this.combatObject.GadgetStates.GetGadgetState(this.CheckValueFromGadget);
			this._valueGadgetStateObject.ListenToValueChange += this.GadgetStateListenToValueChange;
			this.UpdateGadgetValueCondition(this._valueGadgetStateObject.Value);
			this._valueConditionChanged = true;
		}

		private void GadgetStateListenToValueChange(int value)
		{
			bool valueConditionMet = this._valueConditionMet;
			this.UpdateGadgetValueCondition(value);
			this._valueConditionChanged = (valueConditionMet != this._valueConditionMet);
		}

		private void UpdateGadgetValueCondition(int value)
		{
			this._valueConditionMet = (value == this.RequiredGadgetValue);
		}

		protected virtual void OnEnable()
		{
			if (!this.combatObject)
			{
				return;
			}
			if (this.effectStateBased)
			{
				if (this.previousEffectState == this.effectActivateState)
				{
					this.OnActivate();
				}
			}
			else if (this.slot != GadgetSlot.CustomGadget2)
			{
				if (this.previousState == this.activateState)
				{
					this.OnActivate();
				}
			}
			else if (this.HasEnoughMana())
			{
				this.OnActivate();
			}
			else if (this.checkForMana)
			{
				this.OnDeactivate();
			}
		}

		protected bool HasEnoughMana()
		{
			bool result = true;
			GadgetSlot gadgetSlot = this.slot;
			if (gadgetSlot != GadgetSlot.CustomGadget0)
			{
				if (gadgetSlot != GadgetSlot.CustomGadget1)
				{
					if (gadgetSlot == GadgetSlot.CustomGadget2)
					{
						result = (this.combatObject.Data.EP >= (float)this.combatObject.CustomGadget2.Info.ActivationCost);
					}
				}
				else
				{
					result = (this.combatObject.Data.EP >= (float)this.combatObject.CustomGadget1.Info.ActivationCost);
				}
			}
			else
			{
				result = (this.combatObject.Data.EP >= (float)this.combatObject.CustomGadget0.Info.ActivationCost);
			}
			return result;
		}

		private bool CheckMana()
		{
			return !this.checkForMana || this.HasEnoughMana();
		}

		protected void LateUpdate()
		{
			if (this.combatObject == null)
			{
				return;
			}
			this.UpdateImpl();
			if (this._gadgetParameters.Length > 0)
			{
				this.CheckActivationBasedOnNewGadgetParameter(false);
			}
			else if (this.effectStateBased)
			{
				this.previousEffectState = (EffectState)this.CheckActivation((int)this.previousEffectState, (int)this.gadgetState.EffectState, (int)this.effectActivateState, (int)this.effectDeactivateState);
			}
			else if (this.slot == GadgetSlot.CustomGadget2)
			{
				this.CheckUltimateActivation();
			}
			else
			{
				this.previousState = (GadgetState)this.CheckActivation((int)this.previousState, (int)this.gadgetState.GadgetState, (int)this.activateState, (int)this.deactivateState);
			}
			this._valueConditionChanged = false;
		}

		private void CheckActivationBasedOnNewGadgetParameter(bool forceChange = false)
		{
			bool flag = true;
			for (int i = 0; i < this._gadgetParameters.Length; i++)
			{
				if (this._gadgetParameters[i].IsValidParameter)
				{
					flag &= this._gadgetParameters[i].Compare(null);
					if (!flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				if (!this._active)
				{
					this.OnActivate();
				}
				if (this.gadgetState.GadgetState == this.activateState)
				{
					this.previousState = this.gadgetState.GadgetState;
				}
				if (this.gadgetState.GadgetState == this.deactivateState)
				{
					this.previousState = this.deactivateState;
				}
			}
			else if (this._active)
			{
				this.OnDeactivate();
			}
		}

		private int CheckActivation(int prevState, int currentState, int activationState, int deactivationState)
		{
			if (prevState == currentState && !this._valueConditionChanged)
			{
				return prevState;
			}
			if (currentState == activationState && this.CheckMana())
			{
				if (!this._active && this._valueConditionMet)
				{
					this.OnActivate();
				}
				prevState = currentState;
			}
			if (currentState == deactivationState || !this._valueConditionMet)
			{
				if (this._active)
				{
					this.OnDeactivate();
				}
				prevState = currentState;
			}
			return prevState;
		}

		private void CheckUltimateActivation()
		{
			bool flag = this.CheckMana();
			if (flag == this.previousMana && !this._valueConditionChanged)
			{
				return;
			}
			if (flag && this._valueConditionMet)
			{
				if (!this._active)
				{
					this.OnActivate();
				}
			}
			else if (this._active)
			{
				this.OnDeactivate();
			}
			this.previousMana = flag;
		}

		protected virtual void UpdateImpl()
		{
		}

		protected virtual void OnActivate()
		{
			this._active = true;
		}

		protected virtual void OnDeactivate()
		{
			this._active = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BaseGadgetFeedback));

		public GadgetSlot slot;

		public GadgetState activateState = GadgetState.Ready;

		public GadgetState deactivateState = GadgetState.Cooldown;

		public bool effectStateBased;

		public EffectState effectActivateState;

		public EffectState effectDeactivateState = EffectState.Running;

		public bool checkForMana;

		[SerializeField]
		protected GadgetSlot CheckValueFromGadget;

		[SerializeField]
		protected int RequiredGadgetValue;

		[Header("[New Gadgets]")]
		[SerializeField]
		private PublicParameterComparisonFloat[] _gadgetParameters;

		private CombatGadget _combatGadget;

		protected GadgetData.GadgetStateObject gadgetState;

		protected GadgetState previousState;

		protected EffectState previousEffectState;

		protected CombatObject combatObject;

		protected bool previousMana;

		protected bool _active;

		private GadgetData.GadgetStateObject _valueGadgetStateObject;

		private bool _valueConditionMet;

		private bool _valueConditionChanged;

		protected bool previzMode;

		protected bool previzIsAlly;
	}
}
