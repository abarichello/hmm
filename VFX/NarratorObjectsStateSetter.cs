using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class NarratorObjectsStateSetter : MonoBehaviour
	{
		private void OnEnable()
		{
			SpectatorController.EvtSpectatorRoleChanged += this.OnSpectatorRoleChanged;
			this.RefreshSpectatorSettings();
		}

		private void OnDisable()
		{
			SpectatorController.EvtSpectatorRoleChanged -= this.OnSpectatorRoleChanged;
		}

		private void OnSpectatorRoleChanged(SpectatorRole updatedSpectatorRole)
		{
			this.RefreshSpectatorSettings();
		}

		private void RefreshSpectatorSettings()
		{
			if (SpectatorController.IsSpectating)
			{
				this.SetActive(this._narratorGameObjects, true);
				this.SetActive(this._nonNarratorGameObjects, false);
				return;
			}
			this.SetActive(this._narratorGameObjects, false);
		}

		private void SetActive(GameObject[] gameObjects, bool active)
		{
			if (gameObjects == null)
			{
				return;
			}
			for (int i = 0; i < gameObjects.Length; i++)
			{
				if (!(gameObjects[i] == null))
				{
					gameObjects[i].gameObject.SetActive(active);
				}
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(NarratorObjectsStateSetter));

		[SerializeField]
		private GameObject[] _narratorGameObjects;

		[SerializeField]
		private GameObject[] _nonNarratorGameObjects;
	}
}
