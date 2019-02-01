using System;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	[Serializable]
	public class TutorialBusinessStepBiLogger : GameHubBehaviour
	{
		public void LogStep(InGameTutorialStep step, string tutorialVersion)
		{
			TutorialBusinessStepBiLogger.TutorialBusinessStep tutorialBusinessStep = this.GetTutorialBusinessStep(step);
			if (tutorialBusinessStep == null)
			{
				return;
			}
			TutorialBusinessStepName name = tutorialBusinessStep.Name;
			string universalID = GameHubBehaviour.Hub.Players.Players[0].UserSF.UniversalID;
			GameHubBehaviour.Hub.Swordfish.Log.BILogServerMsg(ServerBITags.GameServerTutorialBusinessStepStart, string.Format("Info={{\"StepName\":\"{0}\", \"StepNumber\":{1}, \"UserID\":\"{2}\", \"TutorialVersion\":\"{3}\", \"ReleaseVersion\":\"{4}\"}}", new object[]
			{
				name,
				(int)name,
				universalID,
				tutorialVersion,
				"2.07.972"
			}), false);
		}

		private TutorialBusinessStepBiLogger.TutorialBusinessStep GetTutorialBusinessStep(InGameTutorialStep step)
		{
			for (int i = 0; i < this.TutorialBusinessStepForBiLogs.Length; i++)
			{
				TutorialBusinessStepBiLogger.TutorialBusinessStep tutorialBusinessStep = this.TutorialBusinessStepForBiLogs[i];
				if (tutorialBusinessStep.Step == step)
				{
					return tutorialBusinessStep;
				}
			}
			return null;
		}

		[HideInInspector]
		public TutorialBusinessStepBiLogger.TutorialBusinessStep[] TutorialBusinessStepForBiLogs;

		[Serializable]
		public class TutorialBusinessStep
		{
			public TutorialBusinessStepName Name;

			public InGameTutorialStep Step;
		}
	}
}
