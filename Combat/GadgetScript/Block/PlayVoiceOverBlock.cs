using System;
using HeavyMetalMachines.Combat.Gadget.GadgetScript;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetaMachines.Audio;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Audio/PlayVoiceOver")]
	public class PlayVoiceOverBlock : BaseBlock
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
				MatchClient sourceMatchClient = this.GetSourceMatchClient(ihmmcombatGadgetContext);
				gadgetContext.InjectionResolver.Resolve<IPlaySocialVoiceOver>().PlayMessage(this._voiceOverEvent, sourceMatchClient);
			}
			return this._nextBlock;
		}

		private MatchClient GetSourceMatchClient(IHMMCombatGadgetContext hmmCombatGadgetContext)
		{
			int objectId = this.GetObjectId(hmmCombatGadgetContext);
			MatchPlayers matchPlayers = hmmCombatGadgetContext.InjectionResolver.Resolve<MatchPlayers>();
			PlayerData playerOrBotsByObjectId = matchPlayers.GetPlayerOrBotsByObjectId(objectId);
			return playerOrBotsByObjectId.ToMatchClient();
		}

		private int GetObjectId(IHMMCombatGadgetContext hmmCombatGadgetContext)
		{
			if (this._voiceOverSource == null)
			{
				return hmmCombatGadgetContext.Owner.Identifiable.ObjId;
			}
			return this._voiceOverSource.GetValue(hmmCombatGadgetContext).Identifiable.ObjId;
		}

		[SerializeField]
		private CombatObjectParameter _voiceOverSource;

		[SerializeField]
		private VoiceOverEventGroup _voiceOverEvent;
	}
}
