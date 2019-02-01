using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class UIBaseConfigurationSO : GameHubScriptableObject
	{
		private void OnEnable()
		{
			this.AvailableModalGuiList.Clear();
			this.AvailableModalGuiList.AddRange(this._modalGuiPrefabs);
		}

		public bool HideDrawCallObjects;

		public bool SortDrawCallObjectsInHierarchy = true;

		[SerializeField]
		private List<ModalGUIController> _modalGuiPrefabs;

		[NonSerialized]
		public List<ModalGUIController> AvailableModalGuiList = new List<ModalGUIController>();
	}
}
