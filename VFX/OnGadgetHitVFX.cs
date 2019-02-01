using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class OnGadgetHitVFX : BaseVFX
	{
		private void Update()
		{
			if (this.timeToRelease < Time.timeSinceLevelLoad)
			{
				this.EndLifetime();
			}
		}

		protected override void OnActivate()
		{
			this.hit = false;
			this.timeToRelease = this.releaseMaxTime + Time.timeSinceLevelLoad;
			base.enabled = true;
			this.CanCollectToCache = false;
			if (!this._targetFXInfo.Owner)
			{
				OnGadgetHitVFX.Log.WarnFormat("Owner null on {0}", new object[]
				{
					base.name
				});
				return;
			}
			if (!this._targetFXInfo.Gadget)
			{
				OnGadgetHitVFX.Log.WarnFormat("Gadget null on {0}", new object[]
				{
					base.name
				});
				return;
			}
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId);
			if (playerOrBotsByObjectId != null)
			{
				this.OwnerFound(playerOrBotsByObjectId);
			}
		}

		protected virtual void OwnerFound(PlayerData owner)
		{
			CombatObject component = owner.CharacterInstance.GetComponent<CombatObject>();
			switch (this._targetFXInfo.Gadget.Slot)
			{
			case GadgetSlot.CustomGadget0:
				this.watchinGadgetStateObject = component.GadgetStates.G0StateObject;
				break;
			case GadgetSlot.CustomGadget1:
				this.watchinGadgetStateObject = component.GadgetStates.G1StateObject;
				break;
			case GadgetSlot.CustomGadget2:
				this.watchinGadgetStateObject = component.GadgetStates.G2StateObject;
				break;
			case GadgetSlot.BoostGadget:
				this.watchinGadgetStateObject = component.GadgetStates.GBoostStateObject;
				break;
			case GadgetSlot.PassiveGadget:
				this.watchinGadgetStateObject = component.GadgetStates.GPStateObject;
				break;
			}
			if (this.watchinGadgetStateObject != null)
			{
				this.watchinGadgetStateObject.ClientListenToGadgetHit += this.ListenToGadgetHit;
			}
		}

		private void ListenToGadgetHit(int otherId)
		{
			this.GadgetHitted();
		}

		protected virtual void GadgetHitted()
		{
			this.hit = true;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			this.EndLifetime();
		}

		protected virtual void EndLifetime()
		{
			base.enabled = false;
			this.CanCollectToCache = true;
			this.timeToRelease = float.MaxValue;
			if (this.watchinGadgetStateObject != null)
			{
				this.watchinGadgetStateObject.ClientListenToGadgetHit -= this.ListenToGadgetHit;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(OnGadgetHitVFX));

		public float releaseMaxTime = 10f;

		private float timeToRelease = float.MaxValue;

		private GadgetData.GadgetStateObject watchinGadgetStateObject;

		protected bool hit;
	}
}
