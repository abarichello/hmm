using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class RotateWithRestart : GameHubBehaviour
	{
		private void OnEnable()
		{
			base.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}

		private void Update()
		{
			base.transform.Rotate(this.axis * Time.deltaTime * this.rate);
		}

		public Vector3 axis;

		public float rate;
	}
}
