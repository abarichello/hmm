using System;
using FMod;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public class PlayOneShotSFXBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMCombatGadgetContext ihmmcombatGadgetContext = (IHMMCombatGadgetContext)gadgetContext;
			if (ihmmcombatGadgetContext.IsServer)
			{
				((IHMMEventContext)eventContext).SendToClient();
			}
			else
			{
				this.Play(gadgetContext);
			}
			return this._nextBlock;
		}

		private void Play(IGadgetContext gadgetContext)
		{
			if (this._source == null)
			{
				FMODAudioManager.PlayOneShotAt(this._audioAsset, Vector3.zero, 0);
				return;
			}
			FMODAudioManager.PlayOneShotAt(this._audioAsset, this._source.GetValue(gadgetContext), 0);
		}

		[SerializeField]
		private Vector3Parameter _source;

		[SerializeField]
		private AudioEventAsset _audioAsset;
	}
}
