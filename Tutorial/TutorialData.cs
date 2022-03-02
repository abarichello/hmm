using System;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	[Serializable]
	public class TutorialData
	{
		public TutorialData(int id, string Name)
		{
			this.id = id;
			this.Name = Name;
		}

		internal bool IsObjective()
		{
			return this.dialogType == TutorialData.DialogTypes.Objective;
		}

		[ReadOnly]
		public int id;

		public string Name;

		public string Title;

		public TutorialDataDescription[] Descriptions;

		[Tooltip("Warning description (with optional mouse buttons).")]
		public string DescWarning;

		[Tooltip("If enabled, will show mouse and joystick icons")]
		public bool Redo;

		public bool SaveRightNow;

		public string targetRootName;

		public string targetPath;

		public float ShowDelay;

		public TutorialData.DialogTypes dialogType;

		public bool HasNextWindow;

		public Sprite InformativeSprite;

		public float tutorialGuyLifetime;

		public enum DialogTypes
		{
			TutorialGuy,
			Informative,
			Objective
		}
	}
}
