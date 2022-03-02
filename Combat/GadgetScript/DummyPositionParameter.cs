using System;
using System.Collections.Generic;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Operation/DummyPosition")]
	public class DummyPositionParameter : Vector3Parameter
	{
		protected override void Initialize()
		{
			base.Initialize();
			this._combatObjectParameter.OnParameterValueUpdated += this.OnCombatChanged;
		}

		private void OnCombatChanged(object context)
		{
			if (this._routedContexts.Contains(context))
			{
				return;
			}
			this._routedContexts.Add(context);
			ICombatObject co = this._combatObjectParameter.GetValue(context);
			base.SetRoute(context, (object c) => co.Dummy.GetDummy(this._dummyKind, this._customDummyName, null).position, new Action<object, Vector3>(base.NullSet));
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
		}

		[SerializeField]
		private CombatObjectParameter _combatObjectParameter;

		[SerializeField]
		private CDummy.DummyKind _dummyKind;

		[SerializeField]
		private string _customDummyName;

		private HashSet<object> _routedContexts = new HashSet<object>();
	}
}
