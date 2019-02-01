using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class InterfacePingTester : MonoBehaviour
	{
		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				this.From.position = this.To.position;
				this.To.position = UICamera.currentCamera.ScreenToWorldPoint(UICamera.lastEventPosition);
				this._interfacePing.ExecutePing(this.From.position, this.To.position);
			}
		}

		public Transform From;

		public Transform To;

		[SerializeField]
		private InterfacePing _interfacePing;
	}
}
