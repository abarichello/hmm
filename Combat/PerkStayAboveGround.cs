using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkStayAboveGround : BasePerk, PerkAttachToObject.IEffectAttachListener
	{
		protected override void Awake()
		{
			base.Awake();
			this._myTransform = base.transform;
		}

		public override void PerkInitialized()
		{
			this._speedY = this.BaseSpeedY + this.Effect.Data.MoveSpeed * this.MoveSpeedPercentage;
			this.UpdatePosition();
		}

		private void FixedUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.UpdatePosition();
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			this.UpdatePosition();
		}

		private void UpdatePosition()
		{
			UnityUtils.SnapToGroundPlane(this._myTransform, this.Effect.Data.EffectInfo.Height);
		}

		public void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg)
		{
			this._myTransform = msg.Target;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkStayAboveGround));

		public float BaseSpeedY;

		public float MoveSpeedPercentage;

		private Transform _myTransform;

		private float _speedY;
	}
}
