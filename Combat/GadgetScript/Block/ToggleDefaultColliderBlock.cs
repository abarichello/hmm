using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject")]
	internal class ToggleDefaultColliderBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (((IHMMGadgetContext)gadgetContext).IsClient)
			{
				return this._nextBlock;
			}
			ICombatObject value = this._target.GetValue(gadgetContext);
			if (value == null)
			{
				return this._nextBlock;
			}
			MonoBehaviour monoBehaviour = value.PlayerData as MonoBehaviour;
			monoBehaviour.GetComponent<BoxCollider>().enabled = (this._changeTo == ToggleDefaultColliderBlock.ToggleColliderOption.Enabled);
			return this._nextBlock;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LogBlock));

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _target;

		[SerializeField]
		private ToggleDefaultColliderBlock.ToggleColliderOption _changeTo;

		public enum ToggleColliderOption
		{
			Enabled,
			Disabled
		}
	}
}
