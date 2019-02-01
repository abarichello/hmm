using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemsTab : GameHubBehaviour
	{
		public void Select()
		{
			this.onGameobject.SetActive(true);
			this.offGameobject.SetActive(false);
		}

		public void Deselect()
		{
			this.onGameobject.SetActive(false);
			this.offGameobject.SetActive(true);
		}

		public void SetListener(GameObject pGameObject, GUIEventListener.ParameterKind pParameterKind, int pIndex)
		{
			foreach (GUIEventListener guieventListener in base.GetComponentsInChildren<GUIEventListener>())
			{
				guieventListener.EventListener = pGameObject;
				guieventListener.TheParameterKind = pParameterKind;
				guieventListener.IntParameter = pIndex;
			}
		}

		public UILabel onLabel;

		public UILabel offLabel;

		public GameObject onGameobject;

		public GameObject offGameobject;
	}
}
