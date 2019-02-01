using System;
using HeavyMetalMachines.Character;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class CharacterSelectionGui : MonoBehaviour
	{
		public GameObject CharacterButton;

		public ButtonScriptReference CharacterButtonScriptReference;

		public UIButtonMultiColors PreviewButton;

		public GUIEventListener PreviewEventListener;

		public HeavyMetalMachines.Character.CharacterInfo CharInfo;
	}
}
