using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	public class DisableGameObjectOnTutorial : GameHubBehaviour
	{
		private void Awake()
		{
			this.CheckToDisableObjects();
		}

		private void CheckToDisableObjects()
		{
			bool flag = GameHubBehaviour.Hub.Match.LevelIsTutorial();
			for (int i = 0; i < this.ObjectsToBeDisabled.Count; i++)
			{
				this.ObjectsToBeDisabled[i].SetActive(!flag);
			}
		}

		public List<GameObject> ObjectsToBeDisabled = new List<GameObject>();
	}
}
