using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GUIButtonActivate : GameHubBehaviour
	{
		private void OnClick()
		{
			if (this.target != null)
			{
				this.SetActive(this.target);
			}
		}

		public void SetActive(GameObject go)
		{
			if (go)
			{
				if (go.activeSelf)
				{
					go.SetActive(false);
				}
				else
				{
					go.SetActive(true);
				}
			}
		}

		public GameObject target;
	}
}
