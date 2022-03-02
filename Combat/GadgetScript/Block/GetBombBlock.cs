using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Bomb/GetBomb")]
	public class GetBombBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			this._bombCombatObject.SetValue(gadgetContext, ihmmgadgetContext.Bomb);
			ihmmeventContext.SendToClient();
			return this._nextBlock;
		}

		[Header("Write")]
		[SerializeField]
		private CombatObjectParameter _bombCombatObject;
	}
}
