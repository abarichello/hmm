using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class DisableEnableObjectsOnAwake : MonoBehaviour
	{
		private void Awake()
		{
			if (this.DisableObjects != null)
			{
				for (int i = 0; i < this.DisableObjects.Length; i++)
				{
					GameObject gameObject = this.DisableObjects[i];
					gameObject.SetActive(false);
				}
			}
			if (this.EnableObjects != null)
			{
				for (int j = 0; j < this.EnableObjects.Length; j++)
				{
					GameObject gameObject2 = this.EnableObjects[j];
					gameObject2.SetActive(true);
				}
			}
		}

		public GameObject[] DisableObjects;

		public GameObject[] EnableObjects;
	}
}
