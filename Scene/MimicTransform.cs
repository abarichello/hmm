using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Scene
{
	public class MimicTransform : GameHubBehaviour
	{
		private void Start()
		{
			if (!this._originalTransform)
			{
				MimicTransform.Log.WarnFormat("No original transform set on {0}", new object[]
				{
					base.gameObject.name
				});
				base.enabled = false;
			}
		}

		private void Update()
		{
			Vector3 vector = this._originalTransform.position;
			vector.y += this._yPosIncrement;
			base.transform.SetPositionAndRotation(vector, this._originalTransform.rotation);
			if (!this._mimicScaleX && !this._mimicScaleY && !this._mimicScaleZ)
			{
				return;
			}
			vector = base.transform.localScale;
			if (this._mimicScaleX)
			{
				vector.x = this._originalTransform.localScale.x;
			}
			if (this._mimicScaleY)
			{
				vector.y = this._originalTransform.localScale.y;
			}
			if (this._mimicScaleZ)
			{
				vector.z = this._originalTransform.localScale.z;
			}
			base.transform.localScale = vector;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MimicTransform));

		[SerializeField]
		private Transform _originalTransform;

		[SerializeField]
		private float _yPosIncrement;

		[SerializeField]
		private bool _mimicScaleX;

		[SerializeField]
		private bool _mimicScaleY;

		[SerializeField]
		private bool _mimicScaleZ;
	}
}
