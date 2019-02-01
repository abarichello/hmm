using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class AnimationPositionWrapper : MonoBehaviour
	{
		public void Awake()
		{
			this._localTransform = base.transform;
		}

		public void OnEnable()
		{
			this.Update();
		}

		public void Update()
		{
			Vector3 localPosition = this._localTransform.localPosition;
			if (this.UseX)
			{
				localPosition.x = this.Position.x;
			}
			if (this.UseY)
			{
				localPosition.y = this.Position.y;
			}
			if (this.UseZ)
			{
				localPosition.z = this.Position.z;
			}
			this._localTransform.localPosition = localPosition;
		}

		private Transform _localTransform;

		public Vector3 Position;

		public bool UseX;

		public bool UseY;

		public bool UseZ;
	}
}
