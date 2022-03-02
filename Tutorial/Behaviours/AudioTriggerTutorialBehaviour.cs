using System;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class AudioTriggerTutorialBehaviour : InGameTutorialBehaviourBase
	{
		private void OnTriggerEnter(Collider coll)
		{
			CombatObject combat = CombatRef.GetCombat(coll);
			if (base.playerController.Combat == combat)
			{
				this.CompleteBehaviourAndSync();
			}
		}

		protected override void OnStepCompletedOnClient()
		{
			FMODAudioManager.PlayAtVolume(this._asset, Vector3.zero, 1f);
			base.OnStepCompletedOnClient();
		}

		[SerializeField]
		private AudioEventAsset _asset;
	}
}
