using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Modifier/ApplyModifier")]
	public sealed class ApplyModifiersBlock : BaseApplyModifierBlock
	{
		protected override void ApplyModifier(ModifierData data, IHMMGadgetContext gadgetContext, ICombatController target)
		{
			if (this._direction != null)
			{
				data.SetDirection(this._direction.GetValue<Vector3>(gadgetContext));
			}
			if (this._position != null)
			{
				data.SetPosition(this._position.GetValue<Vector3>(gadgetContext));
			}
			bool barrierHit = this._isBarrier != null && this._isBarrier.GetValue<bool>(gadgetContext);
			target.AddModifier(data, gadgetContext.Owner as ICombatObject, -1, barrierHit);
		}

		[Restrict(false, new Type[]
		{
			typeof(bool)
		})]
		[SerializeField]
		private BaseParameter _isBarrier;

		[Restrict(false, new Type[]
		{
			typeof(Vector3)
		})]
		[SerializeField]
		private BaseParameter _direction;

		[Restrict(false, new Type[]
		{
			typeof(Vector3)
		})]
		[SerializeField]
		private BaseParameter _position;
	}
}
