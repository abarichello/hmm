using System;

namespace HeavyMetalMachines.PlayModes.Business
{
	public class PlayModesNavegabilityResult
	{
		public static PlayModesNavegabilityResult CreateFromBooleanValue(bool canJoin, PlayModesNavegabilityReason reasonIfFalse)
		{
			if (canJoin)
			{
				return PlayModesNavegabilityResult.CreateAsJoinable();
			}
			return PlayModesNavegabilityResult.CreateAsUnjoinable(reasonIfFalse);
		}

		public static PlayModesNavegabilityResult CreateAsJoinable()
		{
			return new PlayModesNavegabilityResult
			{
				CanNavigate = true,
				Reasons = null
			};
		}

		public static PlayModesNavegabilityResult CreateAsUnjoinable(PlayModesNavegabilityReason reason)
		{
			return new PlayModesNavegabilityResult
			{
				CanNavigate = false,
				Reasons = new PlayModesNavegabilityReason[]
				{
					reason
				}
			};
		}

		public bool CanNavigate;

		public PlayModesNavegabilityReason[] Reasons;
	}
}
