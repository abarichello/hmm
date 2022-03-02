using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMatchmakingMatchAcceptView
	{
		IActivatable MainGroup { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IActivatable PlayerDeclinedMessage { get; }

		IButton AcceptButton { get; }

		IButton DeclineButton { get; }

		IActivatable AcceptButtonParent { get; }

		IActivatable DeclineButtonParent { get; }

		IProgressBar TimeoutProgressBar { get; }

		ILabel TimeoutLabel { get; }

		void SetPlayerIndexAsAccepted(int playerIndex);

		void SetPlayerIndexAsDeclined(int playerIndex);

		void DeactivateAllPlayerIndicators();
	}
}
