using System;
using System.Collections.Generic;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial
{
	internal static class TutorialControlActionMsgFormatter
	{
		public static string Format(IInputTranslation inputTranslation, string format, ControllerInputActions[] args)
		{
			if (args == null || args.Length == 0)
			{
				return format;
			}
			string[] args2 = TutorialControlActionMsgFormatter.LocalizeArgs(inputTranslation, args);
			return Language.Format(format, args2);
		}

		private static string[] LocalizeArgs(IInputTranslation inputTranslation, ControllerInputActions[] args)
		{
			List<string> list = new List<string>();
			foreach (ControllerInputActions controllerInputActions in args)
			{
				string item = string.Empty;
				if (controllerInputActions == -1)
				{
					TutorialControlActionMsgFormatter.Log.Warn("Can't format empty control action.");
				}
				else
				{
					ISprite sprite;
					string text;
					inputTranslation.TryToGetInputActionKeyboardMouseAssetOrFallbackToTranslation(controllerInputActions, ref sprite, ref text);
					item = text;
				}
				list.Add(item);
			}
			return list.ToArray();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(TutorialControlActionMsgFormatter));
	}
}
