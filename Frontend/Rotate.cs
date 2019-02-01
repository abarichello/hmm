using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class Rotate : GameHubBehaviour
	{
		private void Update()
		{
			base.transform.Rotate(this.axis * Time.deltaTime * this.rate);
		}

		public Vector3 axis;

		public float rate;
	}
}
