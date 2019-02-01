using System;
using HeavyMetalMachines.Options;
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
			return this.dialogType == TutorialData.DialogTypes.Objective || this.dialogType == TutorialData.DialogTypes.ObjectiveRight;
		}

		[ReadOnly]
		public int id;

		public string Name;

		public string Tip;

		public string Title;

		public string DescMouse;

		public string DescJoystick;

		[Header("[ControlAction for DescMouse/DescJoystick]")]
		public ControlAction ControlAction1;

		public ControlAction ControlAction2;

		public ControlAction ControlAction3;

		public ControlAction ControlAction4;

		[Tooltip("Use direct joystick code here if needed (i.e. [Y]).")]
		public string DirectControlJoyAction1;

		[Tooltip("Use direct joystick code here if needed (i.e. [Y]).")]
		public string DirectControlJoyAction2;

		[Tooltip("Use direct joystick code here if needed (i.e. [Y]).")]
		public string DirectControlJoyAction3;

		[Tooltip("Use direct joystick code here if needed (i.e. [Y]).")]
		public string DirectControlJoyAction4;

		[Tooltip("Warning description (with optional mouse buttons).")]
		public string DescWarning;

		[Tooltip("If enabled, will show mouse and joystick icons")]
		public bool MouseAndJoystickIcons;

		public bool Redo;

		public bool SaveRightNow;

		public string targetRootName;

		public string targetPath;

		public Vector3 arrowOffsets;

		public float arrowAngles;

		public float ShowDelay;

		public TutorialData.DialogTypes dialogType;

		public bool HasNextWindow;

		public Sprite InformativeSprite;

		public float tutorialGuyLifetime;

		public bool EnableArrowPanel;

		public bool EnableWButton;

		public bool EnableAButton;

		public bool EnableDButton;

		public bool EnableDriftButton;

		public enum DialogTypes
		{
			TutorialGuy,
			Informative,
			Objective,
			ObjectiveRight
		}
	}
}
