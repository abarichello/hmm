using System;
using System.Collections;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackBaseVFX : BaseGadgetFeedback
	{
		public void ExternalToggle(bool enable)
		{
			if (enable == this._canBeActivated)
			{
				return;
			}
			this._canBeActivated = enable;
			if (this._canBeActivated)
			{
				if (this._active)
				{
					this.OnActivate();
				}
			}
			else
			{
				this.OnDeactivateInternal();
			}
		}

		protected override void Start()
		{
			base.Start();
			bool flag;
			if (this.previzMode)
			{
				flag = this.previzIsAlly;
			}
			else
			{
				flag = (this.combatObject.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam);
			}
			for (int i = 0; i < this.baseVFXList.Length; i++)
			{
				if (this.baseVFXList[i])
				{
					this.baseVFXList[i].CurrentTeam = ((!flag) ? VFXTeam.Enemy : VFXTeam.Ally);
				}
			}
		}

		protected override void OnActivate()
		{
			base.OnActivate();
			if (!this._canBeActivated)
			{
				return;
			}
			if (this.DelayStart > 0f)
			{
				base.StartCoroutine(this.WaitToActivate());
			}
			else
			{
				this.ActivateBaseVFXs();
			}
			if (this.stopOnTimer > 0f)
			{
				base.StartCoroutine(this.WaitToDeactivate());
			}
		}

		private void ActivateBaseVFXs()
		{
			for (int i = 0; i < this.baseVFXList.Length; i++)
			{
				if (!(this.baseVFXList[i] == null))
				{
					this.baseVFXList[i].Activate(this.fakeTargetInfo);
				}
			}
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			this.OnDeactivateInternal();
		}

		private void OnDeactivateInternal()
		{
			for (int i = 0; i < this.baseVFXList.Length; i++)
			{
				if (!(this.baseVFXList[i] == null))
				{
					this.baseVFXList[i].SignalDeactivation();
				}
			}
		}

		private IEnumerator WaitToActivate()
		{
			yield return new WaitForSeconds(this.DelayStart);
			this.ActivateBaseVFXs();
			yield break;
		}

		private IEnumerator WaitToDeactivate()
		{
			yield return new WaitForSeconds(this.stopOnTimer);
			base.OnDeactivate();
			this.OnDeactivateInternal();
			yield break;
		}

		[SerializeField]
		private BaseVFX[] baseVFXList;

		[SerializeField]
		private float DelayStart;

		[SerializeField]
		private float stopOnTimer;

		protected MasterVFX.TargetFXInfo fakeTargetInfo;

		private bool _canBeActivated = true;
	}
}
