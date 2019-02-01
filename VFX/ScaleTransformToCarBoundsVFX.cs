using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(AttachVFX))]
	public class ScaleTransformToCarBoundsVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			Bounds lhs;
			this.GetScaledBounds(out lhs);
			if (lhs == default(Bounds))
			{
				return;
			}
			this.TargetTransform.localPosition = lhs.center;
			this.TargetTransform.localScale = lhs.size;
		}

		private void GetScaledBounds(out Bounds attachedBounds)
		{
			this.GetAttachedCarBounds(out attachedBounds);
			if (this._normalized)
			{
				this.NormalizeBoundsByMaxSize(ref attachedBounds);
			}
			if (this._useCarIndicatorCenter)
			{
				this.SetCarIndicatorCenter(ref attachedBounds);
			}
			this.OverrideScale(ref attachedBounds);
		}

		private void GetAttachedCarBounds(out Bounds attachedBounds)
		{
			AttachVFX.AttachType attachType = this._attachVfx.attachType;
			if (attachType != AttachVFX.AttachType.EffectOwner)
			{
				if (attachType != AttachVFX.AttachType.EffectTarget)
				{
					ScaleTransformToCarBoundsVFX.Log.ErrorFormat("Invalid AttachType: '{0}'", new object[]
					{
						this._attachVfx.attachType
					});
					attachedBounds = default(Bounds);
				}
				else
				{
					attachedBounds = this._targetFXInfo.Target.GetComponentHub<CarComponentHub>().ArtReference.Bounds;
				}
			}
			else
			{
				attachedBounds = this._targetFXInfo.Owner.GetComponentHub<CarComponentHub>().ArtReference.Bounds;
			}
		}

		private void NormalizeBoundsByMaxSize(ref Bounds bounds)
		{
			float num = bounds.size.x;
			if (bounds.size.y > num)
			{
				num = bounds.size.y;
			}
			if (bounds.size.z > num)
			{
				num = bounds.size.z;
			}
			float y = (!this._dontNormalizeYAxis) ? num : bounds.size.y;
			bounds.size = new Vector3(num, y, num);
		}

		private void OverrideScale(ref Bounds bounds)
		{
			Vector3 size = new Vector3(bounds.size.x, bounds.size.y, bounds.size.z);
			if (this._fixedScale.x > 0f)
			{
				size.x = this._fixedScale.x;
			}
			if (this._fixedScale.y > 0f)
			{
				size.y = this._fixedScale.y;
			}
			if (this._fixedScale.z > 0f)
			{
				size.z = this._fixedScale.z;
			}
			bounds.size = size;
		}

		private void SetCarIndicatorCenter(ref Bounds bounds)
		{
			CarComponentHub componentHub = this._targetFXInfo.Owner.GetComponentHub<CarComponentHub>();
			float playerBorderOffsetPosition = componentHub.Player.Character.IndicatorConfig.PlayerBorderOffsetPosition;
			bounds.center = this._targetFXInfo.Owner.transform.forward * playerBorderOffsetPosition;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		private void OnValidate()
		{
			this._attachVfx = base.GetComponent<AttachVFX>();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ScaleTransformToCarBoundsVFX));

		[SerializeField]
		[Tooltip("The transform that should be scaled according to the car bounds")]
		protected Transform TargetTransform;

		[HideInInspector]
		[SerializeField]
		private AttachVFX _attachVfx;

		[SerializeField]
		[Tooltip("Normalize the car bounds by the highest bound (to make a cube).")]
		private bool _normalized;

		[SerializeField]
		[Tooltip("If normalized is true, won't normalize the Y (vertical) axis. This way, the effect won't scale vertically.")]
		private bool _dontNormalizeYAxis;

		[SerializeField]
		[Tooltip("Will use a fixed scale for the axis not set to zero. This setting is have priority over Normalized setting (will override it).")]
		private Vector3 _fixedScale = Vector3.zero;

		[SerializeField]
		[Tooltip("Use car indicator central position to display the effect (only available for car targets).")]
		private bool _useCarIndicatorCenter;
	}
}
