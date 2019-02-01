using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class UICheckBoxChecker : GameHubBehaviour
	{
		public void UnCheck(int idtonotuncheck)
		{
			UIToggle[] componentsInChildren = base.gameObject.GetComponentsInChildren<UIToggle>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.GetComponent<GUIEventListener>().IntParameter != idtonotuncheck)
				{
					componentsInChildren[i].value = false;
				}
				else
				{
					componentsInChildren[i].value = true;
				}
			}
		}
	}
}
