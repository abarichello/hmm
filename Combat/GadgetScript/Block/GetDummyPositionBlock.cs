using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using NaughtyAttributes;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public class GetDummyPositionBlock : BaseBlock
	{
		public bool IsCustomDummy()
		{
			return this._dummy == CDummy.DummyKind.Custom;
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			ICombatObject value = this._combatObject.GetValue(gadgetContext);
			Transform dummy = value.Dummy.GetDummy(this._dummy, this._customDummyName, null);
			this._dummyPosition.SetValue(gadgetContext, dummy.position);
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _combatObject;

		[SerializeField]
		private CDummy.DummyKind _dummy;

		[SerializeField]
		[EnableIf("IsCustomDummy")]
		private string _customDummyName;

		[Header("Write")]
		[SerializeField]
		private Vector3Parameter _dummyPosition;
	}
}
