using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/OrientatePhysicalObject")]
	public class CombatSwitchBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (ihmmgadgetContext.IsClient)
			{
				return this._nextBlock;
			}
			ICombatObject value = this._target.GetValue<ICombatObject>(gadgetContext);
			if (value == null)
			{
				if (null != this._onIsNotCombatBlock)
				{
					ihmmgadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._onIsNotCombatBlock.Id, ihmmgadgetContext));
				}
				return this._nextBlock;
			}
			if (((CombatObject)value).IsBomb)
			{
				if (null != this._onIsBombBlock)
				{
					ihmmgadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._onIsBombBlock.Id, ihmmgadgetContext));
				}
				return this._nextBlock;
			}
			ICombatObject combatObject = (ICombatObject)ihmmgadgetContext.Owner;
			if (value == combatObject)
			{
				if (null != this._onIsOwnerBlock)
				{
					ihmmgadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._onIsOwnerBlock.Id, ihmmgadgetContext));
				}
				return this._nextBlock;
			}
			if (value.Team == combatObject.Team)
			{
				if (null != this._onIsAllyBlock)
				{
					ihmmgadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._onIsAllyBlock.Id, ihmmgadgetContext));
				}
				return this._nextBlock;
			}
			if (null == this._onIsEnemyBlock)
			{
				return this._nextBlock;
			}
			ihmmgadgetContext.TriggerEvent(GadgetEvent.GetInstance(this._onIsEnemyBlock.Id, ihmmgadgetContext));
			return this._nextBlock;
		}

		[SerializeField]
		private BaseBlock _onIsNotCombatBlock;

		[SerializeField]
		private BaseBlock _onIsBombBlock;

		[SerializeField]
		private BaseBlock _onIsAllyBlock;

		[SerializeField]
		private BaseBlock _onIsOwnerBlock;

		[SerializeField]
		private BaseBlock _onIsEnemyBlock;

		[Header("Read")]
		[SerializeField]
		[Restrict(true, new Type[]
		{
			typeof(ICombatObject)
		})]
		private BaseParameter _target;
	}
}
