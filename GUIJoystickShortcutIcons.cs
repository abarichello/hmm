using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class GUIJoystickShortcutIcons : GameHubScriptableObject
	{
		public Sprite GetJoystickShortcutIcon(string key)
		{
			if (key.Contains("Axi"))
			{
				for (int i = 0; i < this.Axis.Length; i++)
				{
					for (int j = 0; j < this.Axis[i].shortcutstrings.Length; j++)
					{
						if (this.Axis[i].shortcutstrings[j].Contains(key))
						{
							return this.Axis[i].icon;
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < this.Buttons.Length; k++)
				{
					for (int l = 0; l < this.Buttons[k].shortcutstrings.Length; l++)
					{
						if (this.Buttons[k].shortcutstrings[l].Contains(key))
						{
							return this.Buttons[k].icon;
						}
					}
				}
			}
			return this.ErrorIcon;
		}

		public GUIJoystickShortcutIcons.Shortcut[] Buttons;

		public GUIJoystickShortcutIcons.Shortcut[] Axis;

		public Sprite ErrorIcon;

		[Serializable]
		public class Shortcut
		{
			public string Name;

			public string[] shortcutstrings;

			public Sprite icon;
		}
	}
}
