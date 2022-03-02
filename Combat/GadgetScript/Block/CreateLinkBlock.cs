using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Physics/CreateLink")]
	public class CreateLinkBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			IPhysicalObject value = this._firstHook.ObjectParameter.GetValue<IPhysicalObject>(gadgetContext);
			IPhysicalObject value2 = this._secondHook.ObjectParameter.GetValue<IPhysicalObject>(gadgetContext);
			ILinkHook point = new CombatLink.LinkHook(((Component)value).transform, value, this._firstHook.GetMass(gadgetContext), this._firstHook.GetMass(gadgetContext));
			ILinkHook point2 = new CombatLink.LinkHook(((Component)value2).transform, value2, this._secondHook.GetMass(gadgetContext), this._secondHook.GetMass(gadgetContext));
			ICombatLink combatLink = new CombatLink(point, point2, this._linkConfiguration);
			bool force = this._forceCreation == null || this._forceCreation.GetValue<bool>(gadgetContext);
			value.AddLink(combatLink, force);
			value2.AddLink(combatLink, force);
			this._combatLinkParameter.SetValue<ICombatLink>(gadgetContext, combatLink);
			if (this._onLinkBrokenBlock != null)
			{
				combatLink.OnLinkBroken += delegate(ICombatLink brokenLink)
				{
					this.OnLinkBroken(gadgetContext, brokenLink);
				};
			}
			return this._nextBlock;
		}

		private void OnLinkBroken(IGadgetContext gadgetContext, ICombatLink link)
		{
			if (this._brokenLinkParameter != null)
			{
				this._brokenLinkParameter.SetValue<ICombatLink>(gadgetContext, link);
			}
			gadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._onLinkBrokenBlock.Id, (IHMMGadgetContext)gadgetContext));
		}

		[SerializeField]
		private BaseBlock _onLinkBrokenBlock;

		[Header("Read")]
		[SerializeField]
		private CreateLinkBlock.HookConfiguration _firstHook;

		[SerializeField]
		private CreateLinkBlock.HookConfiguration _secondHook;

		[Restrict(false, new Type[]
		{
			typeof(bool)
		})]
		[SerializeField]
		private BaseParameter _forceCreation;

		[SerializeField]
		private CombatLink.CombatLinkConfiguration _linkConfiguration;

		[Header("Write")]
		[Restrict(true, new Type[]
		{
			typeof(ICombatLink)
		})]
		[SerializeField]
		private BaseParameter _combatLinkParameter;

		[Restrict(false, new Type[]
		{
			typeof(ICombatLink)
		})]
		[SerializeField]
		private BaseParameter _brokenLinkParameter;

		[Serializable]
		private struct HookConfiguration
		{
			public float GetMass(IGadgetContext gadgetContext)
			{
				if (this.MassParameter != null)
				{
					return this.MassParameter.GetValue<float>(gadgetContext);
				}
				return this.Mass;
			}

			[Restrict(true, new Type[]
			{
				typeof(IPhysicalObject)
			})]
			public BaseParameter ObjectParameter;

			[Restrict(false, new Type[]
			{
				typeof(float)
			})]
			public BaseParameter MassParameter;

			[Tooltip("Only used if Parameter is not set")]
			public float Mass;
		}
	}
}
