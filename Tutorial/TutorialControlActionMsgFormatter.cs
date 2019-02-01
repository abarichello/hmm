using System;
using System.Collections.Generic;
using HeavyMetalMachines.Options;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial
{
	internal static class TutorialControlActionMsgFormatter
	{
		public static string Format(string format, ControlAction[] args)
		{
			if (args == null || args.Length == 0)
			{
				return format;
			}
			string[] args2 = TutorialControlActionMsgFormatter.LocalizeArgs(args);
			return string.Format(format, args2);
		}

		public static string[] LocalizeArgs(ControlAction[] args)
		{
			List<string> list = new List<string>();
			foreach (ControlAction controlAction in args)
			{
				string item = string.Empty;
				if (controlAction == ControlAction.None)
				{
					TutorialControlActionMsgFormatter.Log.Warn("Can't format empty control action.");
				}
				else
				{
					item = ControlOptions.GetNGUIIconOrTextLocalized(controlAction, ControlOptions.ControlActionInputType.Primary);
				}
				list.Add(item);
			}
			return list.ToArray();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(TutorialControlActionMsgFormatter));
	}
}
