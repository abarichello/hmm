using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class SoundBlasterVFX : BaseVFX
	{
		protected void Awake()
		{
			this.WaveGameObject.SetActive(false);
		}

		protected void Start()
		{
		}

		protected void Stop()
		{
		}

		protected void LateUpdate()
		{
			if (this._effectTransform)
			{
			}
		}

		protected override void OnActivate()
		{
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			this.WaveGameObject.SetActive(false);
			this.WaveGameObject.transform.localScale = this._origScale;
			this._effectTransform = null;
		}

		public GameObject WaveGameObject;

		protected float _gadgetRange;

		protected Vector3 _origScale;

		protected Transform _effectTransform;

		private Vector3 _lastPosition;
	}
}
