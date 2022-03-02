using System;
using Pocketverse;

namespace HeavyMetalMachines.Utils
{
	public class Debug : GameHubObject
	{
		public static void Assert(bool boCondition, string strMessage, Debug.TargetTeam targetTeam = Debug.TargetTeam.All)
		{
		}

		public static void DelegateCheck(object obj)
		{
		}

		public static void DelegateCheckNull(Delegate testingAction)
		{
		}

		public static readonly BitLogger Log = new BitLogger(typeof(Debug));

		public enum TargetTeam
		{
			None,
			All,
			Sd,
			Hpl,
			Art,
			TechnicalArtist,
			GUI,
			GD
		}
	}
}
