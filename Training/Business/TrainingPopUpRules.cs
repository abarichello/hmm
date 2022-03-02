using System;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;

namespace HeavyMetalMachines.Training.Business
{
	public class TrainingPopUpRules : ITrainingPopUpRules
	{
		public TrainingPopUpRules(ILocalPlayerGroup localPlayerGroup, IIsFeatureToggled isFeatureToggled, ICheckNoviceQueueCondition checkNoviceQueueCondition)
		{
			this._localPlayerGroup = localPlayerGroup;
			this._isFeatureToggled = isFeatureToggled;
			this._checkNoviceQueueCondition = checkNoviceQueueCondition;
		}

		public bool CanOpenPopUp()
		{
			return this._isFeatureToggled.Check(Features.OnboardingScreenPopup) && !this._localPlayerGroup.IsPlayerInGroup() && this._checkNoviceQueueCondition.ShouldGoToNoviceQueue();
		}

		private readonly ILocalPlayerGroup _localPlayerGroup;

		private readonly IIsFeatureToggled _isFeatureToggled;

		private readonly ICheckNoviceQueueCondition _checkNoviceQueueCondition;
	}
}
